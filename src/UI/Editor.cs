using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Terminal.Gui;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Root <see cref="Toplevel"/> <see cref="View"/> that is visible on loading the
/// application.  Hooks key and mouse events and mounts as a sub-view whatever file
/// the user opens.
/// </summary>
public class Editor : Toplevel
{
    private readonly KeyMap keyMap;
    private readonly KeyboardManager keyboardManager;
    private readonly MouseManager mouseManager;

    private Design? viewBeingEdited;
    private bool enableDrag = true;
    private bool enableShowFocused = true;
    private bool editing = false;

    private ListView? rootCommandsListView;
    private bool menuOpen;

    /// <summary>
    /// Set this to have a short duration message appear in lower right
    /// (see <see cref="GetLowerRightTextIfAny"/>).
    /// </summary>
    private string? flashMessage = null;

    /// <summary>
    /// The <see cref="IOperation.UniqueIdentifier"/> of the last undertaken
    /// operation at the time of the last save or null if no save or last save
    /// was before applying any operations.
    /// </summary>
    internal Guid? LastSavedOperation;

    /// <summary>
    /// Initializes a new instance of the <see cref="Editor"/> class.
    /// </summary>
    public Editor()
    {
        // Bug: This will have strange inheritance behavior if Editor is inherited from.
        this.CanFocus = true;

        try
        {
            this.keyMap = new ConfigurationBuilder( ).AddYamlFile( "Keys.yaml", true ).Build( ).Get<KeyMap>( ) ?? new( );

            SelectionManager.Instance.SelectedScheme = this.keyMap.SelectionColor.Scheme;
        }
        catch (Exception ex)
        {
            // if there is bad yaml use the defaults
            ExceptionViewer.ShowException("Failed to read keybindings from configuration file", ex);
            this.keyMap = new KeyMap();
        }

        this.keyboardManager = new KeyboardManager(this.keyMap);
        this.mouseManager = new MouseManager();
        this.Closing += this.Editor_Closing;

        this.BuildRootMenu();
    }

