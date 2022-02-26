using System.Reflection;
using Terminal.Gui;
using System.Text;

namespace TerminalGuiDesigner;

internal class Editor : Toplevel
{
    Design<View>? viewBeingEdited;

    const string Help = @"F1 - Show this help
F2 - Add Control
F4 - Edit Selected Control Properties";

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

            var viewToCode = new ViewToCode();
            viewBeingEdited = viewToCode.GenerateNewWindow(classFile, "TerminalGuiDesigner");

            var decompiler = new CodeToView(designerFile);
            viewBeingEdited = decompiler.CreateInstance();

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
                dragging = HitTest(viewBeingEdited.View, m);
            }

            // continue dragging
            if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging != null)
            {
                var dest = ScreenToClient(viewBeingEdited.View, m.X, m.Y);
                dragging.X = dest.X;
                dragging.Y = dest.Y;

                viewBeingEdited.View.SetNeedsDisplay();
                Application.DoEvents();
            }

            // end dragging
            if (!m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging != null)
            {
                dragging = null;
            }
        };

        this.Add(viewBeingEdited.View);

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
        }

        return base.ProcessHotKey(keyEvent);
    }

    private void ShowAddViewWindow()
    {
        if(viewBeingEdited == null)
        {
            return;
        }
        
        var lbl = new Label("yay!");
        viewBeingEdited.Add("label1", lbl);
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
