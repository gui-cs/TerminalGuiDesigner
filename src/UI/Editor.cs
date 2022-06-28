using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;
using YamlDotNet.Serialization;
using System.Text;

namespace TerminalGuiDesigner.UI;

public class Editor : Toplevel
{
    Design? _viewBeingEdited;
    private SourceCodeFile? _currentDesignerFile;
    private bool enableDrag = true;
    private bool enableShowFocused = true;

    bool _editting = false;

    readonly KeyMap _keyMap;

    KeyboardManager _keyboardManager;
    MultiSelectionManager _selectionManager = new();
    MouseManager _mouseManager;
    private bool _menuOpen;

    private string GetHelpWithNothingLoaded()
    {
        return @$"{_keyMap.ShowHelp} - Show Help
{_keyMap.New} - New Window/Class
{_keyMap.Open} - Open a .Designer.cs file";
    }
    private string GetHelpWithEmptyFormLoaded()
    {
        return @$"{_keyMap.AddView} to Add a View";
    }
    private string GetHelp()
    {

        return GetHelpWithNothingLoaded() + @$"
{_keyMap.Save} - Save an opened .Designer.cs file
{_keyMap.ShowContextMenu} - Show right click context menu;
{_keyMap.AddView} - Add View
{_keyMap.ToggleDragging} - Toggle mouse dragging on/off
{_keyMap.ToggleShowFocused} - Toggle show focused view field name
{_keyMap.EditProperties} - Edit View Properties
{_keyMap.ViewSpecificOperations} - View Specific Operations
{_keyMap.EditRootProperties} - Edit Root Properties
{_keyMap.Delete} - Delete selected View
Shift+Cursor - Move focused View
Ctrl+Cursor - Move focused View quickly
Ctrl+Q - Quit
{_keyMap.Undo} - Undo
{_keyMap.Redo} - Redo";

    }

    public Editor()
    {
        CanFocus = true;

        // If there are custom keybindings read those
        if(File.Exists("Keys.yaml"))
        {
            var d = new Deserializer();
            try
            {
                _keyMap = d.Deserialize<KeyMap>(File.ReadAllText("Keys.yaml"));
            }
            catch (Exception ex)
            {
                // if there is bad yaml use the defaults
                ExceptionViewer.ShowException("Failed to read keybindings",ex);
                _keyMap = new KeyMap();
            }
        }
        else
        {
            // otherwise use the defaults
            _keyMap = new KeyMap();
        }

        _keyboardManager = new KeyboardManager(_keyMap);
        _mouseManager = new MouseManager(_selectionManager);
    }

