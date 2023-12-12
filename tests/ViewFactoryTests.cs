using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ViewFactory ) )]
[Category( "Core" )]
internal class ViewFactoryTests : Tests
{
    private static readonly Type[] KnownUnsupportedTypes_ExpectedTypes =
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

    private static MenuBarItem[] ViewFactory_DefaultMenuBarItems => ViewFactory.DefaultMenuBarItems;

    private static Type[] ViewFactory_KnownUnsupportedTypes => ViewFactory.KnownUnsupportedTypes;

    private static IEnumerable<Type> ViewFactory_SupportedViewTypes => ViewFactory.SupportedViewTypes;

    [Test]
    public void CreateT_DoesNotThrowOnSupportedTypes( [ValueSource( nameof( ViewFactory_SupportedViewTypes ) )] Type supportedType )
    {
        // NUnit does not natively support generic type parameters in test methods, so this is easiest to do via reflection
        MethodInfo viewFactoryCreateTGeneric = typeof( ViewFactory ).GetMethods( ).Single( m => m is { IsGenericMethod: true, IsPublic: true, IsStatic: true } );

        MethodInfo viewFactoryCreateTConcrete = viewFactoryCreateTGeneric.MakeGenericMethod( supportedType );

        object? createdView = null;

        Assert.That( ( ) =>
                         createdView = viewFactoryCreateTConcrete
                             .Invoke( null, new object?[] { null, null } ),
                     Throws.Nothing );

        if ( createdView is IDisposable d )
        {
            d.Dispose( );
        }
    }

    [Test]
    public void CreateT_ReturnsValidViewOfExpectedType( [ValueSource( nameof( ViewFactory_SupportedViewTypes ) )] Type supportedType )
    {
        // NUnit does not natively support generic type parameters in test methods, so this is easiest to do via reflection
        MethodInfo viewFactoryCreateTGeneric = typeof( ViewFactory ).GetMethods( ).Single( m => m is { IsGenericMethod: true, IsPublic: true, IsStatic: true } );

        MethodInfo viewFactoryCreateTConcrete = viewFactoryCreateTGeneric.MakeGenericMethod( supportedType );

        object? createdView = viewFactoryCreateTConcrete.Invoke( null, new object?[] { null, null } );

        Assert.That( createdView, Is.Not.Null.And.InstanceOf( supportedType ) );

        if ( createdView is IDisposable d )
        {
            d.Dispose( );
        }
    }

    [Test]
    public void CreateT_ThrowsOnUnsupportedTypes( [ValueSource( nameof( CreateT_ThrowsOnUnsupportedTypes_Cases ) )] Type unsupportedType )
    {
        Type[] a = ViewFactory_SupportedViewTypes.ToArray( );

        // NUnit does not natively support generic type parameters in test methods, so this is easiest to do via reflection
        MethodInfo viewFactoryCreateTGeneric = typeof( ViewFactory ).GetMethods( ).Single( m => m is { IsGenericMethod: true, IsPublic: true, IsStatic: true } );

        MethodInfo viewFactoryCreateTConcrete = viewFactoryCreateTGeneric.MakeGenericMethod( unsupportedType );

        object? createdView = null;
        object?[] methodParameters = { null, null };

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
        Assert.Multiple( ( ) =>
        {
            Assert.That( ViewFactory_DefaultMenuBarItems, Has.Length.EqualTo( 1 ) );
            Assert.That( ViewFactory_DefaultMenuBarItems[ 0 ].Title, Is.EqualTo( "_File (F9)" ) );
        } );

        Assert.Multiple( ( ) =>
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
    public void KnownUnsupportedTypes_DoesNotContainUnexpectedItems( [ValueSource( nameof( ViewFactory_KnownUnsupportedTypes ) )] Type typeDeclaredUnsupportedInViewFactory )
    {
        Assert.That( KnownUnsupportedTypes_ExpectedTypes, Contains.Item( typeDeclaredUnsupportedInViewFactory ) );
    }

    [Test]
    [Category( "Change Control" )]
    public void ViewType_IsTerminalGuiView( )
    {
        Assert.That( ViewFactory.ViewType, Is.EqualTo( typeof( View ) ) );
    }

    private static IEnumerable<Type> CreateT_ThrowsOnUnsupportedTypes_Cases( )
    {
        // Filtering generics out because they'll still throw, but a different exception
        return ViewFactory_KnownUnsupportedTypes.Where( t => !t.IsGenericType );
    }
}
