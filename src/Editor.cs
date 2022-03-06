using System.Reflection;
using Terminal.Gui;
using System.Text;
using TerminalGuiDesigner.Windows;
using TerminalGuiDesigner.Operations;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

internal class Editor : Toplevel
{
    Design? _viewBeingEdited;
    private SourceCodeFile _currentDesignerFile;
    private bool enableDrag = true;

    const string Help = @"F1 - Show this help
F2 - Add Control
F3 - Toggle mouse dragging on/off
Enter - Edit Selected Control Properties
Del - Delete selected View
Shift+Cursor - Move focused View
Ctrl+Cursor - Move focused View quickly
Ctrl+O - Open a .Designer.cs file
Ctrl+S - Save an opened .Designer.cs file
Ctrl+N - New View
Ctrl+Q - Quit
Ctrl+Z - Undo
Ctrl+Y - Redo";

    public Editor()
    {
        CanFocus = true;
    }

    public void Run()
    {
        Application.Init();


        try
        {
            var programDir = Assembly.GetEntryAssembly()?.Location ?? throw new Exception("Could not determine the current executables present directory");

            var classFile = new FileInfo(Path.Combine(programDir, "../../../../MyWindow.cs"));
            var designerFile = new FileInfo(Path.Combine(programDir, "../../../../MyWindow.Designer.cs"));

            Open(designerFile);

            
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error Loading Designer", ex.Message, "Ok");
            Application.Shutdown();
            return;
        }

        DragOperation dragOperation = null;

        Application.RootMouseEvent += (m) => {

            if(!enableDrag)
            {
                return;
            }

            // start dragging
            if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragOperation == null)
            {
                var drag = HitTest(_viewBeingEdited.View, m);

                if(drag != null)
                {
                    var dest = ScreenToClient(_viewBeingEdited.View, m.X, m.Y);
                    dragOperation = new DragOperation(drag, drag.X, drag.Y, dest.X, dest.Y);
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

            // end dragging
            if (!m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragOperation != null)
            {
                // push it onto the undo stack
                OperationManager.Instance.Do(dragOperation);
                dragOperation = null;
            }
        };
        ShowHelp();

        Application.Run(this);
        Application.Shutdown();

    }

    public override bool ProcessHotKey(KeyEvent keyEvent)
    {
        // if another window is showing don't respond to hotkeys
        if (!IsCurrentTop)
            return false;

        switch(keyEvent.Key)
        {
            case Key.F1:
                ShowHelp();
                return true;
            case Key.F2:
                ShowAddViewWindow();
                return true;

            // Cursor keys
            case Key.CursorUp | Key.ShiftMask:
                MoveControl(0,-1);
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
                MoveControl(1,0);
                return true;
            case Key.CursorRight | Key.CtrlMask:
                MoveControl(5, 0);
                return true;
            case Key.F3:
                enableDrag = !enableDrag;
                return true;
            case Key.Enter:
                ShowEditPropertiesWindow();
                return true;
            case Key.DeleteChar:
                Delete();
                return true;
            case Key.CtrlMask | Key.O:
                Open();
                return true;
            case Key.CtrlMask | Key.S:
                Save();
                return true;
            case Key.CtrlMask | Key.N:
                New();
                return true;
            case Key.CtrlMask | Key.Z:
                OperationManager.Instance.Undo();
                return true;
            case Key.CtrlMask | Key.Y:
                OperationManager.Instance.Redo();
                return true;
        }

        return base.ProcessHotKey(keyEvent);
    }

    private void ShowHelp()
    {
        MessageBox.Query("Help", Help, "Ok");
    }

    private void MoveControl(int deltaX, int deltaY)
    {
        var view = GetMostFocused(this);

        // TODO: only if using absolute positioning

        if (view.Data is Design d)
        {
            d.View.X = Math.Min(Math.Max(d.View.Frame.Left + deltaX, 0),view.SuperView.Bounds.Width-1);
            d.View.Y = Math.Min(Math.Max(d.View.Frame.Top + deltaY, 0), view.SuperView.Bounds.Height - 1);
        }
    }

    private void Delete()
    {
        if (_viewBeingEdited == null)
            return;

        var viewToDelete = GetMostFocused(_viewBeingEdited.View);

        OperationManager.Instance.Do(
            new DeleteViewOperation(viewToDelete)
        );
    }
    private void Open()
    {
        var ofd = new OpenDialog("Open",$"Select {SourceCodeFile.ExpectedExtension} file",
            new List<string>(new []{SourceCodeFile.ExpectedExtension}));
        Application.Run(ofd);
        
        if(ofd.FilePath != null)
        {
            Open(new FileInfo(ofd.FilePath.ToString()));
        }
    }
    private void Open(FileInfo toOpen)
    {
        // since we are opening a new view we should
        // clear the history
        OperationManager.Instance.ClearUndoRedo();

        var decompiler = new CodeToView(toOpen);

        _currentDesignerFile = decompiler.SourceFile;

        ReplaceViewBeingEdited(decompiler.CreateInstance());
    }

    private void New()
    {
        var ofd = new SaveDialog("New", $"Class file",
            new List<string>(new[] { ".cs" }));
        Application.Run(ofd);

        if (ofd.FilePath != null)
        {
            New(new FileInfo(ofd.FilePath.ToString()));
        }
    }

    private void New(FileInfo toOpen)
    {
        var viewToCode = new ViewToCode();
        var design = viewToCode.GenerateNewWindow(toOpen, "YourNamespace", out _currentDesignerFile);

        ReplaceViewBeingEdited(design);
    }

    private void ReplaceViewBeingEdited(Design design)
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
        this.Add(_viewBeingEdited.View);
    }
    private void Save()
    {
        var viewToCode = new ViewToCode();
        viewToCode.GenerateDesignerCs(
            _viewBeingEdited.View, _currentDesignerFile);
    }
    private void ShowAddViewWindow()
    {
        if(_viewBeingEdited == null)
        {
            return;
        }
        var selectable = typeof(View).Assembly.DefinedTypes.Where(t => typeof(View).IsAssignableFrom(t)).ToArray();
        
        if(Modals.Get("Type of Control","Add",true,selectable,t=>t.Name,false, out Type selected))
        {
            var factory = new ViewFactory();
            var instance = factory.Create(selected);

            OperationManager.Instance.Do(
                new AddViewOperation(_currentDesignerFile, instance,_viewBeingEdited,GetUniqueFieldName(selected))
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
        var allDesigns = _viewBeingEdited.GetAllDesigns();

        // consider label1
        int number = 1;
        while(allDesigns.Any(d=>d.FieldName.Equals($"{viewType.Name.ToLower()}{number}")))
        {
            // label1 is taken, try label2 etc
            number++;
        }

        // found a unique one
        return $"{viewType.Name.ToLower()}{number}";
    }

    private void ShowEditPropertiesWindow()
    {
        var view = GetMostFocused(this);
        if(view.Data is Design d)
        {
            var edit = new EditDialog(d);
            Application.Run(edit);
        }
    }

    private View? HitTest(View w, MouseEvent m)
    {
        var point = ScreenToClient(w, m.X, m.Y);
        return w.GetActualSubviews().FirstOrDefault(v => v.Frame.Contains(point));
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