    /// <summary>
    /// Gets or Sets a value indicating whether <see cref="View"/> that do not have borders
    /// (e.g. <see cref="ScrollView"/>) should have a dotted line rendered around them so
    /// users don't lose track of where they are on a same-colored background.
    /// </summary>
    // BUG: Thread-safety
    public static bool ShowBorders { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable experimental features.
    /// </summary>
    // BUG: Thread-safety
    public static bool Experimental { get; internal set; }

    /// <summary>
    /// Runs the <see cref="Editor"/>.
    /// </summary>
    /// <param name="options">Command line options provided by user (or default)
    /// indicating what file to open if any and other settings e.g. <see cref="Experimental"/>.</param>
    public void Run(Options options)
    {
        if (!string.IsNullOrWhiteSpace(options.Path))
        {
            try
            {
                var toLoadOrCreate = new FileInfo(options.Path);

                if (toLoadOrCreate.Exists)
                {
                    this.Open(toLoadOrCreate);
                }
                else
                {
                    Type toCreate = typeof(Window);

                    if (!string.IsNullOrWhiteSpace(options.ViewType))
                    {
                        // TODO: We should probably use something like IsAssignableTo instead
                        toCreate = GetSupportedRootViews().FirstOrDefault(v => v.Name.Equals(options.ViewType)) ?? toCreate;
                    }

                    this.New(toLoadOrCreate, toCreate, options.Namespace);
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error Loading Designer", ex.Message, "Ok");
                Application.Shutdown();
                return;
            }
        }

        Application.KeyDown += (_, k) =>
        {
            if (this.editing || this.viewBeingEdited == null)
            {
                return;
            }

            try
            {
                if (this.HandleKey(k))
                {
                    k.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException("Error processing keystroke", ex);
            }
        };

        Application.MouseEvent += (s, m) =>
        {
            // if another window is showing don't respond to mouse
            if (!this.IsCurrentTop)
            {
                return;
            }

            // If disabling drag we suppress all but right click (button 3)
            if (!m.Flags.HasFlag(MouseFlags.Button3Clicked) && !this.enableDrag)
            {
                return;
            }

            if (this.editing || this.viewBeingEdited == null)
            {
                return;
            }

            try
            {
                this.mouseManager.HandleMouse(m, this.viewBeingEdited);

                // right click
                if (m.Flags.HasFlag(this.keyMap.RightClick))
                {
                    var hit = this.viewBeingEdited.View.HitTest(m, out _, out _);

                    if (hit != null)
                    {
                        var d = hit.GetNearestDesign() ?? this.viewBeingEdited;
                        if (d != null)
                        {
                            this.CreateAndShowContextMenu(m, d);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException("Error processing mouse", ex);
            }
        };

        Application.Run(this, this.ErrorHandler);
        Application.Shutdown();
    }

    /// <summary>
    /// Tailors redrawing to add overlays (e.g. showing what is selected etc.).
    /// </summary>
    protected override bool OnDrawingContent()
    {
        var r = base.OnDrawingContent();
        var bounds = Viewport;

        Application.Driver.SetAttribute(new Attribute(Color.Black));
        Application.Driver.FillRect(bounds,' ');

        // if we are editing a view
        if (this.viewBeingEdited != null)
        {
            if (this.enableShowFocused)
            {
                Application.Driver.SetAttribute(this.viewBeingEdited.View.ColorScheme.Normal);

                string? toDisplay = this.GetLowerRightTextIfAny();

                // and have a designable view focused
                if (toDisplay != null)
                {
                    // write its name in the lower right
                    int y = this.GetContentSize().Height - 1;
                    int right = bounds.Width - 1;
                    var len = toDisplay.Length;

                    for (int i = 0; i < len; i++)
                    {
                        this.AddRune(right - len + i, y, new Rune(toDisplay[i]));
                    }
                }
            }

            if (this.mouseManager.SelectionBox != null)
            {
                var box = this.mouseManager.SelectionBox.Value;
                for (int x = 0; x < box.Width; x++)
                {
                    for (int y = 0; y < box.Height; y++)
                    {
                        if (y == 0 || y == box.Height - 1 || x == 0 || x == box.Width - 1)
                        {
                            this.AddRune(box.X + x, box.Y + y, new Rune('.'));
                        }
                    }
                }
            }

            return r;
        }
        else
        {
            var top = new Rectangle(0, 0, bounds.Width, rootCommandsListView.Frame.Top - 1);
            RenderTitle(top);
        }

        return r;
    }

    private void RenderTitle(Rectangle inArea)
    {
        var assembly = typeof(Label).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "unknown";

        if (informationalVersion.Contains("+"))
        {
            informationalVersion = informationalVersion.Substring(0, informationalVersion.IndexOf('+'));
        }

        // The main ASCII art text block
        string artText = """
                ___________                  .__              .__                           
                \__    ___/__________  _____ |__| ____ _____  |  |                          
                  |    |_/ __ \_  __ \/     \|  |/    \\__  \ |  |                          
                  |    |\  ___/|  | \/  Y Y  \  |   |  \/ __ \|  |__                        
                  |____| \___  >__|  |__|_|  /__|___|  (____  /____/                        
                           \/            \/        \/     \/                              
    ________      .__  ________                .__                            
   /  _____/ __ __|__| \______ \   ____   _____|__| ____   ____   ___________ 
  /   \  ___|  |  \  |  |    |  \_/ __ \ /  ___/  |/ ___\ /    \_/ __ \_  __ \
  \    \_\  \  |  /  |  |    `   \  ___/ \___ \|  / /_/  >   |  \  ___/|  | \/
   \______  /____/|__| /_______  /\___  >____  >__\___  /|___|  /\___  >__|   
          \/                   \/     \/     \/  /_____/      \/     \/       
""";

        // Standardize the text
        artText = artText.Replace("\r\n", "\n");

        // The version information line
        string versionLine = $"(Alpha - {informationalVersion} )";

        // Split the ASCII art into lines
        var artLines = artText.Split('\n');

        // Calculate the starting point for centering the art text
        int artHeight = artLines.Length;
        int artWidth = artLines.Max(line => line.Length);

        // Check if there's enough space for the ASCII art and the version line
        if (inArea.Width < artWidth || inArea.Height < (artHeight + 2)) // +2 allows space for version line
        {
            // Not enough space, render the simpler title

            // Simple title and version
            string simpleTitle = "Terminal Gui Designer";
            int simpleTitleX = inArea.X + (inArea.Width - simpleTitle.Length) / 2;
            int versionLineX = inArea.X + (inArea.Width - versionLine.Length) / 2;

            // Create the gradient
            var gradient = new Gradient(
                new[]
                {
                new Color("#FF0000"), // Red
                new Color("#FF7F00"), // Orange
                new Color("#FFFF00"), // Yellow
                new Color("#00FF00"), // Green
                new Color("#00FFFF"), // Cyan
                new Color("#0000FF"), // Blue
                new Color("#8B00FF")  // Violet
                },
                new[] { 10 }
            );
            var fill = new GradientFill(inArea, gradient, GradientDirection.Diagonal);

            // Render the simple title
            for (int i = 0; i < simpleTitle.Length; i++)
            {
                int x = simpleTitleX + i;
                int y = inArea.Y + inArea.Height / 2 - 1; // Center the title vertically

                var colorAtPoint = fill.GetColor(new Point(x, y));
                Driver.SetAttribute(new Attribute(new Color(colorAtPoint), new Color(Color.Black)));
                this.AddRune(x, y, (Rune)simpleTitle[i]);
            }

            // Render the version line below the simple title
            for (int i = 0; i < versionLine.Length; i++)
            {
                int x = versionLineX + i;
                int y = inArea.Y + inArea.Height / 2; // Line below the title

                var colorAtPoint = fill.GetColor(new Point(x, y));
                Driver.SetAttribute(new Attribute(new Color(colorAtPoint), new Color(Color.Black)));
                this.AddRune(x, y, (Rune)versionLine[i]);
            }
        }
        else
        {
            // Enough space, render the ASCII art block

            int artStartX = inArea.X + (inArea.Width - artWidth) / 2;
            int artStartY = inArea.Y + (inArea.Height - artHeight - 1) / 2; // -1 for the version line below

            // Create the gradient
            var gradient = new Gradient(
                new[]
                {
                new Color("#FF0000"), // Red
                new Color("#FF7F00"), // Orange
                new Color("#FFFF00"), // Yellow
                new Color("#00FF00"), // Green
                new Color("#00FFFF"), // Cyan
                new Color("#0000FF"), // Blue
                new Color("#8B00FF")  // Violet
                },
                new[] { 10 }
            );
            var fill = new GradientFill(inArea, gradient, GradientDirection.Diagonal);

            // Render the ASCII art block
            for (int i = 0; i < artLines.Length; i++)
            {
                string line = artLines[i];
                for (int j = 0; j < line.Length; j++)
                {
                    int x = artStartX + j;
                    int y = artStartY + i;

                    var colorAtPoint = fill.GetColor(new Point(x, y));
                    Driver.SetAttribute(new Attribute(new Color(colorAtPoint), new Color(Color.Black)));
                    this.AddRune(x, y, (Rune)line[j]);
                }
            }

            // Render the version line below the ASCII art
            int versionLineX = inArea.X + (inArea.Width - versionLine.Length) / 2;
            int versionLineY = artStartY + artHeight;

            for (int i = 0; i < versionLine.Length; i++)
            {
                int x = versionLineX + i;
                int y = versionLineY;

                var colorAtPoint = fill.GetColor(new Point(x, y));
                Driver.SetAttribute(new Attribute(new Color(colorAtPoint), new Color(Color.Black)));
                this.AddRune(x, y, (Rune)versionLine[i]);
            }
        }
    }




    /// <summary>
    /// Event handler for <see cref="Application.KeyDown"/>.
    /// </summary>
    /// <param name="key">The key pressed.</param>
    /// <returns>True if key is handled.</returns>
    public bool HandleKey(Key key)
    {
        // if another window is showing don't respond to hotkeys
        if (!this.IsCurrentTop)
        {
            return false;
        }

        if (this.editing)
        {
            return false;
        }

        // Give the keyboard manager first shot at consuming
        // this key e.g. for typing into menus / reordering menus
        // etc
        if (this.keyboardManager.HandleKey(
            SelectionManager.Instance.GetSingleSelectionOrNull()?.View ?? this, key))
        {
            return true;
        }

        try
        {
            this.editing = true;
            SelectionManager.Instance.LockSelection = true;

            string keyString = key.ToString( );
            if (keyString == this.keyMap.ShowContextMenu && !this.menuOpen)
            {
                this.CreateAndShowContextMenu(null, null);
                return true;
            }

            if (keyString == this.keyMap.EditProperties)
            {
                this.ShowEditProperties();
                return true;
            }

            if (keyString == this.keyMap.ShowColorSchemes)
            {
                this.ShowColorSchemes();
                return true;
            }

            if (keyString == this.keyMap.Copy)
            {
                this.Copy();
                return true;
            }

            if (keyString == this.keyMap.Paste)
            {
                this.Paste();
                return true;
            }

            if (keyString == this.keyMap.ViewSpecificOperations)
            {
                this.ShowViewSpecificOperations();
                return true;
            }

            if (keyString == this.keyMap.EditRootProperties)
            {
                if (this.viewBeingEdited == null)
                {
                    return false;
                }

                this.ShowEditProperties(this.viewBeingEdited);
                return true;
            }

            if (keyString == this.keyMap.Open)
            {
                this.Open();
                return true;
            }

            if (keyString == this.keyMap.Save)
            {
                this.Save();
                return true;
            }

            if (keyString == this.keyMap.New)
            {
                this.New();
                return true;
            }

            if (keyString == this.keyMap.ShowHelp)
            {
                this.ShowHelp();
                return true;
            }

            if (keyString == this.keyMap.AddView)
            {
                this.ShowAddViewWindow();
                return true;
            }

            if (keyString == this.keyMap.ToggleDragging)
            {
                this.enableDrag = !this.enableDrag;
                return true;
            }

            if (keyString == this.keyMap.Undo)
            {
                OperationManager.Instance.Undo();
                return true;
            }

            if (keyString == this.keyMap.Redo)
            {
                OperationManager.Instance.Redo();
                return true;
            }

            if (keyString == this.keyMap.Delete)
            {
                this.Delete();
                return true;
            }

            if (keyString == this.keyMap.ToggleShowFocused)
            {
                this.enableShowFocused = !this.enableShowFocused;
                this.SetNeedsDraw();
                return true;
            }

            if (keyString == this.keyMap.ToggleShowBorders)
            {
                ShowBorders = !ShowBorders;
                this.SetNeedsDraw();
                return true;
            }

            if (keyString == this.keyMap.SelectAll)
            {
                this.SelectAll();
                return true;
            }

            if (keyString == this.keyMap.MoveUp)
            {
                this.MoveControl(0, -1);
                return true;
            }

            if (keyString == this.keyMap.MoveDown)
            {
                this.MoveControl(0, 1);
                return true;
            }

            if (keyString == this.keyMap.MoveLeft)
            {
                this.MoveControl(-1, 0);
                return true;
            }

            if (keyString == this.keyMap.MoveRight)
            {
                this.MoveControl(1, 0);
                return true;
            }

            if (keyString == this.keyMap.MoveDown)
            {
                this.MoveControl(0, 1);
                return true;
            }

            if (keyString == this.keyMap.MoveLeft)
            {
                this.MoveControl(-1, 0);
                return true;
            }

            if (keyString == this.keyMap.MoveRight)
            {
                this.MoveControl(1, 0);
                return true;
            }

            // Fast moving things
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp | KeyCode.CtrlMask:
                    this.MoveControl(0, -3);
                    return true;
                case KeyCode.CursorDown | KeyCode.CtrlMask:
                    this.MoveControl(0, 3);
                    return true;
                case KeyCode.CursorLeft | KeyCode.CtrlMask:
                    this.MoveControl(-5, 0);
                    return true;
                case KeyCode.CursorRight | KeyCode.CtrlMask:
                    this.MoveControl(5, 0);
                    return true;
            }
        }
        catch (Exception ex)
        {
            ExceptionViewer.ShowException("Error", ex);
        }
        finally
        {
            SelectionManager.Instance.LockSelection = false;
            this.editing = false;
        }

        return false;
    }

    /// <summary>
    /// Gets a value indicating whether there have been any <see cref="Operation"/>s tracked by the <see cref="OperationManager"/>
    /// since the last save.
    /// </summary>
    /// <value><see langword="true" /> if unsaved changes exist.</value>
    public bool HasUnsavedChanges
    {
        get
        {
            var savedOp = this.LastSavedOperation;
            var currentOp = OperationManager.Instance.GetLastAppliedOperation( )?.UniqueIdentifier;

            // if we have nothing saved
            if ( savedOp == null )
            {
                // then we must save if we have done something
                return currentOp != null;
            }

            // we must save if the head of the operations stack doesn't match what we saved
            // this lets us save, perform action, undo action and then still consider us saved
            return savedOp != currentOp;
        }
    }

    private string GetHelpWithEmptyFormLoaded()
    {
        return $"{this.keyMap.AddView} to Add a View";
    }

    private string GetHelp()
    {
        return $"""

                {this.keyMap.ShowHelp} - Show Help
                {this.keyMap.New} - New Window/Class
                {this.keyMap.Open} - Open a .Designer.cs file
                {this.keyMap.Save} - Save an opened .Designer.cs file
                {this.keyMap.ShowContextMenu} - Show right click context menu;
                {this.keyMap.AddView} - Add View
                {this.keyMap.ShowColorSchemes} - Color Schemes
                {this.keyMap.ToggleDragging} - Toggle mouse dragging on/off
                {this.keyMap.ToggleShowFocused} - Toggle show focused view field name
                {this.keyMap.ToggleShowBorders} - Toggle dotted borders for frameless views
                {this.keyMap.EditProperties} - Edit View Properties
                {this.keyMap.ViewSpecificOperations} - View Specific Operations
                {this.keyMap.EditRootProperties} - Edit Root Properties
                {this.keyMap.Delete} - Delete selected View
                Shift+Cursor - Move focused View
                Ctrl+Cursor - Move focused View quickly
                Esc - Quit
                {this.keyMap.Undo} - Undo
                {this.keyMap.Redo} - Redo
                """;
    }

    private void BuildRootMenu()
    {
        /* setup views for when we are not editing a
         * view (nothing is loaded) so show the generic
         * help (open, new etc.) in the center of the
         * screen
         */

        var rootCommands = new ObservableCollection<string>
        {
            $"{this.keyMap.ShowHelp} - Show Help",
            $"{this.keyMap.New} - New Window/Class",
            $"{this.keyMap.Open} - Open a .Designer.cs file",
        };

        // center all the commands
        int maxWidth = rootCommands.Max(v => v.Length);
        for (int i = 0; i < rootCommands.Count; i++)
        {
            rootCommands[i] = rootCommands[i].PadBoth(maxWidth);
        }

        this.rootCommandsListView = new ListView()
        {
            X = Pos.Center(),
            Y = Pos.Percent(75),
            Width = maxWidth,
            Height = 3,
            ColorScheme = new ColorScheme
            (
                new Attribute(new Color(Color.White),new Color(Color.Black)),
                new Attribute(new Color(Color.Black),new Color(Color.White)),
            new Attribute(new Color(Color.White), new Color(Color.Black)),
            new Attribute(new Color(Color.White), new Color(Color.Black)),
            new Attribute(new Color(Color.Black), new Color(Color.White))
                ),
        };
        this.rootCommandsListView.SetSource(rootCommands);
        this.rootCommandsListView.SelectedItem = 0;

        this.rootCommandsListView.KeyDown += (_, e) =>
        {
            if (e == Key.Enter)
            {
                e.Handled = true;

                switch (this.rootCommandsListView.SelectedItem)
                {
                    case 0:
                        this.ShowHelp();
                        break;
                    case 1:
                        this.New();
                        break;
                    case 2:
                        this.Open();
                        break;
                }
            }


            if (e == this.keyMap.New)
            {
                this.New();
            }

            if (e == this.keyMap.ShowHelp)
            {
                this.ShowHelp();
            }

            if (e == this.keyMap.Open)
            {
                this.Open();
            }
        };

        this.Add(this.rootCommandsListView);
    }

    private void Editor_Closing(object? sender, ToplevelClosingEventArgs obj)
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        if (this.HasUnsavedChanges)
        {
            int answer = ChoicesDialog.Query("Unsaved Changes", $"You have unsaved changes to {this.viewBeingEdited.SourceCode.DesignerFile.Name}", "Save", "Don't Save", "Cancel");

            if (answer == 0)
            {
                this.Save();
            }
            else
            if (answer == 1)
            {
                return;
            }
            else
            {
                obj.Cancel = true;
            }
        }
    }

    private void CreateAndShowContextMenu(MouseEventArgs? m, Design? rightClicked)
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        var selected = SelectionManager.Instance.Selected.ToArray();

        // BUG: This is an improper exception here and could have unexpected behavior if this method is ever called asynchronously.
        var factory = new OperationFactory(
                (p, v) => ValueFactory.GetNewValue(p.Design, p, v, out var newValue) ? newValue : throw new OperationCanceledException() );

        var operations = factory
            .CreateOperations(selected, m, rightClicked, out string name)
            .Where(o => !o.IsImpossible)
            .ToArray();

        var setProps = operations.OfType<SetPropertyOperation>();
        var others = operations
            .Except(setProps)
            .GroupBy(k => k.Category, ToMenuItem);

        var setPropsItems = setProps.Select(ToMenuItem).ToArray();
        bool hasPropsItems = setPropsItems.Any();

        var all = new List<MenuItem>();

        // only add the set properties category if there are some
        if (hasPropsItems)
        {
            all.Add(new MenuBarItem(name, setPropsItems)
            {
                Action = () =>
                {
                    if (selected.Length == 1 || rightClicked != null)
                    {
                        this.ShowEditProperties(rightClicked ?? selected[0]);
                    }
                },
            });
        }

        // For each ExtraOperation grouped by Category
        foreach (var g in others)
        {
            // if there is no category
            if (string.IsNullOrWhiteSpace(g.Key))
            {
                // add the operations with no category in alphabetical order
                all.AddRange(g.OrderBy(mi => mi.Title));
            }
            else
            {
                // Add categories first
                all.Insert(
                    hasPropsItems ? 1 : 0,
                    new MenuBarItem(g.Key, g.ToArray()));
            }
        }

        // there's nothing we can do
        if (all.Count == 0)
        {
            return;
        }

        var menu = new ContextMenu();
        menu.SetMenuItems(new MenuBarItem(all.ToArray()));

        if (m != null)
        {
            menu.Position = m.Position;
        }
        else
        {
            var d = SelectionManager.Instance.Selected.FirstOrDefault() ?? this.viewBeingEdited;
            var pt = d.View.ContentToScreen(new Point(0, 0));
            menu.Position = new Point(pt.X, pt.Y);
        }

        this.menuOpen = true;
        SelectionManager.Instance.LockSelection = true;
        
        if(m != null)
        {
            m.Handled = true;
        }

        // TODO: rly? you have to pass it its own menu items!?
        menu.Show(menu.MenuItems);
        menu.MenuBar.MenuAllClosed += (_, _) =>
        {
            this.menuOpen = false;
            SelectionManager.Instance.LockSelection = false;
        };
    }

    private static MenuItem ToMenuItem(IOperation operation)
    {
        return new MenuItem(operation.ToString(), string.Empty, () => Try(() => OperationManager.Instance.Do(operation)));

        static void Try(Action action)
        {
            try
            {
                // BUG: Thread-safety
                // Race conditions because this is not a valid synchronization mechanism
                SelectionManager.Instance.LockSelection = true;
                action();
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException("Operation failed", ex);
            }
            finally
            {
                SelectionManager.Instance.LockSelection = false;
            }
        }
    }


    private string? GetLowerRightTextIfAny()
    {
        if (this.flashMessage != null)
        {
            var m = this.flashMessage;
            this.flashMessage = null;
            return m;
        }

        if (MenuTracker.Instance.CurrentlyOpenMenuItem != null)
        {
            return $"Selected: {MenuTracker.Instance.CurrentlyOpenMenuItem.Title}";
        }

        var selected = SelectionManager.Instance.Selected.ToArray();

        string name = selected.Length == 1 ? selected[0].FieldName : $"{selected.Length} objects";

        if (selected.Any())
        {
            return $"Selected: {name} ({this.keyMap.EditProperties} to Edit, {this.keyMap.ShowHelp} for Help)";
        }

        return this.GetHelpWithEmptyFormLoaded();
    }

    private void SelectAll()
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        var everyone = this.viewBeingEdited.GetAllDesigns()
            .Where(d => !d.IsRoot)
            .ToArray();

        SelectionManager.Instance.ForceSetSelection(everyone);
    }

    private void Paste()
    {
        var d = SelectionManager.Instance.GetMostSelectedContainerOrNull() ?? this.viewBeingEdited;

        if (d != null)
        {
            var paste = new PasteOperation(d);

            if (paste.IsImpossible)
            {
                return;
            }

            OperationManager.Instance.Do(paste);
        }
    }

    private void Copy()
    {
        var copy = new CopyOperation(SelectionManager.Instance.Selected.ToArray());
        OperationManager.Instance.Do(copy);
    }

    private void ShowViewSpecificOperations()
    {
        var d = SelectionManager.Instance.GetSingleSelectionOrNull();

        if (d != null)
        {
            var options = d.GetExtraOperations().Where(o => !o.IsImpossible).ToArray();

            if (options.Any() && Modals.Get("Operations", "Ok", options, null, out var selected) && selected != null)
            {
                OperationManager.Instance.Do(selected);
            }
        }
    }

    private void ShowHelp()
    {
        ChoicesDialog.Query("Help", this.GetHelp(), "Ok");
    }

    private void MoveControl(int deltaX, int deltaY)
    {
        this.DoForSelectedViews((d) => new MoveViewOperation(d, deltaX, deltaY));
    }

    private void Delete()
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        if (SelectionManager.Instance.Selected.Any())
        {
            var cmd = new DeleteViewOperation(SelectionManager.Instance.Selected.ToArray());
            OperationManager.Instance.Do(cmd);
        }
    }

    private void DoForSelectedViews(Func<Design, Operation> operationFunc, bool allowOnRoot = false)
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        var selected = SelectionManager.Instance.Selected.ToArray();

        if (selected.Length > 1)
        {
            var op = new CompositeOperation(
                SelectionManager.Instance.Selected
                .Select(operationFunc).ToArray());

            OperationManager.Instance.Do(op);
        }
        else if (selected.Length == 1)
        {
            var viewDesign = selected.Single();

            // don't delete the root view
            if (viewDesign.IsRoot && !allowOnRoot)
            {
                return;
            }

            OperationManager.Instance.Do(operationFunc(viewDesign));
        }
    }