    public void Run(Options options)
    {
        if(!string.IsNullOrWhiteSpace(options.Path))
        {
            try
            {
                var toLoadOrCreate = new FileInfo(options.Path);

                if (toLoadOrCreate.Exists)
                {
                    Open(toLoadOrCreate);
                }
                else
                {
                    Type toCreate = typeof(Window);

                    if(!string.IsNullOrWhiteSpace(options.ViewType))
                    {
                        toCreate = GetSupportedRootViews().FirstOrDefault(v => v.Name.Equals(options.ViewType))??toCreate;
                    }

                    New(toLoadOrCreate, toCreate, options.Namespace);
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
            if (_editting)
                return false;

            try
            {
                return HandleKey(k);
            }
            catch (System.Exception ex)
            {
                ExceptionViewer.ShowException("Error processing keystroke", ex);
                return false;
            }
        };

        Application.RootMouseEvent += (m) =>
        {
            if(_editting || !enableDrag || _viewBeingEdited == null)
                return;

            try
            {
                _mouseManager.HandleMouse(m,_viewBeingEdited);

      
                //right click
                if(m.Flags.HasFlag(_keyMap.RightClick))
                {
                    var hit = _viewBeingEdited.View.HitTest(m, out _, out _);
                    
                    if(hit != null)
                    {
                        var d = hit.GetNearestDesign() ?? _viewBeingEdited;
                        if(d != null)
                            CreateAndShowContextMenu(m,d);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ExceptionViewer.ShowException("Error processing mouse",ex);
            }            
        };

        Application.Run(this,ErrorHandler);
        Application.Shutdown();
    }

    private bool CreateAndShowContextMenu()
    {
        var d = GetMostFocused(this)?.GetNearestDesign();
        if(d != null)
        {
            CreateAndShowContextMenu(null,d);
            return true;
        }    

        return false;        
    }

    private void CreateAndShowContextMenu(MouseEvent? m, Design d)
    {
        
        // things we can do/change
        IEnumerable<IOperation> options;

        if(m == null)
        {
            options = d.GetExtraOperations(_selectionManager).Where(c=>!c.IsImpossible);
        }
        else
        {
            options = d.GetExtraOperations(
                d.View.ScreenToClient(m.Value.X, m.Value.Y),
                _selectionManager                
                ).Where(c=>!c.IsImpossible);
        }
        
        
        var properties = d.GetDesignableProperties().OrderBy(p=>p.GetHumanReadableName());
        
        // menu items to click to make them happen/change
        var setPropertyMenuItems = properties.Select(p => new MenuItem(p.GetHumanReadableName(), null,
            () => Try(()=>EditDialog.SetPropertyToNewValue(d, p, p.GetValue())))).ToArray();
        
        var extraOptionsMenuItems = options.Select(o => new MenuItem(o.ToString(), "", ()=>Try(()=>OperationManager.Instance.Do(o)))).ToArray();

        MenuBarItem allItems;

        var propertiesCategory = new MenuBarItem(d.FieldName, setPropertyMenuItems);
        propertiesCategory.Action = ()=>{
            ShowEditProperties(d);
        };

        var items = new List<MenuItem>();
        items.Add(propertiesCategory);
        items.AddRange(extraOptionsMenuItems);

        allItems = new MenuBarItem(items.ToArray());
    
        var menu = new ContextMenu();
        menu.MenuItems = allItems;

        if(m.HasValue)
        {
            menu.Position = new Point(m.Value.X,m.Value.Y);
        }
        else
        {
            d.View.ViewToScreen(0,0,out var x, out var y);
            menu.Position = new Point(x,y);
        }

        _menuOpen = true;
        menu.Show();
        menu.MenuBar.MenuClosing += (m)=> _menuOpen = false;
    }

    private void Try(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            ExceptionViewer.ShowException("Operation failed",ex);
        }
    }

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);


        // if we are editing a view
        if(_viewBeingEdited != null)
        {
            if(enableShowFocused)
            {
                string? toDisplay = GetLowerRightTextIfAny();

                // and have a designable view focused
                if(toDisplay != null)
                {
                    // write its name in the lower right
                    int y = Bounds.Height -1;
                    int right = bounds.Width -1;
                    var len = toDisplay.Length;
                    
                    for(int i=0;i<len;i++)
                    {
                        AddRune(right -len +i,y,toDisplay[i]);
                    }
                }
            }

            
            if(_mouseManager.SelectionBox != null)
            {
                var box = _mouseManager.SelectionBox.Value;
                for(int x = 0;x<box.Width;x++)
                    for(int y = 0;y<box.Height;y++)
                { 	
                    if(y==0 || y == box.Height-1 || x==0 || x== box.Width-1)
                        AddRune(box.X + x,box.Y + y,'.');
                }
            }

            return;
        }

        // we are not editing a view (nothing is loaded)
        // so show the generic help (open, new etc)
        // in the center of the screen

        var lines = GetHelpWithNothingLoaded().Split('\n');

        Driver.SetAttribute(new Attribute(Color.DarkGray, Color.Black));

        int midX = Bounds.Width / 2;
        int midY = Math.Max(0,(Bounds.Height / 2) - (lines.Length/2)) -1;

        for (int y = 0 ; y < lines.Length ; y++)
        {
            var line = lines[y].TrimEnd();

            int startFromX = midX - line.Length / 2;

            for (int x = 0; x < line.Length; x++)
            {
                AddRune(startFromX + x, midY + y, line[x]);
            }
        }
    }

    private string? GetLowerRightTextIfAny()
    {
        if(MenuTracker.Instance.CurrentlyOpenMenuItem != null)
        {
            return $"Selected: {MenuTracker.Instance.CurrentlyOpenMenuItem.Title}";
        }

        var design = GetMostFocused(this).GetNearestDesign();

        if(design != null)
        {
            return $"Selected: {design.FieldName} ({_keyMap.EditProperties} to Edit, {_keyMap.ShowHelp} for Help)";
        }

        return  GetHelpWithEmptyFormLoaded();
    }

    public bool HandleKey(KeyEvent keyEvent)
    {
        // if another window is showing don't respond to hotkeys
        if (!IsCurrentTop)
            return false;

        if(_editting)
            return false;


        // Give the keyboard manager first shot at consuming
        // this key e.g. for typing into menus / reordering menus
        // etc
        if(_keyboardManager.HandleKey(GetMostFocused(this),keyEvent))
            return true;

        try
        {
            _editting = true;

            if(keyEvent.Key == _keyMap.ShowContextMenu && !_menuOpen)
            {
                return CreateAndShowContextMenu();
            }

            if(keyEvent.Key == _keyMap.EditProperties)
            {
                ShowEditProperties();
                return true;
            }

            if(keyEvent.Key == _keyMap.Copy)
            {
                Copy();
                return true;
            }

            if(keyEvent.Key == _keyMap.Paste)
            {
                Paste();
                return true;
            }

            if(keyEvent.Key == _keyMap.ViewSpecificOperations)
            {
                ShowViewSpecificOperations();
                return true;
            }

            if(keyEvent.Key == _keyMap.EditRootProperties)
            {
                if (_viewBeingEdited == null)
                    return false;
                ShowEditProperties(_viewBeingEdited);
                return true;
            }
            if(keyEvent.Key == _keyMap.Open)
            {
                Open();
                return true;
            }
            if(keyEvent.Key == _keyMap.Save)
            {
                Save();
                return true;
            }
            if(keyEvent.Key == _keyMap.New)
            {
                New();
                return true;
            }
            if(keyEvent.Key == _keyMap.ShowHelp)
            {
                ShowHelp();
                return true;
            }
            if(keyEvent.Key == _keyMap.AddView)
            {
                ShowAddViewWindow();
                return true;
            }
            if(keyEvent.Key == _keyMap.ToggleDragging)
            {
                enableDrag = !enableDrag;
                return true;
            }
            if(keyEvent.Key == _keyMap.Undo)
            {
                OperationManager.Instance.Undo();
                return true;
            }
            if(keyEvent.Key == _keyMap.Redo)
            {
                OperationManager.Instance.Redo();
                return true;
            }
            if(keyEvent.Key == _keyMap.Delete)
            {
                Delete();
                return true;
            }
            if(keyEvent.Key == _keyMap.ToggleShowFocused)
            {
                enableShowFocused = !enableShowFocused;
                SetNeedsDisplay();
            }
            if(keyEvent.Key == _keyMap.SelectAll)
            {
                SelectAll();
            }

            switch (keyEvent.Key)
            {
                // Cursor keys
                case Key.CursorUp | Key.ShiftMask:
                    MoveControl(0, -1);
                    return true;
                case Key.CursorUp | Key.CtrlMask:
                    MoveControl(0, -3);
                    return true;
                case Key.CursorDown | Key.ShiftMask:
                    MoveControl(0, 1);
                    return true;
                case Key.CursorDown | Key.CtrlMask:
                    MoveControl(0, 3);
                    return true;
                case Key.CursorLeft | Key.ShiftMask:
                    MoveControl(-1, 0);
                    return true;
                case Key.CursorLeft | Key.CtrlMask:
                    MoveControl(-5, 0);
                    return true;
                case Key.CursorRight | Key.ShiftMask:
                    MoveControl(1, 0);
                    return true;
                case Key.CursorRight | Key.CtrlMask:
                    MoveControl(5, 0);
                    return true;
            }
        }
        catch (System.Exception ex)
        {
            ExceptionViewer.ShowException("Error",ex);
        }
        finally
        {
            _editting = false;
        }

        return false;
    }

    private void SelectAll()
    {
        if(_viewBeingEdited == null)
            return;

        var everyone = _viewBeingEdited.GetAllDesigns()
            .Where(d=>!d.IsRoot)
            .ToArray();

        _selectionManager.SetSelection(everyone);
    }

    private void Paste()
    {
        var d = GetMostFocused(this)?.GetNearestContainerDesign() ?? _viewBeingEdited;

        if (d != null)
        {
            var paste = new PasteOperation(d,_selectionManager);

            if(paste.IsImpossible)
                return;

            OperationManager.Instance.Do(paste);
        }
    }

    private void Copy()
    {
        var d = GetMostFocused(this)?.GetNearestDesign();

        if (d != null)
        {
            var copy = new CopyOperation(d,_selectionManager);
            OperationManager.Instance.Do(copy);
        }
    }

    private void ShowViewSpecificOperations()
    {
        var d = GetMostFocused(this)?.GetNearestDesign();

        if (d != null)
        {
            var options = d.GetExtraOperations(_selectionManager).Where(o=>!o.IsImpossible).ToArray();

            if(options.Any() && Modals.Get("Operations","Ok",options, out var selected) && selected != null)
            {
                OperationManager.Instance.Do(selected);
            }
        }
    }

    private void ShowHelp()
    {
        MessageBox.Query("Help", GetHelp(), "Ok");
    }

    private void MoveControl(int deltaX, int deltaY)
    {
        DoForSelectedViews((d)=>new MoveViewOperation(d,deltaX,deltaY));
    }

    private void Delete()
    {
        DoForSelectedViews((d)=>new DeleteViewOperation(d.View));
    }

    private void DoForSelectedViews(Func<Design, Operation> operationFuc, bool allowOnRoot=false)
    {
        if(_viewBeingEdited == null)
            return;

        if(_selectionManager.Selected.Any())
        {
            var op = new CompositeOperation(
                _selectionManager.Selected
                .Select(operationFuc).ToArray());

            OperationManager.Instance.Do(op);
        }
        else
        {
            var singleSelection = GetMostFocused(_viewBeingEdited.View);
            var viewDesign = singleSelection?.GetNearestDesign();

            // don't delete the root view
            if (viewDesign != null)
            {
                if(viewDesign.IsRoot && !allowOnRoot)
                    return;

                OperationManager.Instance.Do(
                    operationFuc(viewDesign)
                );
            }
        }
    }

    private void Open()
    {
        var ofd = new OpenDialog("Open", $"Select {SourceCodeFile.ExpectedExtension} file",
            new List<string>(new[] { SourceCodeFile.ExpectedExtension }));
        
        Application.Run(ofd,ErrorHandler);

        if (!ofd.Canceled)
        {
            try
            {
                var path = ofd.FilePath.ToString();

                if (string.IsNullOrEmpty(path))
                    return;

                Open(new FileInfo(path));
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException($"Failed to open '{ofd.FilePath}'", ex);
            }
        }
    }

    private bool ErrorHandler(Exception arg)
    {
        ExceptionViewer.ShowException("Global Exception",arg);
        return true;
    }

    private void Open(FileInfo toOpen)
    {
        var open = new LoadingDialog(toOpen);

        // since we are opening a new view we should
        // clear the history
        OperationManager.Instance.ClearUndoRedo();
        Design? instance = null;
        _selectionManager.Clear();

        Task.Run(()=>{
            
            var decompiler = new CodeToView(new SourceCodeFile(toOpen));
            _currentDesignerFile = decompiler.SourceFile;
            instance = decompiler.CreateInstance();

        }).ContinueWith((t,o)=>{

            // no longer loading
            Application.MainLoop.Invoke(()=>Application.RequestStop());

            if (t.Exception != null)
            {
                Application.MainLoop.Invoke(() =>
                    ExceptionViewer.ShowException($"Failed to open '{toOpen.Name}'", t.Exception));
                return;
            }

            // if loaded correctly then 
            if (instance != null)
                ReplaceViewBeingEdited(instance);

        },TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open,ErrorHandler);
    }

    private void New()
    {

        if(!Modals.Get("Create New View","Ok",GetSupportedRootViews(),out var selected))
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
                    return;

                var file = new FileInfo(path);
        
                // Check if we are about to overwrite some files
                // and if so warn the user
                var files = new SourceCodeFile(file);
                var sb = new StringBuilder();
                
                if(files.CsFile.Exists)
                {
                    sb.AppendLine(files.CsFile.Name);
                }
                if(files.DesignerFile.Exists)
                {
                    sb.AppendLine(files.DesignerFile.Name);
                }

                if(sb.Length > 0)
                {
                    var chosen = MessageBox.Query("Overwrite Files?",$"The following files will be overwritten:{Environment.NewLine}{sb}","Ok","Cancel");
                    
                    // user cancelled
                    if(chosen != 0)
                        return;
                }

                New(file,selected,null);
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
                //user cancelled typing a namespace
                return;
            }
        }
        
