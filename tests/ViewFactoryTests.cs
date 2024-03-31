using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ViewFactory ) )]
[Category( "Core" )]
[Order( 1 )]
[Parallelizable(ParallelScope.Children)]
internal class ViewFactoryTests
{
    [ThreadStatic]
    private static bool? _init;

    [OneTimeSetUp]
    public virtual void SetUp()
    {
        _init ??= false;
        if (_init.Value)
        {
            throw new InvalidOperationException("After did not run.");
        }

        Application.Init(new FakeDriver());
        _init = true;
    }

    [OneTimeTearDown]
    public virtual void TearDown()
    {
        Application.Shutdown();
        _init = false;
    }

    /// <summary>
    ///   Gets every known supported <see cref="View" /> type as an uninitialized object of the corresponding type for testing of generic methods.
    /// </summary>
    private static IEnumerable<TestCaseData> Create_And_CreateT_Type_Provider
    {
        get { return ViewFactory_SupportedViewTypes
                .Select(Tests.PickFirstTTypeForGenerics)
                .Select(
            static t => new TestCaseData( 
                RuntimeHelpers.GetUninitializedObject( t ) 
                ) ); }
    }

    private static MenuBarItem[] ViewFactory_DefaultMenuBarItems => ViewFactory.DefaultMenuBarItems;

    private static Type[] ViewFactory_KnownUnsupportedTypes => ViewFactory.KnownUnsupportedTypes;

    private static IEnumerable<Type> ViewFactory_SupportedViewTypes => ViewFactory.SupportedViewTypes;

    [Test]
    [TestCaseSource( nameof( Create_And_CreateT_Type_Provider ) )]
    [Parallelizable(ParallelScope.Children)]
    [Category( "Change Control" )]
    [Description( "This test makes sure that both the generic and non-generic Create methods return objects with the same property values" )]
    [Obsolete("Can be removed once non-generic ViewFactory.Create method is no longer in use.")]
    public void Create_And_CreateT_ReturnEquivalentInstancesForSameInputs<T>( T dummyInvalidObject )
        where T : View, new( )
    {
        T? createdByNonGeneric = ViewFactory.Create( typeof( T ) ) as T;
        Assume.That( createdByNonGeneric, Is.Not.Null.And.InstanceOf<T>( ) );

        T createdByGeneric = ViewFactory.Create<T>( );
        Assume.That( createdByGeneric, Is.Not.Null.And.InstanceOf<T>( ) );

        PropertyInfo[] publicPropertiesOfType = typeof( T ).GetProperties( BindingFlags.Instance | BindingFlags.Public ).Where( static p => p.CanRead ).ToArray( );
        Assert.Multiple( ( ) =>
        {
            foreach ( PropertyInfo property in publicPropertiesOfType )
            {
                switch ( dummyInvalidObject, property )
                {
                    case (ComboBox, { Name: "Subviews" }):
                    case (MenuBar, { Name: "Menus" }):
                    case (TableView, { Name: "Table" }):
                    case (TabView, { Name: "Tabs" }):
                    case (TabView, { Name: "SelectedTab" }):
                    case (TabView, { Name: "Subviews" }):
                        // Warn about these, until they are implemented (WIP)
                        Assert.Warn( $"{property.Name} not yet checked on {typeof( T ).Name}" );
                        continue;
                    case (ScrollView, { Name: "Subviews" }):
                    case (TileView, { Name: "Subviews" }):
                    case (TileView, { Name: "Tiles" }):
                    case (_, { Name: "ContextMenu" }):
                    case (_, { Name: "OverlappedTop" }):
                        continue;
                    case (Window, _):
                        // Safe to skip these, as we don't set them in Create
                        continue;
                }

                object? nonGenericPropertyValue = property.GetValue( createdByNonGeneric );
                object? genericPropertyValue = property.GetValue( createdByGeneric );

                // First, if they're both null, we're good so just skip it.
                if ( nonGenericPropertyValue is null && genericPropertyValue is null )
                {
                    continue;
                }

                // If one or the other isn't null, assert they both are not null
                Assert.Multiple( ( ) =>
                {
                    Assert.That( nonGenericPropertyValue, Is.Not.Null );
                    Assert.That( genericPropertyValue, Is.Not.Null );
                } );

                // Special cases for certain properties by property type, property name, and/or tested view type.
                // In general, we could actually skip basically everything that isn't explicitly in Create,
                // but doing it this way allows us to just test everything and only skip what we absolutely have to.
                switch ( dummyInvalidObject, property )
                {
                    case (_, not null) when property.PropertyType.IsAssignableTo( typeof( Dim ) ):
                        Assert.That( (Dim)nonGenericPropertyValue!, Is.EqualTo( (Dim)genericPropertyValue! ) );
                        continue;
                    case (_, not null) when property.PropertyType.IsAssignableTo( typeof( TextFormatter ) ):
                        TextFormatter nonGenericTextFormatter = (TextFormatter)nonGenericPropertyValue!;
                        TextFormatter genericTextFormatter = (TextFormatter)genericPropertyValue!;
                        Assert.That( nonGenericTextFormatter.ToCodePrimitiveExpression( ), Is.EqualTo( genericTextFormatter.ToCodePrimitiveExpression( ) ) );
                        continue;
                    case (_, { Name: "TabIndexes" }):
                        var nonGenericTabIndexes = (ReadOnlyCollection<View>)nonGenericPropertyValue!;
                        var genericTabIndexes = (ReadOnlyCollection<View>)genericPropertyValue!;
                        Assert.That(
                            nonGenericTabIndexes,
                            Is.EquivalentTo( genericTabIndexes )
                              .Using<View, View>( CompareTwoViews ) );
                        continue;
                }

                Assert.That(
                    nonGenericPropertyValue,
                    Is.EqualTo( genericPropertyValue ),
                    $"Property {property!.Name} of type {property.ReflectedType!.Name} mismatch. Generic: {genericPropertyValue} Non-Generic: {nonGenericPropertyValue}" );
            }
        } );
    }

