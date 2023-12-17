using System.Text;
using Microsoft.Extensions.Configuration;
using Terminal.Gui;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

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
        this.CanFocus = true;

        // If there are custom keybindings read those
        if (File.Exists("Keys.yaml"))
        {
            try
            {
                this.keyMap = new ConfigurationBuilder( ).AddYamlFile( "Keys.yaml", true ).Build( ).Get<KeyMap>( ) ?? new( );

                if (this.keyMap.SelectionColor != null)
                {
                    SelectionManager.Instance.SelectedScheme = this.keyMap.SelectionColor.Scheme;
                }
            }
            catch (Exception ex)
            {
                // if there is bad yaml use the defaults
                ExceptionViewer.ShowException("Failed to read keybindings", ex);
                this.keyMap = new KeyMap();
            }
        }
        else
        {
            // otherwise use the defaults
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
    /// users don't loose track of where they are on a same colored background.
    /// </summary>
    public static bool ShowBorders { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether true to enable experimental features.
    /// </summary>
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
                        toCreate = this.GetSupportedRootViews().FirstOrDefault(v => v.Name.Equals(options.ViewType)) ?? toCreate;
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

        Application.KeyPressed += (s,k) =>
        {
            if (this.editing)
            {
                return;
            }

            try
            {
                if (this.HandleKey(k.KeyEvent))
                {
                    k.Handled = true;
                }
            }
            catch (System.Exception ex)
            {
                ExceptionViewer.ShowException("Error processing keystroke", ex);
            }
        };

        Application.MouseEvent += (s,m) =>
        {
            // if another window is showing don't respond to mouse
            if (!this.IsCurrentTop)
            {
                return;
            }

            // If disabling drag we suppress all but right click (button 3)
            if (!m.MouseEvent.Flags.HasFlag(MouseFlags.Button3Clicked) && !this.enableDrag)
            {
                return;
            }

            if (this.editing || this.viewBeingEdited == null)
            {
                return;
            }

            try
            {
                this.mouseManager.HandleMouse(m.MouseEvent, this.viewBeingEdited);

                // right click
                if (m.MouseEvent.Flags.HasFlag(this.keyMap.RightClick))
                {
                    var hit = this.viewBeingEdited.View.HitTest(m.MouseEvent, out _, out _);

                    if (hit != null)
                    {
                        var d = hit.GetNearestDesign() ?? this.viewBeingEdited;
                        if (d != null)
                        {
                            this.CreateAndShowContextMenu(m.MouseEvent, d);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ExceptionViewer.ShowException("Error processing mouse", ex);
            }
        };

        Application.Run(this, this.ErrorHandler);
        Application.Shutdown();
    }

    /// <summary>
    /// Tailors redrawing to add overlays (e.g. showing what is selected etc).
    /// </summary>
    /// <param name="bounds">The view bounds.</param>
    public override void OnDrawContent(Rect bounds)
    {
        base.OnDrawContent(bounds);

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
                    int y = this.Bounds.Height - 1;
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

            return;
        }
    }

    /// <summary>
    /// Event handler for <see cref="Application.RootKeyEvent"/>.
    /// </summary>
    /// <param name="keyEvent">The key pressed.</param>
    /// <returns>True if key is handled.</returns>
    public bool HandleKey(KeyEvent keyEvent)
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
            SelectionManager.Instance.GetSingleSelectionOrNull()?.View ?? this, keyEvent))
        {
            return true;
        }

        try
        {
            this.editing = true;
            SelectionManager.Instance.LockSelection = true;

            if (keyEvent.Key == this.keyMap.ShowContextMenu && !this.menuOpen)
            {
                this.CreateAndShowContextMenu(null, null);
                return true;
            }

            if (keyEvent.Key == this.keyMap.EditProperties)
            {
                this.ShowEditProperties();
                return true;
            }

            if (keyEvent.Key == this.keyMap.ShowColorSchemes)
            {
                this.ShowColorSchemes();
                return true;
            }

            if (keyEvent.Key == this.keyMap.Copy)
            {
                this.Copy();
                return true;
            }

            if (keyEvent.Key == this.keyMap.Paste)
            {
                this.Paste();
                return true;
            }

            if (keyEvent.Key == this.keyMap.ViewSpecificOperations)
            {
                this.ShowViewSpecificOperations();
                return true;
            }

            if (keyEvent.Key == this.keyMap.EditRootProperties)
            {
                if (this.viewBeingEdited == null)
                {
                    return false;
                }

                this.ShowEditProperties(this.viewBeingEdited);
                return true;
            }

            if (keyEvent.Key == this.keyMap.Open)
            {
                this.Open();
                return true;
            }

            if (keyEvent.Key == this.keyMap.Save)
            {
                this.Save();
                return true;
            }

            if (keyEvent.Key == this.keyMap.New)
            {
                this.New();
                return true;
            }

            if (keyEvent.Key == this.keyMap.ShowHelp)
            {
                this.ShowHelp();
                return true;
            }

            if (keyEvent.Key == this.keyMap.AddView)
            {
                this.ShowAddViewWindow();
                return true;
            }

            if (keyEvent.Key == this.keyMap.ToggleDragging)
            {
                this.enableDrag = !this.enableDrag;
                return true;
            }

            if (keyEvent.Key == this.keyMap.Undo)
            {
                OperationManager.Instance.Undo();
                return true;
            }

            if (keyEvent.Key == this.keyMap.Redo)
            {
                OperationManager.Instance.Redo();
                return true;
            }

            if (keyEvent.Key == this.keyMap.Delete)
            {
                this.Delete();
                return true;
            }

            if (keyEvent.Key == this.keyMap.ToggleShowFocused)
            {
                this.enableShowFocused = !this.enableShowFocused;
                this.SetNeedsDisplay();
                return true;
            }

            if (keyEvent.Key == this.keyMap.ToggleShowBorders)
            {
                ShowBorders = !ShowBorders;
                this.SetNeedsDisplay();
                return true;
            }

            if (keyEvent.Key == this.keyMap.SelectAll)
            {
                this.SelectAll();
                return true;
            }

            if (keyEvent.Key == this.keyMap.MoveUp)
            {
                this.MoveControl(0, -1);
                return true;
            }

            if (keyEvent.Key == this.keyMap.MoveDown)
            {
                this.MoveControl(0, 1);
                return true;
            }

            if (keyEvent.Key == this.keyMap.MoveLeft)
            {
                this.MoveControl(-1, 0);
                return true;
            }

            if (keyEvent.Key == this.keyMap.MoveRight)
            {
                this.MoveControl(1, 0);
                return true;
            }

            if (keyEvent.Key == this.keyMap.MoveDown)
            {
                this.MoveControl(0, 1);
                return true;
            }

            if (keyEvent.Key == this.keyMap.MoveLeft)
            {
                this.MoveControl(-1, 0);
                return true;
            }

            if (keyEvent.Key == this.keyMap.MoveRight)
            {
                this.MoveControl(1, 0);
                return true;
            }

            // Fast moving things
            switch (keyEvent.Key)
            {
                case Key.CursorUp | Key.CtrlMask:
                    this.MoveControl(0, -3);
                    return true;
                case Key.CursorDown | Key.CtrlMask:
                    this.MoveControl(0, 3);
                    return true;
                case Key.CursorLeft | Key.CtrlMask:
                    this.MoveControl(-5, 0);
                    return true;
                case Key.CursorRight | Key.CtrlMask:
                    this.MoveControl(5, 0);
                    return true;
            }
        }
        catch (System.Exception ex)
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
        return @$"{this.keyMap.AddView} to Add a View";
    }

    private string GetHelp()
    {
        return @$"
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
Ctrl+Q - Quit
{this.keyMap.Undo} - Undo
{this.keyMap.Redo} - Redo";
    }

    private void BuildRootMenu()
    {
        /* setup views for when we are not editing a
         * view (nothing is loaded) so show the generic
         * help (open, new etc) in the center of the
         * screen
         */

        var rootCommands = new List<string>
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

        this.rootCommandsListView = new ListView(rootCommands)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = maxWidth,
            Height = 3,
            ColorScheme = new DefaultColorSchemes().GetDefaultScheme("greyOnBlack").Scheme,
        };

        this.rootCommandsListView.KeyDown += (s, e) =>
        {
            if (e.KeyEvent.Key == Key.Enter)
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

    private void CreateAndShowContextMenu(MouseEvent? m, Design? rightClicked)
    {
        if (this.viewBeingEdited == null)
        {
            return;
        }

        var selected = SelectionManager.Instance.Selected.ToArray();

        var factory = new OperationFactory(
                (p, v) =>
                {
                    return EditDialog.GetNewValue(p.Design, p, v, out var newValue) ? newValue
                    : throw new OperationCanceledException();
                });

        var operations = factory
            .CreateOperations(selected, m, rightClicked, out string name)
            .Where(o => !o.IsImpossible)
            .ToArray();

        var setProps = operations.OfType<SetPropertyOperation>();
        var others = operations
            .Except(setProps)
            .GroupBy(k => k.Category, this.ToMenuItem);

        var setPropsItems = setProps.Select(this.ToMenuItem).ToArray();
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
                all.AddRange(g.OrderBy(g => g.Title));
            }
            else
            {
                // Add categories first
                all.Insert(
                    hasPropsItems ? 1 : 0,
                    new MenuBarItem(g.Key, g.ToArray()));
            }
        }

        // theres nothing we can do
        if (all.Count == 0)
        {
            return;
        }

        var menu = new ContextMenu();
        menu.MenuItems = new MenuBarItem(all.ToArray());

        if (m != null)
        {
            menu.Position = new Point(m.X, m.Y);
        }
        else
        {
            var d = SelectionManager.Instance.Selected.FirstOrDefault() ?? this.viewBeingEdited;
            d.View.BoundsToScreen(0, 0, out var x, out var y);
            menu.Position = new Point(x, y);
        }

        this.menuOpen = true;
        SelectionManager.Instance.LockSelection = true;
        menu.Show();
        menu.MenuBar.MenuAllClosed += (s, e) =>
        {
            this.menuOpen = false;
            SelectionManager.Instance.LockSelection = false;
        };
    }

    private MenuItem ToMenuItem(IOperation operation)
    {
        return new MenuItem(operation.ToString(), string.Empty, () => this.Try(() => OperationManager.Instance.Do(operation)));
    }

    private void Try(Action action)
    {
        try
        {
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

    private void DoForSelectedViews(Func<Design, Operation> operationFuc, bool allowOnRoot = false)
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
                .Select(operationFuc).ToArray());

            OperationManager.Instance.Do(op);
        }
        else if (selected.Length == 1)
        {
            var viewDesign = selected.Single();

            // don't delete the root view
            if (viewDesign != null)
            {
                if (viewDesign.IsRoot && !allowOnRoot)
                {
                    return;
                }

                OperationManager.Instance.Do(
                    operationFuc(viewDesign));
            }
        }
    }

    private void Open()
    {
        var ofd = new OpenDialog(
            "Open",
            new List<IAllowedType>(new[] { new AllowedType("View", SourceCodeFile.ExpectedExtension ) }));

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
            (t, o) =>
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
        if (!Modals.Get("Create New View", "Ok", this.GetSupportedRootViews(), null, out var selected))
        {
            return;
        }

        var ofd = new SaveDialog(
            "New",
            new List<IAllowedType>() { new AllowedType("C# File", ".cs") })
        {
            Path = "MyView.cs",
        };

        Application.Run(ofd);

        if (!ofd.Canceled)
        {
            try
            {
                var path = ofd.Path.ToString();

                if (string.IsNullOrWhiteSpace(path) || selected == null)
                {
                    return;
                }

                var file = new FileInfo(path);

                // Check if we are about to overwrite some files
                // and if so warn the user
                var files = new SourceCodeFile(file);
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

    private Type[] GetSupportedRootViews()
    {
        return new Type[] { typeof(Window), typeof(Dialog), typeof(View), typeof(Toplevel) };
    }

    private void New(FileInfo toOpen, Type typeToCreate, string? explicitNamespace)
    {
        var viewToCode = new ViewToCode();
        string? ns = explicitNamespace;

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
        if (string.IsNullOrWhiteSpace(ns) || ns.Contains(" ") || char.IsDigit(ns.First()))
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

        Task.Run(() =>
        {
            // Create the view files and compile
            instance = viewToCode.GenerateNewView(toOpen, ns ?? "YourNamespace", typeToCreate);
        }).ContinueWith(
            (t, o) =>
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
        this.SetNeedsDisplay();

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
