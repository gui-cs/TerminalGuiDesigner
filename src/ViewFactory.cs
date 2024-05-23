using System.Data;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using TerminalGuiDesigner.Operations.MenuOperations;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

/// <summary>
///   Creates new <see cref="View" /> instances configured to have sensible dimensions
///   and content for dragging/configuring in the designer.
/// </summary>
public static class ViewFactory
{
    /// <summary>
    ///   A constant defining the default text for a new menu item added via the <see cref="ViewFactory" />
    /// </summary>
    /// <remarks>
    ///   <see cref="AddMenuOperation" /> adds a new top level menu (e.g. File, Edit etc.).<br />
    ///   In the designer, all menus must have at least 1 <see cref="MenuItem" /> under them, so it will be
    ///   created with a single <see cref="MenuItem" /> in it, already.<br />
    ///   That item will bear this text.<br /><br />
    ///   This string should be used by any other areas of code that want to create new <see cref="MenuItem" /> under
    ///   a top/sub menu (e.g. <see cref="ViewFactory" />).
    /// </remarks>
    /// <value>The string "Edit Me"</value>
    /// <seealso cref="DefaultMenuBarItems" />
    internal const string DefaultMenuItemText = "Edit Me";

    internal static readonly Type[] KnownUnsupportedTypes =
    [
        typeof( Toplevel ),
        typeof( Dialog ),
        typeof( FileDialog ),
        typeof( SaveDialog ),
        typeof( OpenDialog ),
        typeof( ScrollBarView ),

        // BUG These seem to cause stack overflows in CreateSubControlDesigns (see TestAddView_RoundTrip)
        typeof( Wizard ),
        typeof( WizardStep )
    ];

    /// <summary>
    ///   Gets a new instance of a default <see cref="MenuBarItem" />[], to include as the default initial
    ///   <see cref="MenuBar.Menus" />
    ///   collection of a new <see cref="MenuBar" />
    /// </summary>
    /// <value>
    ///   A new single-element array of <see cref="MenuBarItem" />, with default text, an empty
    ///   <see cref="MenuItem.Action" />, and empty <see cref="MenuItem.Help" /> string.
    /// </value>
    internal static MenuBarItem[] DefaultMenuBarItems
    {
        get
        {
            return
            [
                new( "_File (F9)",
                     [ new MenuItem( DefaultMenuItemText, string.Empty, static ( ) => { } ) ] )
            ];
        }
    }

    /// <summary>
    ///   Gets all <see cref="View" /> Types that are supported by <see cref="ViewFactory" />.
    /// </summary>
    /// <value>An <see cref="IEnumerable{T}" /> of <see cref="Type" />s supported by <see cref="ViewFactory" />.</value>
    public static IEnumerable<Type> SupportedViewTypes { get; } =
        typeof(View).Assembly.DefinedTypes
                    .Where(unfilteredType => unfilteredType is
                    {
                        IsInterface: false,
                        IsAbstract: false,
                        IsPublic: true,
                        IsValueType: false
                    })
                    .Where(filteredType => filteredType.IsSubclassOf(typeof(View)) && filteredType != typeof(Adornment)
                                           && !filteredType.IsSubclassOf(typeof(Adornment)))
                    .Where(viewDescendantType => !KnownUnsupportedTypes.Any(viewDescendantType.IsAssignableTo)
                                                  || viewDescendantType == typeof(Window))
                    // Slider is an alias of Slider<object> so don't offer that
                    .Where(vt => vt != typeof(Slider));

    private static bool IsSupportedType( this Type t )
    {
        return t == typeof( Window ) || ( !KnownUnsupportedTypes.Any( t.IsSubclassOf ) & !KnownUnsupportedTypes.Contains( t ) );
    }

