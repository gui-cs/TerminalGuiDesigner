using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using Terminal.Gui;

namespace designer;


public partial class Program
{
    private static List<View> views = new List<View>();
    private static Label info;

    public static void Main(string[] args)
    {
        ShowEditor();

        GenerateDesignerCs();
    }

    private static void ShowEditor()
    {
        Application.Init();

        View w;

        try
        {
            var programDir = Assembly.GetEntryAssembly()?.Location ?? throw new Exception("Could not determine the current executables present directory");
            var file = new FileInfo(Path.Combine(programDir,"../../../../MyWindow.Designer.cs"));

            var decompiler = new DeCompiler(file);
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
        return new Point(x - view.Bounds.X,y - view.Bounds.Y);
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

    private static void GenerateDesignerCs()
    {
        
        var samples = new CodeNamespace("Samples");
        samples.Imports.Add(new CodeNamespaceImport("System"));
        samples.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

        CodeCompileUnit compileUnit = new CodeCompileUnit();
        compileUnit.Namespaces.Add(samples);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration("Class1");
        class1.IsPartial = true;

        samples.Types.Add(class1);

        CSharpCodeProvider provider = new CSharpCodeProvider();

        using (var sw = new StringWriter())
        {
            IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");

            // Generate source code using the code provider.
            provider.GenerateCodeFromCompileUnit(compileUnit, tw,
                new CodeGeneratorOptions());

            tw.Close();

            Console.Write(sw.ToString());
        }
    }

}
