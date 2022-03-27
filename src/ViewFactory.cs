using System.Data;
using NStack;
using Terminal.Gui;
using Terminal.Gui.Graphs;
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

        if(typeof(GraphView).IsAssignableFrom(t))
        {
            var g = new GraphView{
                Width = 20,
                Height = 5,
            };

            g.AxisX.Visible = true;

            g.Annotations.Add(new TextAnnotation
            {
                ScreenPosition = new Point(1, 1),
                Text = "Graph"
            });

            g.GraphColor = Attribute.Make(Color.White,Color.Black);
            return g;
        }

        var instance = (View?)Activator.CreateInstance(t) ?? throw new Exception($"CreateInstance returned null for Type '{t}'");

        instance.SetActualText("Heya");

        instance.Width = Math.Max(instance.Bounds.Width, 4);

        return instance;
    }

    private View CreateRadioGroup()
    {
        var group = new RadioGroup
        {
            Width = 10,
            Height = 5,
        };
        group.RadioLabels = new ustring[]{"Option 1","Option 2"};

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
        var tabView =  new TabView
        {
            Width = 50,
            Height = 5,
        };

        tabView.AddTab(new TabView.Tab("Tab1", new View{Width = Dim.Fill(),Height=Dim.Fill()}),false);
        tabView.AddTab(new TabView.Tab("Tab2", new View{Width = Dim.Fill(),Height=Dim.Fill()}),false);

        return tabView;
    }

    internal IEnumerable<Type> GetSupportedViews()
    {
        Type[] exclude = new Type[]{
            typeof(Toplevel),
             typeof(Window),
             typeof(ToplevelContainer)};

        return typeof(View).Assembly.DefinedTypes.Where(t => 
                typeof(View).IsAssignableFrom(t) && 
                !t.IsInterface && !t.IsAbstract && t.IsPublic
            ).Except(exclude)
            .OrderBy(t=>t.Name).ToArray();
    }
}