    /// <summary>
    ///   Creates a new instance of a <see cref="View" /> of Type <typeparamref name="T" /> with
    ///   size/placeholder values that make it easy to see and design in the editor.
    /// </summary>
    /// <typeparam name="T">
    ///   A concrete descendant type of <see cref="View" /> that does not exist in the
    ///   <see cref="KnownUnsupportedTypes" /> collection and which has a public constructor.
    /// </typeparam>
    /// <param name="width">
    ///   An optional width of the requested view. Default values are dependent on the requested
    ///   type, if not supplied.
    /// </param>
    /// <param name="height">
    ///   An optional height of the requested view. Default values are dependent on the requested
    ///   type, if not supplied.
    /// </param>
    /// <param name="text">Initial text for the new view. Only used if it is relevant.</param>
    /// <exception cref="NotSupportedException">If an unsupported type is requested</exception>
    /// <returns>
    ///   A new instance of <typeparamref name="T" /> with the specified dimensions or defaults, if not provided.
    /// </returns>
    /// <remarks>
    ///   <typeparamref name="T" /> must inherit from <see cref="View" />, must have a public constructor, and must
    ///   not exist in the <see cref="KnownUnsupportedTypes" /> collection, at run-time.
    /// </remarks>
    public static T Create<T>(int? width = null, int? height = null, string? text = null )
        where T : View, new( )
    {
        if ( !IsSupportedType( typeof( T ) ) )
        {
            throw new NotSupportedException( $"Requested type {typeof( T ).Name} is not supported" );
        }

        T newView = new( );

        switch ( newView )
        {
            case Button:
            case CheckBox:
            case ColorPicker:
            case ComboBox:
            case TextView:
                newView.SetActualText( text ?? "Heya" );
                SetDefaultDimensions( newView, width ?? 4, height ?? 1 );
                break;
            case Line:
            case Slider:
            case TileView:
                SetDefaultDimensions( newView, width ?? 4, height ?? 1 );
                break;
            case TableView tv:
                var dt = new DataTable( );
                dt.Columns.Add( "Column 0" );
                dt.Columns.Add( "Column 1" );
                dt.Columns.Add( "Column 2" );
                dt.Columns.Add( "Column 3" );
                SetDefaultDimensions( newView, width ?? 50, height ?? 5 );
                tv.Table = new DataTableSource( dt );
                break;
            case TabView tv:
                tv.AddEmptyTab( "Tab1" );
                tv.AddEmptyTab( "Tab2" );
                SetDefaultDimensions( newView, width ?? 50, height ?? 5 );
                break;
            case TextValidateField tvf:
                tvf.Provider = new TextRegexProvider( ".*" );
                tvf.Text = text ?? "Heya";
                SetDefaultDimensions( newView, width ?? 5, height ?? 1 );
                break;
            case DateField df:
                df.Date = DateTime.Today;
                SetDefaultDimensions( newView, width ?? 20, height ?? 1 );
                break;
            case TextField tf:
                tf.Text = text ?? "Heya";
                SetDefaultDimensions( newView, width ?? 5, height ?? 1 );
                break;
            case ProgressBar pb:
                pb.Fraction = 1f;
                SetDefaultDimensions( newView, width ?? 10, height ?? 1 );
                break;
            case MenuBar mb:
                mb.Menus = DefaultMenuBarItems;
                break;
            case StatusBar sb:
                sb.Items = new[] { new StatusItem( Key.F1, "F1 - Edit Me", null ) };
                break;
            case RadioGroup rg:
                rg.RadioLabels = new string[] { "Option 1", "Option 2" };
                SetDefaultDimensions( newView, width ?? 10, height ?? 2 );
                break;
            case GraphView gv:
                gv.GraphColor = new Attribute( Color.White, Color.Black );
                SetDefaultDimensions( newView, width ?? 20, height ?? 5 );
                break;
            case ListView lv:
                lv.SetSource( new List<string> { "Item1", "Item2", "Item3" } );
                SetDefaultDimensions( newView, width ?? 20, height ?? 3 );
                break;
            case FrameView:
            case HexView:
                newView.SetActualText( text ?? "Heya" );
                SetDefaultDimensions( newView, width ?? 10, height ?? 5 );
                break;
            case Window:
                SetDefaultDimensions( newView, width ?? 10, height ?? 5 );
                break;
            case LineView:
                SetDefaultDimensions( newView, width ?? 8, height ?? 1 );
                break;
            case TreeView:
                SetDefaultDimensions( newView, width ?? 16, height ?? 5 );
                break;
            case TreeView<FileSystemInfo> fstv:
                fstv.TreeBuilder = new DelegateTreeBuilder<FileSystemInfo>((p) =>
                {
                    try
                    {
                        return p is DirectoryInfo d ? d.GetFileSystemInfos() : Enumerable.Empty<FileSystemInfo>();
                    }
                    catch (Exception)
                    {
                        return Enumerable.Empty<FileSystemInfo>();
                    }
                });

                SetDefaultDimensions(newView, width ?? 16, height ?? 5);
                break;
            case ScrollView sv:
                sv.SetContentSize(new Size( 20, 10 ));
                SetDefaultDimensions(newView, width ?? 10, height ?? 5 );
                break;
            case Label l:
                SetDefaultDimensions( newView, width ?? 4, height ?? 1 );
                l.SetActualText( text ?? "Heya" );
                break;
            case SpinnerView sv:
                sv.AutoSpin = true;
                if ( width is not null )
                {
                    sv.Width = width;
                }

                if ( height is not null )
                {
                    sv.Height = height;
                }

                break;
            case not null when newView.GetType( ).IsSubclassOf( typeof(View) ):
                // Case for view inheritors
                SetDefaultDimensions( newView, width ?? 5, height ?? 1 );
                break;
            case { }:
                newView.SetActualText( text ?? "Heya" );
                SetDefaultDimensions(newView, 10, 5);
                break;
            case null:
                throw new InvalidOperationException( $"Unexpected null result from type {typeof( T ).Name} construtor." );
        }

        return newView;

        static void SetDefaultDimensions( T v, int width = 5, int height = 1 )
        {
            v.Width = Math.Max( v.ContentSize.Width, width );
            v.Height = Math.Max( v.ContentSize.Height, height );
        }
    }

