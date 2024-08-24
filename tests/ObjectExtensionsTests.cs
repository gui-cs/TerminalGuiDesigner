using System;
using System.Collections.Generic;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ObjectExtensions ) )]
[Category( "Core" )]
[NonParallelizable]
[Order( 10 )]
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
    public void ToCodePrimitiveExpression_DoesNotThrowForUnmanagedPrimitives( [ValueSource( nameof( Get_ToCodePrimitiveExpression_DoesNotThrowForUnmanagedPrimitives_Cases ) )] object testValue )
    {
        Assert.That( testValue.ToCodePrimitiveExpression, Throws.Nothing );
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

    private static IEnumerable<object> Get_ToCodePrimitiveExpression_DoesNotThrowForUnmanagedPrimitives_Cases( )
    {
        yield return 1;
        yield return 1L;
        yield return 1F;
        yield return 1D;
        yield return 1M;
        yield return new List<string>( );
        yield return DateTimeOffset.UnixEpoch;
    }

    private static IEnumerable<object?> Get_ToCodePrimitiveExpression_DoesNotThrowOnSupportedAbsoluteTypes_Cases( )
    {
        yield return new DimAbsolute( 10 );
        yield return new PosAbsolute( 10 );
        yield return null;
    }

    private static IEnumerable<object?> Get_ToCodePrimitiveExpression_ThrowsOnUnsupportedNonAbsoluteTypes_Cases( )
    {
        yield return new DimFill( 10 );
        yield return new DimPercent( 10 );
        yield return new PosAnchorEnd( 10 );
        yield return new DimPercent( 10 );
        yield return new PosCenter( );
    }
}