    [Test]
    [TestCaseSource( nameof( Create_And_CreateT_Type_Provider ) )]
    [Parallelizable(ParallelScope.Children)]
    [Category( "Change Control" )]
    [Description( "This test makes sure that both the generic and non-generic Create methods return non-null instances of the same type" )]
    [Obsolete("Can be removed once non-generic ViewFactory.Create method is no longer in use.")]
    [SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "It is expected" )]
    public void Create_And_CreateT_ReturnExpectedTypeForSameInputs<T>( T dummyTypeForNUnit )
        where T : View, new( )
    {
        T? createdByNonGeneric = ViewFactory.Create( typeof( T ) ) as T;
        Assert.That( createdByNonGeneric, Is.Not.Null.And.InstanceOf<T>( ) );

        T createdByGeneric = ViewFactory.Create<T>( );
        Assert.That( createdByGeneric, Is.Not.Null.And.InstanceOf<T>( ) );
    }

    [Test]
    [Parallelizable(ParallelScope.Children)]
    [TestCaseSource( nameof( Create_And_CreateT_Type_Provider ) )]
    [SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "It is expected" )]
    public void CreateT_DoesNotThrowOnSupportedTypes<T>(T dummyObject  )
        where T : View, new( )
    {
        T createdView = ViewFactory.Create<T>( );

        Assert.That( ( ) => createdView = ViewFactory.Create<T>( ), Throws.Nothing );

        if ( createdView is IDisposable d )
        {
            d.Dispose( );
        }
    }

    [Test]
    [Parallelizable(ParallelScope.Children)]
    [TestCaseSource( nameof( Create_And_CreateT_Type_Provider ) )]
    [SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "It is expected" )]
    public void CreateT_ReturnsValidViewOfExpectedType<T>( T dummyObject )
        where T : View, new( )
    {
        T createdView = ViewFactory.Create<T>( );

        Assert.That( createdView, Is.Not.Null.And.InstanceOf<T>( ) );

        if ( createdView is IDisposable d )
        {
            d.Dispose( );
        }
    }

    [Test]
    [Parallelizable(ParallelScope.Children)]
    public void CreateT_ThrowsOnUnsupportedTypes( [ValueSource( nameof( CreateT_ThrowsOnUnsupportedTypes_Cases ) )] Type unsupportedType )
    {
        MethodInfo viewFactoryCreateTGeneric = typeof( ViewFactory ).GetMethods( ).Single( static m => m is { IsGenericMethod: true, IsPublic: true, IsStatic: true, Name: "Create" } );

        MethodInfo viewFactoryCreateTConcrete = viewFactoryCreateTGeneric.MakeGenericMethod( unsupportedType );

        object? createdView = null;
        object?[] methodParameters = Enumerable.Repeat<object?>( null, viewFactoryCreateTConcrete.GetParameters( ).Length ).ToArray( );

        Assert.That( ( ) => createdView = viewFactoryCreateTConcrete.Invoke( null, methodParameters ),
                     Throws.TypeOf<TargetInvocationException>( ).With.InnerException.TypeOf<NotSupportedException>( ) );

        if ( createdView is IDisposable d )
        {
            d.Dispose( );
        }
    }

    [Test]
    [Category( "Change Control" )]
    public void DefaultMenuBarItems_IsExactlyAsExpected( )
    {
        Assert.Multiple( static ( ) =>
        {
            Assert.That( ViewFactory_DefaultMenuBarItems, Has.Length.EqualTo( 1 ) );
            Assert.That( ViewFactory_DefaultMenuBarItems[ 0 ].Title, Is.EqualTo( "_File (F9)" ) );
        } );

        Assert.Multiple( static ( ) =>
        {
            Assert.That( ViewFactory_DefaultMenuBarItems[ 0 ].Children, Has.Length.EqualTo( 1 ) );
            Assert.That( ViewFactory_DefaultMenuBarItems[ 0 ].Children[ 0 ].Title, Is.EqualTo( ViewFactory.DefaultMenuItemText ) );
            Assert.That( ViewFactory_DefaultMenuBarItems[ 0 ].Children[ 0 ].Help, Is.Empty );
        } );
    }

    [Test]
    [Description( "Checks that all tested types exist in the collection in ViewFactory" )]
    [Category( "Change Control" )]
    [Parallelizable(ParallelScope.Children)]
    public void KnownUnsupportedTypes_ContainsExpectedItems( [ValueSource( nameof( KnownUnsupportedTypes_ExpectedTypes ) )] Type expectedType )
    {
        Assert.That( ViewFactory.KnownUnsupportedTypes, Contains.Item( expectedType ) );
    }

    [Test]
    [Description( "Checks that no new types have been added that aren't tested" )]
    [Category( "Change Control" )]
    [Parallelizable(ParallelScope.Children)]
    public void KnownUnsupportedTypes_DoesNotContainUnexpectedItems( [ValueSource( nameof( ViewFactory_KnownUnsupportedTypes ) )] Type typeDeclaredUnsupportedInViewFactory )
    {
        Assert.That( KnownUnsupportedTypes_ExpectedTypes, Contains.Item( typeDeclaredUnsupportedInViewFactory ) );
    }

    private bool CompareTwoViews( View nonGenericTabIndex, View genericTabIndex )
    {
        Assert.Warn( "View comparison only done by bounds check." );
        return nonGenericTabIndex.Bounds == genericTabIndex.Bounds;
    }

    private static IEnumerable<Type> CreateT_ThrowsOnUnsupportedTypes_Cases( )
    {
        // Filtering generics out because they'll still throw, but a different exception
        return ViewFactory_KnownUnsupportedTypes.Where( static t => !t.IsGenericType );
    }

    private static Type[] KnownUnsupportedTypes_ExpectedTypes( )
    {
        return new[]
        {
            typeof( Toplevel ),
            typeof( Dialog ),
            typeof( FileDialog ),
            typeof( SaveDialog ),
            typeof( OpenDialog ),
            typeof( ScrollBarView ),
            typeof( Wizard ),
            typeof( WizardStep )
        };
    }
}
