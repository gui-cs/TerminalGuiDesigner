using System.Data;
using NStack;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using TerminalGuiDesigner.Operations;
using static Terminal.Gui.Border;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

/// <summary>
/// Creates new <see cref="View"/> instances configured to have
/// sensible dimensions and content for dragging/configuring in
/// the designer.
/// </summary>
public class ViewFactory
{
    public ViewFactory()
    {
    }

    public View Create(Type t)
    {
        if (typeof(TableView).IsAssignableFrom(t))
        {
            return CreateTableView();
        }

        if (typeof(TabView).IsAssignableFrom(t))
        {
            return CreateTabView();
        }

        if (typeof(RadioGroup).IsAssignableFrom(t))
        {
            return CreateRadioGroup();
        }
        if (typeof(MenuBar).IsAssignableFrom(t))
        {
            return CreateMenuBar();
        }

        if (t == typeof(TextValidateField))
        {
            return new TextValidateField
            {

                Provider = new TextRegexProvider(".*"),
                Text = "Heya",
                Width = 5,
                Height = 1,
            };
        }

        if (t == typeof(ProgressBar))
        {
            return new ProgressBar
            {
                Width = 10,
                Height = 1,
                Fraction = 1f
            };
        }

        if (t == typeof(View))
        {
            return new View
            {
                Width = 10,
                Height = 5,
            };
        }

        if (typeof(GraphView).IsAssignableFrom(t))
        {
            return new GraphView
            {
                Width = 20,
                Height = 5,
                GraphColor = Attribute.Make(Color.White, Color.Black)
            };
        }

        if (typeof(ListView).IsAssignableFrom(t))
        {
            var lv = new ListView(new List<string> { "Item1", "Item2", "Item3" })
            {
                Width = 20,
                Height = 3,
            };

            return lv;
        }

        if (t == typeof(LineView))
        {
            return new LineView()
            {
                Width = 8,
                Height = 1
            };
        }
        if (t == typeof(TreeView))
        {
            return new TreeView()
            {
                Width = 16,
                Height = 5
            };
        }
        if (t == typeof(ScrollView))
        {
            return new ScrollView()
            {
                Width = 10,
                Height = 5,
                ContentSize = new Size(20, 10)
            };
        }
        var instance = (View?)Activator.CreateInstance(t) ?? throw new Exception($"CreateInstance returned null for Type '{t}'");
        instance.SetActualText("Heya");

        instance.Width = Math.Max(instance.Bounds.Width, 4);
        instance.Height = Math.Max(instance.Bounds.Height, 1);

        if (instance is FrameView || instance is HexView)
        {
            instance.Height = 5;
            instance.Width = 10;
        }


        instance.ColorScheme = Colors.Base;

        return instance;
    }

    private MenuBar CreateMenuBar()
    {
         return new MenuBar (new MenuBarItem [] {
				new MenuBarItem ("_File (F9)", new MenuItem [] {
					new MenuItem (AddMenuOperation.DefaultMenuItemText, "", () => {})
                })
         });
    }

    private View CreateRadioGroup()
    {
        var group = new RadioGroup
        {
            Width = 10,
            Height = 5,
        };
        group.RadioLabels = new ustring[] { "Option 1", "Option 2" };

        return group;
    }

    private TableView CreateTableView()
    {
        var dt = new DataTable();
        dt.Columns.Add("Column 0");
        dt.Columns.Add("Column 1");
        dt.Columns.Add("Column 2");
        dt.Columns.Add("Column 3");

        return new TableView
        {
            Width = 50,
            Height = 5,
            Table = dt
        };
    }

    private TabView CreateTabView()
    {
        var tabView = new TabView
        {
            Width = 50,
            Height = 5,
        };

        tabView.AddTab(new TabView.Tab("Tab1", new View { Width = Dim.Fill(), Height = Dim.Fill() }), false);
        tabView.AddTab(new TabView.Tab("Tab2", new View { Width = Dim.Fill(), Height = Dim.Fill() }), false);

        return tabView;
    }

    internal IEnumerable<Type> GetSupportedViews()
    {
        Type[] exclude = new Type[]{
            typeof(Toplevel),
             typeof(Window),
             typeof(ToplevelContainer),
             typeof(Dialog),
             typeof(FileDialog),
             typeof(SaveDialog),
             typeof(OpenDialog),
             typeof(ScrollBarView),
             typeof(TreeView<>)}; // The generic version of TreeView

        return typeof(View).Assembly.DefinedTypes.Where(t =>
                typeof(View).IsAssignableFrom(t) &&
                !t.IsInterface && !t.IsAbstract && t.IsPublic
            ).Except(exclude)
            .OrderBy(t => t.Name).ToArray();
    }
}
