using System;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( DimExtensions ) )]
[Category( "Core" )]
[Category( "Terminal.Gui Extensions" )]
[Parallelizable( ParallelScope.Children )]
[DefaultFloatingPointTolerance( 0.00001f )]
internal class DimExtensionsTests
{
    [Test]
    public void GetDimType_GivesExpectedOutOffset( [Values] DimType dimType, [Values( -2, 0, 2, 36 )] int inOffset )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => ( Dim.Sized( 24 ) + inOffset ).GetDimType( out _, out _, out int outOffset ) && Is.Zero.ApplyTo( outOffset ).IsSuccess,
                DimType.Percent => ( Dim.Percent( 24 ) + inOffset ).GetDimType( out _, out _, out int outOffset ) && Is.EqualTo( inOffset ).ApplyTo( outOffset ).IsSuccess,
                DimType.Fill => ( Dim.Fill( 24 ) + inOffset ).GetDimType( out _, out _, out int outOffset ) && Is.EqualTo( inOffset ).ApplyTo( outOffset ).IsSuccess,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void GetDimType_GivesExpectedOutType( [Values] DimType dimType )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => Dim.Sized( 24 ).GetDimType( out DimType outDimType, out _, out _ ) && outDimType == dimType,
                DimType.Percent => Dim.Percent( 24 ).GetDimType( out DimType outDimType, out _, out _ ) && outDimType == dimType,
                DimType.Fill => Dim.Fill( 24 ).GetDimType( out DimType outDimType, out _, out _ ) && outDimType == dimType,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void GetDimType_GivesExpectedOutValue( [Values] DimType dimType )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => Dim.Sized( 24 ).GetDimType( out _, out float outValue, out _ ) && Is.EqualTo( 24f ).ApplyTo( outValue ).IsSuccess,
                DimType.Percent => Dim.Percent( 24 ).GetDimType( out _, out float outValue, out _ ) && Is.EqualTo( 24f ).ApplyTo( outValue ).IsSuccess,
                DimType.Fill => Dim.Fill( 24 ).GetDimType( out _, out float outValue, out _ ) && Is.EqualTo( 24f ).ApplyTo( outValue ).IsSuccess,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsAbsolute_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedSize )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => Dim.Sized( requestedSize ).IsAbsolute( ),
                DimType.Percent => !Dim.Percent( requestedSize ).IsAbsolute( ),
                DimType.Fill => !Dim.Fill( requestedSize ).IsAbsolute( ),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsAbsolute_With_OutPercent_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedSize )
    {
        Assert.That(
            dimType switch
            {
                // With an and, because either one being false is a fail
                DimType.Absolute => Dim.Sized( requestedSize ).IsAbsolute( out int size ) && Is.EqualTo( requestedSize ).ApplyTo( size ).IsSuccess,
                // With an or, because either one being true is a fail
                DimType.Percent => !( Dim.Percent( requestedSize ).IsAbsolute( out int size ) || Is.EqualTo( requestedSize ).ApplyTo( size ).IsSuccess ),
                // With an or, because either one being true is a fail
                DimType.Fill => !( Dim.Fill( requestedSize ).IsAbsolute( out int size ) || Is.EqualTo( requestedSize ).ApplyTo( size ).IsSuccess ),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsFill_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedMargin )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => !Dim.Sized( requestedMargin ).IsFill( ),
                DimType.Percent => !Dim.Percent( requestedMargin ).IsFill( ),
                DimType.Fill => Dim.Fill( requestedMargin ).IsFill( ),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsFill_With_OutPercent_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedMargin )
    {
        Assert.That(
            dimType switch
            {
                // With an or, because either one being true is a fail
                DimType.Absolute => !( Dim.Sized( requestedMargin ).IsFill( out int margin ) || Is.EqualTo( requestedMargin ).ApplyTo( margin ).IsSuccess ),
                // With an or, because either one being true is a fail
                DimType.Percent => !( Dim.Percent( requestedMargin ).IsFill( out int margin ) || Is.EqualTo( requestedMargin ).ApplyTo( margin ).IsSuccess ),
                // With an and, because either one being false is a fail
                DimType.Fill => Dim.Fill( requestedMargin ).IsFill( out int margin ) && Is.EqualTo( requestedMargin ).ApplyTo( margin ).IsSuccess,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsPercent_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedSize )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => !Dim.Sized( requestedSize ).IsPercent( ),
                DimType.Percent => Dim.Percent( requestedSize ).IsPercent( ),
                DimType.Fill => !Dim.Fill( requestedSize ).IsPercent( ),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsPercent_With_OutPercent_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedSize )
    {
        Assert.That(
            dimType switch
            {
                // With an or, because either one being true is a fail
                DimType.Absolute => !( Dim.Sized( requestedSize ).IsPercent( out float percent ) || Is.EqualTo( requestedSize ).ApplyTo( percent ).IsSuccess ),
                // With an and, because either one being false is a fail
                DimType.Percent => Dim.Percent( requestedSize ).IsPercent( out float percent ) && Is.EqualTo( requestedSize ).ApplyTo( percent ).IsSuccess,
                // With an or, because either one being true is a fail
                DimType.Fill => !( Dim.Fill( requestedSize ).IsPercent( out float percent ) || Is.EqualTo( requestedSize ).ApplyTo( percent ).IsSuccess ),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    [Category( "Change Control" )]
    public void NullDim_ActsLikeAbsoluteZero( )
    {
        var v = new View( );

        Assert.That( v.Width, Is.Null, "As of v1.7.0 a new View started getting null for its Width, if this assert fails it means that behaviour was reverted and this test can be altered or suppressed" );

        Assert.That( v.Width.IsAbsolute( ) );
        Assert.That( v.Width.IsAbsolute( out int n ) );
        Assert.That( n, Is.Zero );

        Assert.That( v.Width.IsFill( ), Is.False );
        Assert.That( v.Width.IsPercent( ), Is.False );
        Assert.That( v.Width.IsCombine( ), Is.False );

        Assert.That( v.Width.GetDimType( out var type, out var val, out _ ) );

        Assert.That( type, Is.EqualTo( DimType.Absolute ) );
        Assert.That( val, Is.Zero );
    }

    [Test]
    [Sequential]
    public void ToCode_ReturnsExpectedString(
        [Values( DimType.Percent, DimType.Fill )] DimType dimType,
        [Values( "Dim.Percent(50f)", "Dim.Fill(5)" )] string expectedCode )
    {
        Assert.That(
            dimType switch
            {
                DimType.Percent => Dim.Percent( 50 ).ToCode( ),
                DimType.Fill => Dim.Fill( 5 ).ToCode( )
            },
            Is.EqualTo( expectedCode )
        );
    }

    [Test]
    [Sequential]
    public void ToCode_ReturnsExpectedString_WithOffset(
        [Values( DimType.Percent, DimType.Percent, DimType.Fill, DimType.Fill )] DimType dimType,
        [Values( 2, -2, 2, -2 )] int offset,
        [Values( "Dim.Percent(50f) + 2", "Dim.Percent(50f) - 2", "Dim.Fill(5) + 2", "Dim.Fill(5) - 2" )] string expectedCode )
    {
        Assert.That(
            dimType switch
            {
                DimType.Percent => ( Dim.Percent( 50 ) + offset ).ToCode( ),
                DimType.Fill => ( Dim.Fill( 5 ) + offset ).ToCode( )
            },
            Is.EqualTo( expectedCode )
        );
    }
}
