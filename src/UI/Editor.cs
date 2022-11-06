using System.Text;
using Terminal.Gui;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;
using YamlDotNet.Serialization;

namespace TerminalGuiDesigner.UI;

public class Editor : Toplevel
{
    Design? _viewBeingEdited;
    private bool enableDrag = true;
    private bool enableShowFocused = true;
    public static bool ShowBorders = true;

    bool editting = false;

    readonly KeyMap keyMap;

    KeyboardManager keyboardManager;
    MouseManager mouseManager;
    ListView? rootCommandsListView;
    private bool menuOpen;

    /// <summary>
    /// Set this to have a short duration message appear in lower right
    /// (see <see cref="GetLowerRightTextIfAny"/>)
    /// </summary>
    private string? flashMessage = null;

    /// <summary>
    /// The <see cref="IOperation.UniqueIdentifier"/> of the last undertaken
    /// operation at the time of the last save or null if no save or last save
    /// was before applying any operations.
    /// </summary>
    private Guid? lastSavedOperation;

    /// <summary>
    /// True to enable experimental features
    /// </summary>
    public static bool Experimental { get; internal set; }

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

    public Editor()
    {
        this.CanFocus = true;

        // If there are custom keybindings read those
        if (File.Exists("Keys.yaml"))
        {
            var d = new Deserializer();
            try
            {
                this.keyMap = d.Deserialize<KeyMap>(File.ReadAllText("Keys.yaml"));

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

    private void BuildRootMenu()
    {
        // setup views for when we are not editing a
        // view (nothing is loaded) so show the generic
        // help (open, new etc) in the center of the
        // screen

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
            rootCommands[i] = this.PadBoth(rootCommands[i], maxWidth);
        }

        this.rootCommandsListView = new ListView(rootCommands)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = maxWidth,
            Height = 3,
            ColorScheme = new DefaultColorSchemes().GetDefaultScheme("greyOnBlack").Scheme,
        };

        this.rootCommandsListView.KeyDown += (e) =>
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

    public string PadBoth(string source, int length)
    {
        int spaces = length - source.Length;
        int padLeft = spaces / 2 + source.Length;
        return source.PadLeft(padLeft).PadRight(length);
    }

    private void Editor_Closing(ToplevelClosingEventArgs obj)
    {
        if (this._viewBeingEdited == null)
        {
            return;
        }

        if (this.HasUnsavedChanges())
        {
            int answer = MessageBox.Query("Unsaved Changes", $"You have unsaved changes to {this._viewBeingEdited.SourceCode.DesignerFile.Name}", "Save", "Don't Save", "Cancel");

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

        Application.RootKeyEvent += (k) =>
        {
            if (this.editting)
            {
                return false;
            }

            try
            {
                return this.HandleKey(k);
            }
            catch (System.Exception ex)
            {
                ExceptionViewer.ShowException("Error processing keystroke", ex);
                return false;
            }
        };

        Application.RootMouseEvent += (m) =>
        {
            // if another window is showing don't respond to mouse
            if (!this.IsCurrentTop)
            {
                return;
            }

            if (this.editting || !this.enableDrag || this._viewBeingEdited == null)
            {
                return;
            }

            try
            {
                this.mouseManager.HandleMouse(m, this._viewBeingEdited);

                // right click
                if (m.Flags.HasFlag(this.keyMap.RightClick))
                {
                    var hit = this._viewBeingEdited.View.HitTest(m, out _, out _);

                    if (hit != null)
                    {
                        var d = hit.GetNearestDesign() ?? this._viewBeingEdited;
                        if (d != null)
                        {
                            this.CreateAndShowContextMenu(m, d);
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

    private void CreateAndShowContextMenu(MouseEvent? m, Design? rightClicked)
    {
        if (this._viewBeingEdited == null)
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
        var others = operations.Except(setProps);

        var setPropsItems = setProps.Select(this.ToMenuItem);
        var othersItems = others.Select(this.ToMenuItem);

        var all = new List<MenuItem>();

        // only add the set properties category if there are some
        if (setPropsItems.Any())
        {
            all.Add(new MenuBarItem(name, setPropsItems.ToArray())
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

        all.AddRange(othersItems.ToArray());

        // theres nothing we can do
        if (all.Count == 0)
        {
            return;
        }

        var menu = new ContextMenu();
        menu.MenuItems = new MenuBarItem(all.ToArray());

        if (m.HasValue)
        {
            menu.Position = new Point(m.Value.X, m.Value.Y);
        }
        else
        {
            var d = SelectionManager.Instance.Selected.FirstOrDefault() ?? this._viewBeingEdited;
            d.View.ViewToScreen(0, 0, out var x, out var y);
            menu.Position = new Point(x, y);
        }

        this.menuOpen = true;
        SelectionManager.Instance.LockSelection = true;
        menu.Show();
        menu.MenuBar.MenuAllClosed += () =>
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

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);

        // if we are editing a view
        if (this._viewBeingEdited != null)
        {
            if (this.enableShowFocused)
            {
                Application.Driver.SetAttribute(this._viewBeingEdited.View.ColorScheme.Normal);

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
                        this.AddRune(right - len + i, y, toDisplay[i]);
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
                            this.AddRune(box.X + x, box.Y + y, '.');
                        }
                    }
                }
            }

            return;
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

    public bool HandleKey(KeyEvent keyEvent)
    {
        // if another window is showing don't respond to hotkeys
        if (!this.IsCurrentTop)
        {
            return false;
        }

        if (this.editting)
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
            this.editting = true;
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
                if (this._viewBeingEdited == null)
                {
                    return false;
                }

                this.ShowEditProperties(this._viewBeingEdited);
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
            this.editting = false;
        }

        return false;
    }

    private void SelectAll()
    {
        if (this._viewBeingEdited == null)
        {
            return;
        }

        var everyone = this._viewBeingEdited.GetAllDesigns()
            .Where(d => !d.IsRoot)
            .ToArray();

        SelectionManager.Instance.ForceSetSelection(everyone);
    }

    private void Paste()
    {
        var d = SelectionManager.Instance.GetMostSelectedContainerOrNull() ?? this._viewBeingEdited;

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

            if (options.Any() && Modals.Get("Operations", "Ok", options, out var selected) && selected != null)
            {
                OperationManager.Instance.Do(selected);
            }
        }
    }

    private void ShowHelp()
    {
        MessageBox.Query("Help", this.GetHelp(), "Ok");
    }

    private void MoveControl(int deltaX, int deltaY)
    {
        this.DoForSelectedViews((d) => new MoveViewOperation(d, deltaX, deltaY));
    }

    private void Delete()
    {
        if (this._viewBeingEdited == null)
        {
            return;
        }

        if (SelectionManager.Instance.Selected.Any())
        {
            var cmd = new DeleteViewOperation(SelectionManager.Instance.Selected.Select(d => d.View).ToArray());
            OperationManager.Instance.Do(cmd);
        }
    }

    private void DoForSelectedViews(Func<Design, Operation> operationFuc, bool allowOnRoot = false)
    {
        if (this._viewBeingEdited == null)
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
        var ofd = new OpenDialog("Open", $"Select {SourceCodeFile.ExpectedExtension} file",
            new List<string>(new[] { SourceCodeFile.ExpectedExtension }));

        Application.Run(ofd, this.ErrorHandler);

        if (!ofd.Canceled)
        {
            try
            {
                var path = ofd.FilePath.ToString();

                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                this.Open(new FileInfo(path));
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException($"Failed to open '{ofd.FilePath}'", ex);
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
            Application.MainLoop.Invoke(() => Application.RequestStop());

            if (t.Exception != null)
            {
                Application.MainLoop.Invoke(() =>
                    ExceptionViewer.ShowException($"Failed to open '{toOpen.Name}'", t.Exception));
                return;
            }

            // if loaded correctly then
            if (instance != null)
            {
                this.ReplaceViewBeingEdited(instance);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open, this.ErrorHandler);
    }

    private void New()
    {
        if (!Modals.Get("Create New View", "Ok", this.GetSupportedRootViews(), out var selected))
        {
            return;
        }

        var ofd = new SaveDialog("New", $"Class file",
            new List<string>(new[] { ".cs" }))
        {
            NameDirLabel = "Directory",
            NameFieldLabel = "Class",
            FilePath = "MyView.cs",
            AllowsOtherFileTypes = false,
        };

        Application.Run(ofd);

        if (!ofd.Canceled)
        {
            try
            {
                var path = ofd.FilePath.ToString();

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
                    if (!ConfirmDialog.Show("Overwrite Files?", $"The following files will be overwritten:{Environment.NewLine}{sb.ToString().TrimEnd()}", "Ok", "Cancel"))
                    {
                        return; // user cancelled overwrite
                    }
                }

                this.New(file, selected, null);
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException($"Failed to create '{ofd.FilePath}'", ex);
                throw;
            }
        }
    }

    private Type[] GetSupportedRootViews()
    {
        // TODO: When more robust, remove these from experimental status
        if (Editor.Experimental)
        {
            return new Type[] { typeof(Window), typeof(Dialog), typeof(View), typeof(Toplevel) };
        }

        return new Type[] { typeof(Window), typeof(Dialog) };
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
            Application.MainLoop.Invoke(() => Application.RequestStop());

            if (t.Exception != null)
            {
                Application.MainLoop.Invoke(() =>
                    ExceptionViewer.ShowException($"Failed to create '{toOpen.Name}'", t.Exception));
                return;
            }

            // if loaded correctly then
            if (instance != null)
            {
                this.ReplaceViewBeingEdited(instance);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open, this.ErrorHandler);
    }

    private void ReplaceViewBeingEdited(Design design)
    {
        Application.MainLoop.Invoke(() =>
        {
            // remove the old view
            if (this._viewBeingEdited != null)
            {
                // and dispose it
                this.Remove(this._viewBeingEdited.View);
                this._viewBeingEdited.View.Dispose();
            }

            // remove list view to prevent it stealing keystrokes and jumping back
            // into input focus
            this.Remove(this.rootCommandsListView);

            // Load new instance
            this._viewBeingEdited = design;

            // TODO: Find a better place for this
            ColorSchemeManager.Instance.FindDeclaredColorSchemes(this._viewBeingEdited);

            // And add it to the editing window
            this.Add(this._viewBeingEdited.View);
        });
    }

    private void Save()
    {
        if (this._viewBeingEdited == null)
        {
            return;
        }

        var viewToCode = new ViewToCode();

        viewToCode.GenerateDesignerCs(
            this._viewBeingEdited,
            this._viewBeingEdited.View.GetType().BaseType ?? throw new Exception("View being edited had no base class"));

        this.flashMessage = $"Saved {this._viewBeingEdited.SourceCode.DesignerFile.Name}";
        this.SetNeedsDisplay();

        this.lastSavedOperation = OperationManager.Instance.GetLastAppliedOperation()?.UniqueIdentifier;
    }

    public bool HasUnsavedChanges()
    {
        var savedOp = this.lastSavedOperation;
        var currentOp = OperationManager.Instance.GetLastAppliedOperation()?.UniqueIdentifier;

        // if we have nothing saved
        if (savedOp == null)
        {
            // then we must save if we have done something
            return currentOp != null;
        }

        // we must save if the head of the operations stack doesn't match what we saved
        // this lets us save, perform action, undo action and then still consider us saved
        return savedOp != currentOp;
    }

    private void ShowAddViewWindow()
    {
        if (this._viewBeingEdited == null)
        {
            return;
        }

        // what is the currently selected design
        var toAddTo = SelectionManager.Instance.GetMostSelectedContainerOrNull() ?? this._viewBeingEdited;

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
        if (this._viewBeingEdited == null)
        {
            return;
        }

        var schemes = new ColorSchemesUI(this._viewBeingEdited);
        Application.Run(schemes);
    }
}