    private void Open()
    {
        var ofd = new OpenDialog()
        {
            Title = "Open",
            AllowedTypes = new List<IAllowedType>(new[] { new AllowedType("View", SourceCodeFile.ExpectedExtension) })
        };  

        Application.Run(ofd, this.ErrorHandler);

        if (!ofd.Canceled)
        {
            try
            {
                var path = ofd.Path.ToString();

                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                this.Open(new FileInfo(path));
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException($"Failed to open '{ofd.Path}'", ex);
            }
        }
    }

    private bool ErrorHandler(Exception arg)
    {
        ExceptionViewer.ShowException("Global Exception", arg);
        return true;
    }

    private void Open(FileInfo toOpen)
    {
        var open = new LoadingDialog(toOpen);

        // since we are opening a new view we should
        // clear the history
        OperationManager.Instance.ClearUndoRedo();
        Design? instance = null;
        SelectionManager.Instance.Clear();

        Task.Run(() =>
        {
            var decompiler = new CodeToView(new SourceCodeFile(toOpen));
            instance = decompiler.CreateInstance();
        }).ContinueWith(
            (t, _) =>
            {
                // no longer loading
                Application.Invoke(() => Application.RequestStop());

                if (t.Exception != null)
                {
                    Application.Invoke(() =>
                        ExceptionViewer.ShowException($"Failed to open '{toOpen.Name}'", t.Exception));
                    return;
                }

                // if loaded correctly then
                if (instance != null)
                {
                    this.ReplaceViewBeingEdited(instance);
                }
            },
            TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open, this.ErrorHandler);
    }