    /// <summary>
    ///   Creates a new instance of <see cref="View" /> of <see cref="Type" /> <paramref name="requestedType" /> with
    ///   size/placeholder values that make it easy to see and design in the editor.
    /// </summary>
    /// <param name="requestedType">
    ///   A <see cref="Type" /> of <see cref="View" />.<br />
    ///   See <see cref="SupportedViewTypes" /> for the full list of allowed Types.
    /// </param>
    /// <returns>A new instance of <see cref="Type" /> <paramref name="requestedType" />.</returns>
    /// <exception cref="Exception">Thrown if <see cref="Type" /> is not a subclass of <see cref="View" />.</exception>
    /// <remarks>Delegates to <see cref="Create{T}" />, for types supported by that method.</remarks>
    [Obsolete( "Migrate to using generic Create<T> method" )]
    public static View Create( Type requestedType )
    {
        if (requestedType.IsGenericType)
        {
            var method = typeof(ViewFactory).GetMethods().Single(m=>m.Name=="Create" && m.IsGenericMethodDefinition);
            method = method.MakeGenericMethod(requestedType) ?? throw new Exception("Could not find Create<T> method on ViewFactory");

            return (View)(method.Invoke(null, new object?[] { null, null, null }) ?? throw new Exception("ViewFactory.Create resulted in null"));
        }

        return requestedType switch
        {
            null => throw new ArgumentNullException( nameof( requestedType ) ),
            { } t when t == typeof( DateField ) => Create<DateField>( ),
            { } t when t == typeof( Button ) => Create<Button>( ),
            { } t when t == typeof( ComboBox ) => Create<ComboBox>( ),
            { } t when t == typeof( Line ) => Create<Line>( ),
            { } t when t == typeof( Slider ) => Create<Slider>( ),
            { } t when t == typeof( TileView ) => Create<TileView>( ),
            { } t when t.IsAssignableTo( typeof( CheckBox ) ) => Create<CheckBox>( ),
            { } t when t.IsAssignableTo( typeof( TableView ) ) => Create<TableView>( ),
            { } t when t.IsAssignableTo( typeof( TabView ) ) => Create<TabView>( ),
            { } t when t.IsAssignableTo( typeof( RadioGroup ) ) => Create<RadioGroup>( ),
            { } t when t.IsAssignableTo( typeof( MenuBar ) ) => Create<MenuBar>( ),
            { } t when t.IsAssignableTo( typeof( StatusBar ) ) => Create<StatusBar>( ),
            { } t when t == typeof( TextValidateField ) => Create<TextValidateField>( ),
            { } t when t == typeof( ProgressBar ) => Create<ProgressBar>( ),
            { } t when t == typeof( View ) => Create<View>( ),
            { } t when t == typeof( Window ) => Create<Window>( ),
            { } t when t == typeof( TextField ) => Create<TextField>( ),
            { } t when t.IsAssignableTo( typeof( GraphView ) ) => Create<GraphView>( ),
            { } t when t.IsAssignableTo( typeof( ListView ) ) => Create<ListView>( ),
            { } t when t == typeof( LineView ) => Create<LineView>( ),
            { } t when t == typeof( TreeView ) => Create<TreeView>( ),
            { } t when t == typeof( ScrollView ) => Create<ScrollView>( ),
            { } t when t.IsAssignableTo( typeof( SpinnerView ) ) => Create<SpinnerView>( ),
            { } t when t.IsAssignableTo( typeof( FrameView ) ) => Create<FrameView>( ),
            { } t when t.IsAssignableTo( typeof( HexView ) ) => Create<HexView>( ),
            { } t when t.IsAssignableTo( typeof( Tab ) ) => Create<Tab>( ),
            { } t when t.IsAssignableTo( typeof( LegendAnnotation ) ) => Create<LegendAnnotation>( ),
            { } t when t.IsAssignableTo( typeof( DatePicker ) ) => Create<DatePicker>( ),
            _ => ReflectionHelpers.GetDefaultViewInstance( requestedType )
        };
    }

    public static T CreateAnother<T>( T oneOfThese )
        where T : View, new( )
    {
        return Create<T>( );
    }

}
