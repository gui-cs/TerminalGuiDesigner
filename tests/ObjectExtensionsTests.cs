using System.Collections.Generic;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ObjectExtensions ) )]
[Category( "Core" )]
[Parallelizable( ParallelScope.All )]
internal class ObjectExtensionsTests
{
    [Test]
    public void String_CastToReflected( )
    {
        const string str = "cat";
        object? obj = null;

        Assert.That( ( ) => obj = str.CastToReflected( typeof( object ) ), Throws.Nothing );

        //TODO: This is always going to return true if the previous assertion passed, because every object is an object.
        Assert.That( obj, Is.Not.Null.And.InstanceOf<object>( ) );
    }

    [Test]
    public void ToCodePrimitiveExpression_DoesNotThrowOnSupportedAbsoluteTypes( [ValueSource( nameof( Get_ToCodePrimitiveExpression_DoesNotThrowOnSupportedAbsoluteTypes_Cases ) )] object? testValue )
    {
        Assert.That( testValue.ToCodePrimitiveExpression, Throws.Nothing );
    }

    [Test]
    public void ToCodePrimitiveExpression_ThrowsOnUnsupportedNonAbsoluteTypes( [ValueSource( nameof( Get_ToCodePrimitiveExpression_ThrowsOnUnsupportedNonAbsoluteTypes_Cases ) )] object? testValue )
    {
        Assert.That( testValue.ToCodePrimitiveExpression, Throws.ArgumentException );
    }

    private static IEnumerable<object?> Get_ToCodePrimitiveExpression_DoesNotThrowOnSupportedAbsoluteTypes_Cases( )
    {
        yield return new Dim.DimAbsolute( 10 );
        yield return new Pos.PosAbsolute( 10 );
        yield return null;
    }

    private static IEnumerable<object?> Get_ToCodePrimitiveExpression_ThrowsOnUnsupportedNonAbsoluteTypes_Cases( )
    {
        yield return new Dim.DimFill( 10 );
        yield return new Dim.DimFactor( 10 );
        yield return new Pos.PosAnchorEnd( 10 );
        yield return new Pos.PosFactor( 10 );
        yield return new Pos.PosCenter( );
    }
}