    private void New()
    {
        if (!Modals.Get("Create New View", "Ok", GetSupportedRootViews(), null, out var selected))
        {
            return;
        }

        var ofd = new SaveDialog()
        {
            Title = "New",
            AllowedTypes = new List<IAllowedType>() { new AllowedType("C# File", ".cs") },
            Path = "MyView.cs",
        };

        Application.Run(ofd);

        if (!ofd.Canceled)
        {
            try
            {
                var path = ofd.Path;

                if (string.IsNullOrWhiteSpace(path) || selected == null)
                {
                    return;
                }

                var file = new FileInfo(path);

                // Check if we are about to overwrite some files
                // and if so warn the user
                var files = new SourceCodeFile(file);

                if(!CodeDomArgs.IsValidIdentifier(files.ClassName))
                {
                    ChoicesDialog.Query("Invalid Name",$"Invalid class name '{files.ClassName}'","Ok");
                    return;
                }

                var sb = new StringBuilder();

                if (files.CsFile.Exists)
                {
                    sb.AppendLine(files.CsFile.Name);
                }

                if (files.DesignerFile.Exists)
                {
                    sb.AppendLine(files.DesignerFile.Name);
                }

                if (sb.Length > 0)
                {
                    if (!ChoicesDialog.Confirm("Overwrite Files?", $"The following files will be overwritten:{Environment.NewLine}{sb.ToString().TrimEnd()}", "Ok", "Cancel"))
                    {
                        return; // user canceled overwrite
                    }
                }

                this.New(file, selected, null);
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException($"Failed to create '{ofd.Path}'", ex);
                throw;
            }
        }
    }

