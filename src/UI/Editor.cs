using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.ToCode;
using System.Text.RegularExpressions;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI;

public class Editor : Toplevel
{
    Design? _viewBeingEdited;
    private SourceCodeFile? _currentDesignerFile;
    private bool enableDrag = true;
    DragOperation? dragOperation = null;
    bool _editting = false;

    const string HelpWithNothingLoaded = @"F1/Ctrl+H - Show Help
Ctrl+N - New View
Ctrl+O - Open a .Designer.cs file";

    const string Help = @"
Ctrl+S - Save an opened .Designer.cs file
F2 - Add Control
F3 - Toggle mouse dragging on/off
F4/Enter - Properties
Shift+F4/Enter - View Specific Operations
F5 - Edit Root Properties
Del - Delete selected View
Shift+Cursor - Move focused View
Ctrl+Cursor - Move focused View quickly
Ctrl+Q - Quit
Ctrl+Z - Undo
Ctrl+Y - Redo";

    public Editor()
    {
        CanFocus = true;
    }

    public void Run(string? fileToLoad)
    {
        Application.Init();

        if(fileToLoad != null)
        {
            try
            {
                var toLoadOrCreate = new FileInfo(fileToLoad);

                if(toLoadOrCreate.Exists)
                {
                    Open(toLoadOrCreate);
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
            var drag = HitTest(_viewBeingEdited.View, m);

            if (drag == null)
            {
                return;
            }

            if (drag.Data is Design design)
            {
                var dest = ScreenToClient(_viewBeingEdited.View, m.X, m.Y);
                dragOperation = new DragOperation(design, drag.X, drag.Y, dest.X, dest.Y);
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
    }

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);

        if(_viewBeingEdited != null)
            return;

        var lines = HelpWithNothingLoaded.Split('\n');
        var tf = new TextFormatter();
        var width = Frame.Width;
        var grey = new Attribute(Color.Black);

        for(int y = 0 ; y < lines.Length ; y++)
        {
            tf.Text = lines[y];
            tf.Alignment = TextAlignment.Centered;
            tf.Draw(new Rect(0,y+2,width,1),grey,grey);
        }
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

            switch (keyEvent.Key)
            {
                case Key.F1:
                case Key.H | Key.CtrlMask:
                    ShowHelp();
                    return true;
                case Key.F2:
                    ShowAddViewWindow();
                    return true;

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
                case Key.F3:
                    enableDrag = !enableDrag;
                    return true;
                case Key.Enter:
                case Key.F4:
                    ShowEditPropertiesWindow();
                    return true;
                case Key.F5:
                    if (_viewBeingEdited == null)
                        return false;

                    ShowEditPropertiesWindow(_viewBeingEdited);
                    return true;
                case Key.Enter | Key.ShiftMask:
                case Key.F4 | Key.ShiftMask:
                    ShowExtraOptions();
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

    private void ShowExtraOptions()
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
        MessageBox.Query("Help", HelpWithNothingLoaded + Help, "Ok");
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

            // if loaded correctly then 
            if(instance != null)
                ReplaceViewBeingEdited(instance);

        },TaskScheduler.FromCurrentSynchronizationContext());

        Application.Run(open);
    }

    private void New()
    {

        if(!Modals.Get("Create New View","Ok",new Type[]{typeof(Window),typeof(Dialog)},out var selected))
        {
            return;
        }

        var ofd = new SaveDialog("New", $"Class file",
            new List<string>(new[] { ".cs" }))
        {
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

                New(new FileInfo(path),selected);
            }
            catch (Exception ex)
            {
                ExceptionViewer.ShowException($"Failed to create '{ofd.FilePath}'", ex);
                throw;
            }
        }
    }

    private void New(FileInfo toOpen, Type typeToCreate)
    {
        var viewToCode = new ViewToCode();

        var design = viewToCode.GenerateNewView(toOpen, "YourNamespace", typeToCreate, out _currentDesignerFile);
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
        Add(_viewBeingEdited.View);
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

    private void ShowEditPropertiesWindow()
    {
        var d = GetMostFocused(this).GetNearestDesign();
        if (d != null)
        {
            ShowEditPropertiesWindow(d);
        }
    }

    private void ShowEditPropertiesWindow(Design d)
    {
        var edit = new EditDialog(d);
        Application.Run(edit);
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
