using System.Reflection;
using System.Text;
using Terminal.Gui;

namespace TerminalGuiDesigner;


public partial class Program
{
    private static List<View> views = new List<View>();
    private static Label info;

    public static void Main(string[] args)
    {
        ShowEditor();
    }

    private static void ShowEditor()
    {
        Application.Init();

        View w;

        try
        {
            var programDir = Assembly.GetEntryAssembly()?.Location ?? throw new Exception("Could not determine the current executables present directory");

            var classFile = new FileInfo(Path.Combine(programDir, "../../../../MyWindow.cs"));
            var designerFile = new FileInfo(Path.Combine(programDir,"../../../../MyWindow.Designer.cs"));

            var viewToCode = new ViewToCode();
            w = viewToCode.GenerateNewWindow(classFile, "TerminalGuiDesigner");

            var decompiler = new CodeToView(designerFile);
            w = decompiler.CreateInstance();

        }catch(Exception ex)
        {
            MessageBox.ErrorQuery("Error Loading Designer",ex.Message,"Ok");
            Application.Shutdown();
            return;
        }

        View? dragging = null;
        
        Add(w,new Label("Drag Me 1"){Y=0});
        Add(w,new Label("Drag Me 2"){Y=2});
        Add(w,new Label("Drag Me 3"){Y=4});

        info = new Label("Info"){Y = Pos.AnchorEnd(1)};
        w.Add(info);

        Application.RootMouseEvent += (m)=>{
            
            // start dragging
            if(m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging == null)
            {            
                dragging = HitTest(w,m);
            }
            
            // continue dragging
            if(m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging != null)
            {
                var dest = ScreenToClient(w,m.X,m.Y);
                dragging.X = dest.X;
                dragging.Y = dest.Y;

                w.SetNeedsDisplay();
                Application.DoEvents();
            }

            // end dragging
            if(!m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragging != null)
            {
                dragging = null;
            }

            UpdateInfoLabel(w,m);
        };

        Application.Top.Add(w);

        Application.Run();
        Application.Shutdown();

    }

    private static View? HitTest(View w,MouseEvent m)
    {
        var point = ScreenToClient(w,m.X,m.Y);
        return views.FirstOrDefault(v=>v.Frame.Contains(point));
    }

    private static void UpdateInfoLabel(View w, MouseEvent m)
    {
        var point = ScreenToClient(w,m.X,m.Y);
        var focused = GetMostFocused(w);
        info.Text = $"({point.X},{point.Y}) - {focused?.GetType().Name} ({focused?.Frame})";
    }

    private static View GetMostFocused(View view)
    {
        if(view.Focused == null)
        {
            return view;
        }

        return GetMostFocused(view.Focused);
    }

    private static Point ScreenToClient(View view,int x, int y)
    {
        if(view is Window w)
        {
            // has invisible ContentView pane
            return w.Subviews[0].ScreenToView(x, y);
        }

        return view.ScreenToView(x,y);
    }

    private static void Add(View w, View view)
    {
        view.CanFocus = true;
        w.Add(view);
        views.Add(view);

        view.KeyPress += (k)=>
        {
            
            if(k.KeyEvent.Key == Key.F4)
            {
                StringBuilder sb = new StringBuilder();
                
                foreach(var prop in view.GetType().GetProperties())
                {
                    sb.AppendLine($"{prop.Name}:{prop.GetValue(view)}");
                }

                MessageBox.Query(10,10,"Properties",sb.ToString(),"Ok");
                k.Handled = true;
            }
        };
    }

    

}
