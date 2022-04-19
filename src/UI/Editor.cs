using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;
using YamlDotNet.Serialization;

namespace TerminalGuiDesigner.UI;

public class Editor : Toplevel
{
    Design? _viewBeingEdited;
    private SourceCodeFile? _currentDesignerFile;
    private bool enableDrag = true;
    private bool enableShowFocused = true;
    DragOperation? dragOperation = null;
    ResizeOperation? resizeOperation = null;

    bool _editting = false;

    readonly KeyMap _keyMap;

    KeyboardManager _keyboardManager = new ();

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

        Application.RootMouseEvent += (m) =>
        {
            if(_editting)
                return;

            try
            {
                HandleMouse(m);
            }
            catch (System.Exception ex)
            {
                ExceptionViewer.ShowException("Error processing mouse",ex);
            }            
        };

        Application.Run(this);
        Application.Shutdown();
    }

    private void HandleMouse(MouseEvent m)
    {
        if (!enableDrag || _viewBeingEdited == null)
        {
            return;
        }

        // start dragging
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragOperation == null)
        {
            var drag = HitTest(_viewBeingEdited.View, m, out bool isLowerRight);


            // if nothing is going on yet
            if (drag != null && drag.Data is Design design && resizeOperation == null && dragOperation == null)
            {
                var dest = ScreenToClient(_viewBeingEdited.View, m.X, m.Y);
                
                if (isLowerRight)
                {
                    resizeOperation = new ResizeOperation(design, dest.X, dest.Y);
                }
                else
                {
                    dragOperation = new DragOperation(design, dest.X, dest.Y);
                }
                
            }
        }

        // continue dragging
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragOperation != null)
        {
            var dest = ScreenToClient(_viewBeingEdited.View, m.X, m.Y);

            dragOperation.ContinueDrag(dest);

            _viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
        }

        // continue resizing
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && resizeOperation != null)
        {
            var dest = ScreenToClient(_viewBeingEdited.View, m.X, m.Y);

            resizeOperation.ContinueResize(dest);

            _viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
        }

        // end dragging
        if (!m.Flags.HasFlag(MouseFlags.Button1Pressed))
        {
            if ( dragOperation != null)
            {
                // end drag
                OperationManager.Instance.Do(dragOperation);
                dragOperation = null;
            }

            if (resizeOperation != null)
            {
                // end resize
                OperationManager.Instance.Do(resizeOperation);
                resizeOperation = null;
            }

        }
        
        //right click
        if(m.Flags.HasFlag(_keyMap.RightClick))
        {
            var hit = HitTest(_viewBeingEdited.View, m, out _);
            
            if(hit != null)
            {
                var d = hit.GetNearestDesign();
                if(d != null)
                    CreateAndShowContextMenu(m,d);
            }

        }
    }

    private void CreateAndShowContextMenu(MouseEvent m, Design d)
    {
        var options = d.GetExtraOperations(ScreenToClient(d.View, m.X, m.Y));

        if(options.Any())
        {
            var menu = new ContextMenu(d.View, new MenuBarItem(
                options.Select(o=>new MenuItem(o.ToString(),"",o.Do)).ToArray()));
            menu.Show();

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
        var design = GetMostFocused(this).GetNearestDesign();

        if(design != null)
        {
            return $"Selected: {design.FieldName} ({_keyMap.EditProperties} to Edit, {_keyMap.ShowHelp} for Help)";
        }

        return  GetHelpWithEmptyFormLoaded();
    }

    public override bool ProcessHotKey(KeyEvent keyEvent)
    {
        // if another window is showing don't respond to hotkeys
        if (!IsCurrentTop)
            return false;

        if(_editting)
            return false;

        try
        {
            _editting = true;

            if(keyEvent.Key == _keyMap.EditProperties)
            {
                ShowEditProperties();
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

            return _keyboardManager.HandleKey(GetMostFocused(this),keyEvent);
        }
        catch (System.Exception ex)
        {
            ExceptionViewer.ShowException("Error",ex);
        }
        finally
        {
            _editting = false;
        }

        return base.ProcessHotKey(keyEvent);
    }

    private void ShowViewSpecificOperations()
    {
        var d = GetMostFocused(this)?.GetNearestDesign();

        if (d != null)
        {
            var options = d.GetExtraOperations().Where(o=>!o.IsImpossible).ToArray();

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
        var view = GetMostFocused(this);

        if (view.Data is Design d)
        {
            d.View.X = Math.Min(Math.Max(d.View.Frame.Left + deltaX, 0), view.SuperView.Bounds.Width - 1);
            d.View.Y = Math.Min(Math.Max(d.View.Frame.Top + deltaY, 0), view.SuperView.Bounds.Height - 1);
        }
    }

    private void Delete()
    {
        if (_viewBeingEdited == null)
            return;

        var viewToDelete = GetMostFocused(_viewBeingEdited.View);
        var viewDesign = viewToDelete?.GetNearestDesign();

        // don't delete the root view
        if(viewDesign != null && viewDesign != _viewBeingEdited)
        {
            OperationManager.Instance.Do(
                new DeleteViewOperation(viewDesign.View)
            );
        }

    }
    private void Open()
    {
        var ofd = new OpenDialog("Open", $"Select {SourceCodeFile.ExpectedExtension} file",
            new List<string>(new[] { SourceCodeFile.ExpectedExtension }));

        Application.Run(ofd);

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
    private void Open(FileInfo toOpen)
    {
        var open = new LoadingDialog(toOpen);

        // since we are opening a new view we should
        // clear the history
        OperationManager.Instance.ClearUndoRedo();
        Design? instance = null;
       
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

        Application.Run(open);
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

                New(new FileInfo(path),selected,null);
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
        
        //TODO: Validate the namespace

        // if we have a valid namespace
        if (!string.IsNullOrWhiteSpace(ns))
        {
            // Create the view and open it
            var design = viewToCode.GenerateNewView(toOpen, ns ?? "YourNamespace", typeToCreate, out _currentDesignerFile);
            ReplaceViewBeingEdited(design);
        }
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

        var factory = new ViewFactory();
        var selectable = factory.GetSupportedViews().ToArray();
            
        if (Modals.Get("Type of Control", "Add", true, selectable, t => t?.Name ?? "Null", false, out var selected) && selected != null)
        {
            var instance = factory.Create(selected);

            OperationManager.Instance.Do(
                new AddViewOperation(_currentDesignerFile, instance, toAddTo, GetUniqueFieldName(selected))
            );
        }
    }

    /// <summary>
    /// Returns a new unique name for a view of type <paramref name="viewType"/>
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="viewType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private string GetUniqueFieldName(Type viewType)
    {
        if (_viewBeingEdited == null)
            throw new Exception("Cannot generate unique field name because no view is being edited");

        var allDesigns = _viewBeingEdited.GetAllDesigns();

        // consider label1
        int number = 1;
        while (allDesigns.Any(d => d.FieldName.Equals($"{viewType.Name.ToLower()}{number}")))
        {
            // label1 is taken, try label2 etc
            number++;
        }

        // found a unique one
        return $"{viewType.Name.ToLower()}{number}";
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
        Application.Run(edit);
    }

    private View? HitTest(View w, MouseEvent m, out bool isLowerRight)
    {

        var point = ScreenToClient(w, m.X, m.Y);
        var hit = ApplicationExtensions.FindDeepestView(w, m.X, m.Y);

        if (hit != null)
        {
            isLowerRight = hit.Frame.Right - 1 == point.X && hit.Frame.Bottom - 1 == point.Y;
        }
        else
            isLowerRight = false;

        return hit;
    }

    private View GetMostFocused(View view)
    {
        if (view.Focused == null)
        {
            return view;
        }

        return GetMostFocused(view.Focused);
    }


    private Point ScreenToClient(View view, int x, int y)
    {
        if (view is Window w)
        {
            // has invisible ContentView pane
            return w.Subviews[0].ScreenToView(x, y);
        }

        return view.ScreenToView(x, y);
    }
}
