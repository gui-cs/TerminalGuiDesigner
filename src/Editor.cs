using System.Reflection;
using Terminal.Gui;
using System.Text;
using TerminalGuiDesigner.Windows;

namespace TerminalGuiDesigner;

internal class Editor : Toplevel
{
    Design? _viewBeingEdited;
    private FileInfo _currentDesignerFile;

    const string Help = @"F1 - Show this help
F2 - Add Control
F4 - Edit Selected Control Properties
Del - Delete selected View
Ctrl+O - Open a .Designer.cs file
Ctrl+S - Save an opened .Designer.cs file
Ctrl+N - New View";

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

        View? dragging = null;
        
        Application.RootMouseEvent += (m) => {

            // start dragging
            if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging == null)
            {
                dragging = HitTest(_viewBeingEdited.View, m);
            }

            // continue dragging
            if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging != null)
            {
                var dest = ScreenToClient(_viewBeingEdited.View, m.X, m.Y);
                dragging.X = dest.X;
                dragging.Y = dest.Y;

                _viewBeingEdited.View.SetNeedsDisplay();
                Application.DoEvents();
            }

            // end dragging
            if (!m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging != null)
            {
                dragging = null;
            }
        };

        Application.Run(this);
        Application.Shutdown();

    }

    public override bool ProcessHotKey(KeyEvent keyEvent)
    {
        switch(keyEvent.Key)
        {
            case Key.F1: MessageBox.Query("Help", Help, "Ok");
                return true;
            case Key.F2:
                ShowAddViewWindow();
                return true;
            case Key.F4:
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
        }

        return base.ProcessHotKey(keyEvent);
    }


    private void Delete()
    {
        if (_viewBeingEdited == null)
            return;

        var viewToDelete = GetMostFocused(_viewBeingEdited.View);
        _viewBeingEdited.RemoveDesign(viewToDelete);
    }
    private void Open()
    {
        var ofd = new OpenDialog("Open",$"Select {CodeToView.ExpectedExtension} file",
            new List<string>(new []{CodeToView.ExpectedExtension}));
        Application.Run(ofd);
        
        if(ofd.FilePath != null)
        {
            Open(new FileInfo(ofd.FilePath.ToString()));
        }
    }
    private void Open(FileInfo toOpen)
    {
        var decompiler = new CodeToView(toOpen);

        _currentDesignerFile = toOpen;

        // remove the old view
        if(_viewBeingEdited != null)
        {
            // and dispose it
            Remove(_viewBeingEdited.View);
            _viewBeingEdited.View.Dispose();
        }

        // Load new instance
        _viewBeingEdited = decompiler.CreateInstance();

        // And add it to the editing window
        this.Add(_viewBeingEdited.View);
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
        var design = viewToCode.GenerateNewWindow(toOpen, "Your Namespace");

        _currentDesignerFile = new FileInfo(Path.GetFileNameWithoutExtension(toOpen.Name) + CodeToView.ExpectedExtension);

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
            _viewBeingEdited.View, _currentDesignerFile, _viewBeingEdited.GetType().Namespace);
    }
    private void ShowAddViewWindow()
    {
        if(_viewBeingEdited == null)
        {
            return;
        }
        var selectable = typeof(View).Assembly.DefinedTypes.Where(t => typeof(View).IsAssignableFrom(t)).ToArray();

        var pick = new BigListBox<Type>("Type of Control","Add",true,selectable,t=>t.Name,false);
        if(pick.ShowDialog())
        {
            var factory = new ViewFactory();
            var instance = factory.Create(pick.Selected);

            
            _viewBeingEdited.AddDesign(GetUniqueFieldName(pick.Selected), instance);
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
        StringBuilder sb = new StringBuilder();
        var view = GetMostFocused(this);
        foreach (var prop in view.GetType().GetProperties())
        {
            sb.AppendLine($"{prop.Name}:{prop.GetValue(view)}");
        }

        MessageBox.Query(10, 10, "Properties", sb.ToString(), "Ok");
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
