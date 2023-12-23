using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ViewFactory ) )]
[Category( "Core" )]
[Order( 1 )]
internal class ViewFactoryTests : Tests
{
    /// <summary>
    ///   Gets every known supported <see cref="View" /> type as an uninitialized object of the corresponding type for testing of generic methods.
    /// </summary>
    private static IEnumerable<TestCaseData> Create_And_CreateT_Type_Provider
    {
        get { return ViewFactory_SupportedViewTypes.Select( static t => new TestCaseData( FormatterServices.GetUninitializedObject( t ) ) ); }
    }

    private static MenuBarItem[] ViewFactory_DefaultMenuBarItems => ViewFactory.DefaultMenuBarItems;

    private static Type[] ViewFactory_KnownUnsupportedTypes => ViewFactory.KnownUnsupportedTypes;

    private static IEnumerable<Type> ViewFactory_SupportedViewTypes => ViewFactory.SupportedViewTypes;

    [Test]
    [TestCaseSource( nameof( Create_And_CreateT_Type_Provider ) )]
    [Category( "Change Control" )]
    [Description( "This test makes sure that both the generic and non-generic Create methods return objects with the same property values" )]
#pragma warning disable CS0618 // Type or member is obsolete
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
                switch ( dummyInvalidOject: dummyInvalidObject, property )
                {
                    case (_, not null) when property.PropertyType == typeof( Frame ):
                        Assert.That( nonGenericPropertyValue!.ToString( ), Is.EqualTo( genericPropertyValue!.ToString( ) ) );
                        continue;
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
                    case (ComboBox, { Name: "Subviews" }):
                    case (ScrollView, { Name: "Subviews" }):
                    case (MenuBar, { Name: "Menus" }):
                    case (TableView, { Name: "Table" }):
                    case (TabView, { Name: "Tabs" }):
                    case (TabView, { Name: "SelectedTab" }):
                    case (TabView, { Name: "Subviews" }):
                    case (TileView, { Name: "Subviews" }):
                    case (TileView, { Name: "Tiles" }):
                        // Warn about these, until they are implemented (WIP)
                        Assert.Warn( $"{property.Name} not yet checked on {typeof( T ).Name}" );
                        continue;
                    case (_, { Name: "ContextMenu" }):
                        // Safe to skip these, as we don't set them in Create
                        continue;
                }

                Assert.That(
                    nonGenericPropertyValue,
                    Is.EqualTo( genericPropertyValue ),
                    $"Property {property!.Name} of type {property.ReflectedType!.Name} mismatch. Generic: {genericPropertyValue} Non-Generic: {nonGenericPropertyValue}" );
            }
        } );
    }
#pragma warning restore CS0618 // Type or member is obsolete

    [Test]
    [TestCaseSource( nameof( Create_And_CreateT_Type_Provider ) )]
    [Category( "Change Control" )]
    [Description( "This test makes sure that both the generic and non-generic Create methods return non-null instances of the same type" )]
#pragma warning disable CS0618  // Type or member is obsolete
#pragma warning disable IDE0060 // Remove unused parameter
    public void Create_And_CreateT_ReturnExpectedTypeForSameInputs<T>( T dummyTypeForNUnit )
        where T : View, new( )
    {
        T? createdByNonGeneric = ViewFactory.Create( typeof( T ) ) as T;
        Assert.That( createdByNonGeneric, Is.Not.Null.And.InstanceOf<T>( ) );

        T createdByGeneric = ViewFactory.Create<T>( );
        Assert.That( createdByGeneric, Is.Not.Null.And.InstanceOf<T>( ) );
    }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CS0618  // Type or member is obsolete

    [Test]
    [RequiresThread]
    [NonParallelizable]
    public void CreateT_DoesNotThrowOnSupportedTypes( [ValueSource( nameof( ViewFactory_SupportedViewTypes ) )] Type supportedType )
    {
        // NUnit does not natively support generic type parameters in test methods, so this is easiest to do via reflection
        MethodInfo viewFactoryCreateTGeneric = typeof( ViewFactory ).GetMethods( ).Single( static m => m is { IsGenericMethod: true, IsPublic: true, IsStatic: true } );

        MethodInfo viewFactoryCreateTConcrete = viewFactoryCreateTGeneric.MakeGenericMethod( supportedType );

        object? createdView = null;

        Assert.That( ( ) =>
                         createdView = viewFactoryCreateTConcrete
                             .Invoke( null, Enumerable.Repeat<object?>( null, viewFactoryCreateTConcrete.GetParameters( ).Length ).ToArray( ) ),
                     Throws.Nothing );

        if ( createdView is IDisposable d )
        {
            d.Dispose( );
        }
    }

    [Test]
    [RequiresThread]
    [NonParallelizable]
    public void CreateT_ReturnsValidViewOfExpectedType( [ValueSource( nameof( ViewFactory_SupportedViewTypes ) )] Type supportedType )
    {
        // NUnit does not natively support generic type parameters in test methods, so this is easiest to do via reflection
        MethodInfo viewFactoryCreateTGeneric = typeof( ViewFactory ).GetMethods( ).Single( static m => m is { IsGenericMethod: true, IsPublic: true, IsStatic: true } );

        MethodInfo viewFactoryCreateTConcrete = viewFactoryCreateTGeneric.MakeGenericMethod( supportedType );

        object? createdView = viewFactoryCreateTConcrete.Invoke( null, Enumerable.Repeat<object?>( null, viewFactoryCreateTConcrete.GetParameters( ).Length ).ToArray( ) );

        Assert.That( createdView, Is.Not.Null.And.InstanceOf( supportedType ) );

        if ( createdView is IDisposable d )
        {
            d.Dispose( );
        }
    }

    [Test]
    [RequiresThread]
    [NonParallelizable]
    public void CreateT_ThrowsOnUnsupportedTypes( [ValueSource( nameof( CreateT_ThrowsOnUnsupportedTypes_Cases ) )] Type unsupportedType )
    {
        // NUnit does not natively support generic type parameters in test methods, so this is easiest to do via reflection
        MethodInfo viewFactoryCreateTGeneric = typeof( ViewFactory ).GetMethods( ).Single( static m => m is { IsGenericMethod: true, IsPublic: true, IsStatic: true } );

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
    [RequiresThread]
    [NonParallelizable]
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
    public void KnownUnsupportedTypes_ContainsExpectedItems( [ValueSource( nameof( KnownUnsupportedTypes_ExpectedTypes ) )] Type expectedType )
    {
        Assert.That( ViewFactory.KnownUnsupportedTypes, Contains.Item( expectedType ) );
    }

    [Test]
    [Description( "Checks that no new types have been added that aren't tested" )]
    [Category( "Change Control" )]
    [RequiresThread]
    [NonParallelizable]
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
            typeof( TreeView<> ),
            typeof( Slider<> ),
            typeof( Frame ),
            typeof( Wizard ),
            typeof( WizardStep )
        };
    }
}