    private static Type[] GetSupportedRootViews()
    {
        return new Type[] { typeof(Window), typeof(Dialog), typeof(View), typeof(Toplevel) };
    }

    private void New(FileInfo toOpen, Type typeToCreate, string? explicitNamespace)
    {
        var viewToCode = new ViewToCode();
        string? ns = explicitNamespace;

        // TODO: The following two if statements can be combined and run in a loop until the user either cancels or gets it right
        // if no explicit one
        if (string.IsNullOrWhiteSpace(ns))
        {
            // prompt user for namespace
            if (!Modals.GetString("Namespace", "Enter the namespace for your class", "YourNamespace", out ns))
            {
                // user cancelled typing a namespace
                return;
            }
        }

        // Validate the namespace
        if (string.IsNullOrWhiteSpace(ns) || ns.Contains(' ') || char.IsDigit(ns.First()))
        {
            MessageBox.ErrorQuery("Invalid Namespace", "Namespace must not contain spaces, be empty or begin with a number", "Ok");
            return;
        }

        // since we are creating a new view we should
        // clear the history
        OperationManager.Instance.ClearUndoRedo();
        Design? instance = null;
        SelectionManager.Instance.Clear();

        var open = new LoadingDialog(toOpen);

        // BUG: If this is not awaited, exceptions at any point of it can be thrown at an indeterminate place and time.
        Task.Run(() =>
        {
            // Create the view files and compile
            instance = viewToCode.GenerateNewView(toOpen, ns ?? "YourNamespace", typeToCreate);
        }).ContinueWith(
            (t, _) =>
            {
                // no longer loading
                Application.Invoke(() => Application.RequestStop());

                if (t.Exception != null)
                {
                    Application.Invoke(() =>
                        ExceptionViewer.ShowException($"Failed to create '{toOpen.Name}'", t.Exception));
                    return;
                }

                // if loaded correctly then
                if (instance != null)
                {
                    this.ReplaceViewBeingEdited(instance);
                }
            },
            TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open, this.ErrorHandler);
    }

