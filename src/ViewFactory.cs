using System.Data;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using TerminalGuiDesigner.Operations.MenuOperations;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

/// <summary>
/// Creates new <see cref="View"/> instances configured to have
/// sensible dimensions and content for dragging/configuring in
/// the designer.
/// </summary>
public static class ViewFactory
{
    private static readonly Type[] KnownUnsupportedTypes = new[] {
        typeof( Toplevel ),
        typeof( Dialog ),
        typeof( FileDialog ),
        typeof( SaveDialog ),
        typeof( OpenDialog ),
        typeof( ScrollBarView ),
        typeof( TreeView<> ),

        typeof( Slider<> ),

        // Theses are special types of view and shouldn't be added manually by user
        typeof( Frame ),

        // BUG These seem to cause stack overflows in CreateSubControlDesigns (see TestAddView_RoundTrip)
        typeof( Wizard ),
        typeof( WizardStep ),
    };

    private static readonly Type ViewType = typeof(View);

    /// <summary>
    /// Returns all <see cref="View"/> Types that are supported by <see cref="ViewFactory"/>.
    /// </summary>
    /// <returns>All supported types.</returns>
    public static IEnumerable<Type> GetSupportedViews()
    {
        Type[] exclude = new Type[]
        {
            typeof(Toplevel),
            typeof(Dialog),
            typeof(FileDialog),
            typeof(SaveDialog),
            typeof(OpenDialog),
            typeof(ScrollBarView),
            typeof(TreeView<>),

            typeof(Slider<>),

            // Theses are special types of view and shouldn't be added manually by user
            typeof(Frame),

            // These seem to cause stack overflows in CreateSubControlDesigns (see TestAddView_RoundTrip)
            typeof(Wizard),
            typeof(WizardStep),
        }; // The generic version of TreeView

        return ViewType.Assembly.DefinedTypes
                       .Where(IsSupportedType)
                       .OrderBy(t => t.Name).ToArray();

        static bool IsSupportedType( Type candidateType )
        {
            return candidateType is
                   {
                       IsInterface: false, IsAbstract: false, IsEnum: false, IsValueType: false, IsClass: true
                   }
                   && candidateType.IsAssignableTo( ViewType )
                   & !KnownUnsupportedTypes.Any( candidateType.IsAssignableTo );
        }
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="View"/> of Type <typeparamref name="T"/> with
    /// size/placeholder values that make it easy to see and design in the editor.
    /// </summary>
    /// <typeparam name="T">A descendant of <see cref="View"/> that does not exist in the
    /// <see cref="KnownUnsupportedTypes"/> collection.</typeparam>
    /// <returns>A new instance of <paramref name="{T}"/>.</returns>
    public static T Create<T>( )
        where T : View, new( )
    {
        T newView = new()
        {
            Width = 10,
            Height = 1
        };

        switch ( newView )
        {
            case TextValidateField tvf:
                tvf.Provider = new TextRegexProvider( ".*" );
                tvf.Text = "Heya";
                break;
            case TextField tf:
                tf.Text = "Heya";
                break;
            case ProgressBar pb:
                pb.Fraction = 1f;
                break;
            case Window:
            default:
                newView.Height = 5;
                break;
        }

        return newView;
    }

    /// <summary>
    /// Creates a new instance of <see cref="View"/> of Type <paramref name="t"/> with
    /// size/placeholder values that make it easy to see and design in the editor.
    /// </summary>
    /// <param name="t">A Type of <see cref="View"/>.  See <see cref="GetSupportedViews"/> for the
    /// full list of allowed Types.</param>
    /// <returns>A new instance of Type <paramref name="t"/>.</returns>
    /// <exception cref="Exception">Thrown if Type is not a subclass of <see cref="View"/>.</exception>
    public static View Create(Type t)
    {
        if (typeof(TableView).IsAssignableFrom(t))
        {
            return CreateTableView( );
        }

        if (typeof(TabView).IsAssignableFrom(t))
        {
            return CreateTabView( );
        }

        if (typeof(RadioGroup).IsAssignableFrom(t))
        {
            return CreateRadioGroup( );
        }

        if (typeof(MenuBar).IsAssignableFrom(t))
        {
            return CreateMenuBar( );
        }

        if (typeof(StatusBar).IsAssignableFrom(t))
        {
            return new StatusBar(new[] { new StatusItem(Key.F1, "F1 - Edit Me", null) });
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
                Fraction = 1f,
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

        if (t == typeof(Window))
        {
            return new Window
            {
                Width = 10,
                Height = 5,
            };
        }

        if (t == typeof(TextField))
        {
            return new TextField
            {
                Width = 10,
                Height = 1,
            };
        }

        if (typeof(GraphView).IsAssignableFrom(t))
        {
            return new GraphView
            {
                Width = 20,
                Height = 5,
                GraphColor = new Attribute(Color.White, Color.Black),
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
                Height = 1,
            };
        }

        if (t == typeof(TreeView))
        {
            return new TreeView()
            {
                Width = 16,
                Height = 5,
            };
        }

        if (t == typeof(ScrollView))
        {
            return new ScrollView()
            {
                Width = 10,
                Height = 5,
                ContentSize = new Size(20, 10),
            };
        }

        if (typeof(SpinnerView).IsAssignableFrom(t))
        {
            return new SpinnerView() { AutoSpin = true };
        }

        var instance = Activator.CreateInstance(t) as View ?? throw new Exception($"CreateInstance returned null for Type '{t}'");
        instance.SetActualText("Heya");

        instance.Width = Math.Max(instance.Bounds.Width, 4);
        instance.Height = Math.Max(instance.Bounds.Height, 1);

        if (instance is FrameView || instance is HexView)
        {
            instance.Height = 5;
            instance.Width = 10;
        }

        return instance;
    }

    private static MenuBar CreateMenuBar()
    {
        return new MenuBar(new MenuBarItem[]
        {
                new MenuBarItem(
                    "_File (F9)",
                    new MenuItem[] { new MenuItem(AddMenuOperation.DefaultMenuItemText, string.Empty, () => { }) }),
        });
    }

    private static View CreateRadioGroup()
    {
        var group = new RadioGroup
        {
            Width = 10,
            Height = 2,
        };
        group.RadioLabels = new string[] { "Option 1", "Option 2" };

        return group;
    }

    private static TableView CreateTableView()
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
            Table = new DataTableSource(dt),
        };
    }

    private static TabView CreateTabView()
    {
        var tabView = new TabView
        {
            Width = 50,
            Height = 5,
        };

        tabView.AddEmptyTab("Tab1");
        tabView.AddEmptyTab("Tab2");

        return tabView;
    }
}
