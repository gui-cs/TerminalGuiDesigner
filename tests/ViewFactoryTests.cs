using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terminal.Gui.TextValidateProviders;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ViewFactory ) )]
[Category( "Core" )]
[Order( 1 )]
[NonParallelizable]
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
    [NonParallelizable]
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
    [NonParallelizable]
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
    [NonParallelizable]
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
    [NonParallelizable]
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
    [NonParallelizable]
    public void KnownUnsupportedTypes_ContainsExpectedItems( [ValueSource( nameof( KnownUnsupportedTypes_ExpectedTypes ) )] Type expectedType )
    {
        Assert.That( ViewFactory.KnownUnsupportedTypes, Contains.Item( expectedType ) );
    }

    [Test]
    [Description( "Checks that no new types have been added that aren't tested" )]
    [Category( "Change Control" )]
    [NonParallelizable]
    public void KnownUnsupportedTypes_DoesNotContainUnexpectedItems( [ValueSource( nameof( ViewFactory_KnownUnsupportedTypes ) )] Type typeDeclaredUnsupportedInViewFactory )
    {
        Assert.That( KnownUnsupportedTypes_ExpectedTypes, Contains.Item( typeDeclaredUnsupportedInViewFactory ) );
    }

    private bool CompareTwoViews( View nonGenericTabIndex, View genericTabIndex )
    {
        Assert.Warn( "View comparison only done by bounds check." );
        return nonGenericTabIndex.GetContentSize() == genericTabIndex.GetContentSize();
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
            typeof( WizardStep ),

            typeof( MenuBarv2 ),
            typeof( Shortcut )
        };
    }
}