    private void ReplaceViewBeingEdited(Design design)
    {
        Application.Invoke(() =>
        {
            // remove the old view
            if (this.viewBeingEdited != null)
            {
                // and dispose it
                this.Remove(this.viewBeingEdited.View);
                this.viewBeingEdited.View.Dispose();
            }

            // remove list view to prevent it stealing keystrokes and jumping back
            // into input focus
            this.Remove(this.rootCommandsListView);

            // Load new instance
            this.viewBeingEdited = design;

            // TODO: Find a better place for this
            ColorSchemeManager.Instance.FindDeclaredColorSchemes(this.viewBeingEdited);

            // And add it to the editing window
            this.Add(this.viewBeingEdited.View);
        });
    }

    private void Save()
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        var viewToCode = new ViewToCode();

        viewToCode.GenerateDesignerCs(
            this.viewBeingEdited,
            this.viewBeingEdited.View.GetType().BaseType ?? throw new Exception("View being edited had no base class"));

        this.flashMessage = $"Saved {this.viewBeingEdited.SourceCode.DesignerFile.Name}";
        this.SetNeedsDraw();

        this.LastSavedOperation = OperationManager.Instance.GetLastAppliedOperation()?.UniqueIdentifier;
    }

    private void ShowAddViewWindow()
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        // what is the currently selected design
        var toAddTo = SelectionManager.Instance.GetMostSelectedContainerOrNull() ?? this.viewBeingEdited;

        OperationManager.Instance.Do(
            new AddViewOperation(toAddTo));
    }

    private void ShowEditProperties()
    {
        var d = SelectionManager.Instance.GetSingleSelectionOrNull();
        if (d != null)
        {
            this.ShowEditProperties(d);
        }
    }

    private void ShowEditProperties(Design d)
    {
        var edit = new EditDialog(d);
        Application.Run(edit, this.ErrorHandler);
    }

    private void ShowColorSchemes()
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        var schemes = new ColorSchemesUI(this.viewBeingEdited);
        Application.Run(schemes);
    }
}