        //Validate the namespace
        if (string.IsNullOrWhiteSpace(ns) || ns.Contains(" ") || char.IsDigit(ns.First()))
        {
            MessageBox.ErrorQuery("Invalid Namespace","Namespace must not contain spaces, be empty or begin with a number","Ok");
            return;
        }

        // since we are creating a new view we should
        // clear the history
        OperationManager.Instance.ClearUndoRedo();
        Design? instance = null;
        _selectionManager.Clear();

        var open = new LoadingDialog(toOpen);

        Task.Run(() => {

            // Create the view files and compile
            instance = viewToCode.GenerateNewView(toOpen, ns ?? "YourNamespace", typeToCreate, out _currentDesignerFile);

        }).ContinueWith((t, o) => {

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
                ReplaceViewBeingEdited(instance);

        }, TaskScheduler.FromCurrentSynchronizationContext());


        Application.Run(open,ErrorHandler);
    }

    private void ReplaceViewBeingEdited(Design design)
    {
        Application.MainLoop.Invoke(() =>
        {
            // remove the old view
            if (_viewBeingEdited != null)
            {
                // and dispose it
                Remove(_viewBeingEdited.View);
                _viewBeingEdited.View.Dispose();
            }

            // Load new instance
            _viewBeingEdited = design;

            // And add it to the editing window
            Add(_viewBeingEdited.View);
        });
    }
    private void Save()
    {
        if (_viewBeingEdited == null || _currentDesignerFile == null)
            return;

        var viewToCode = new ViewToCode();

        viewToCode.GenerateDesignerCs(
            _viewBeingEdited, _currentDesignerFile,
            _viewBeingEdited.View.GetType().BaseType ?? throw new Exception("View being edited had no base class"));
    }
    private void ShowAddViewWindow()
    {
        if (_viewBeingEdited == null || _currentDesignerFile == null)
        {
            return;
        }

        // what is the currently selected design
        var toAddTo = GetMostFocused(_viewBeingEdited.View)?.GetNearestContainerDesign() ?? _viewBeingEdited;

        OperationManager.Instance.Do(
            new AddViewOperation(_currentDesignerFile,toAddTo)
        );
    }


    private void ShowEditProperties()
    {
        var d = GetMostFocused(this).GetNearestDesign();
        if (d != null)
        {
            ShowEditProperties(d);
        }
    }

    private void ShowEditProperties(Design d)
    {
        var edit = new EditDialog(d);
        Application.Run(edit,ErrorHandler);
    }

    

    private View GetMostFocused(View view)
    {
        if (view.Focused == null)
        {
            return view;
        }

        return GetMostFocused(view.Focused);
    }

}
