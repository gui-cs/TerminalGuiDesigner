using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( DimExtensions ) )]
[Category( "Core" )]
[Category( "Terminal.Gui Extensions" )]
[NonParallelizable]
internal class DimExtensionsTests
{
    [Test]
    public void GetDimType_GivesExpectedOutOffset( [Values] DimType dimType, [Values( -2, 0, 2, 36 )] int inOffset )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => ( Dim.Absolute( 24 ) + inOffset ).GetDimType( out _, out _, out int outOffset ) && Is.Zero.ApplyTo( outOffset ).IsSuccess,
                DimType.Percent => ( Dim.Percent( 24 ) + inOffset ).GetDimType( out _, out _, out int outOffset ) && Is.EqualTo( inOffset ).ApplyTo( outOffset ).IsSuccess,
                DimType.Fill => ( Dim.Fill( 24 ) + inOffset ).GetDimType( out _, out _, out int outOffset ) && Is.EqualTo( inOffset ).ApplyTo( outOffset ).IsSuccess,
                DimType.Auto => (Dim.Auto() + inOffset).GetDimType(out _, out _, out int outOffset) && Is.EqualTo(inOffset).ApplyTo(outOffset).IsSuccess,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void GetDimType_GivesExpectedOutType( [Values] DimType dimType )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => Dim.Absolute( 24 ).GetDimType( out DimType outDimType, out _, out _ ) && outDimType == dimType,
                DimType.Percent => Dim.Percent( 24 ).GetDimType( out DimType outDimType, out _, out _ ) && outDimType == dimType,
                DimType.Fill => Dim.Fill( 24 ).GetDimType( out DimType outDimType, out _, out _ ) && outDimType == dimType,
                DimType.Auto => Dim.Auto().GetDimType(out DimType outDimType, out _, out _) && outDimType == dimType,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void GetDimType_GivesExpectedOutValue( [Values] DimType dimType )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => Dim.Absolute( 24 ).GetDimType( out _, out int outValue, out _ ) && Is.EqualTo( 24f ).ApplyTo( outValue ).IsSuccess,
                DimType.Percent => Dim.Percent( 24 ).GetDimType( out _, out int outValue, out _ ) && Is.EqualTo( 24f ).ApplyTo( outValue ).IsSuccess,
                DimType.Fill => Dim.Fill( 24 ).GetDimType( out _, out int outValue, out _ ) && Is.EqualTo( 24f ).ApplyTo( outValue ).IsSuccess,
                DimType.Auto => Dim.Auto().GetDimType(out _, out int outValue, out _) && Is.EqualTo(0).ApplyTo(outValue).IsSuccess,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsAbsolute_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedSize )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => Dim.Absolute( requestedSize ).IsAbsolute( ),
                DimType.Percent => !Dim.Percent( requestedSize ).IsAbsolute( ),
                DimType.Fill => !Dim.Fill( requestedSize ).IsAbsolute( ),
                DimType.Auto => !Dim.Auto().IsAbsolute(),
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
                DimType.Absolute => Dim.Absolute( requestedSize ).IsAbsolute( out int size ) && Is.EqualTo( requestedSize ).ApplyTo( size ).IsSuccess,
                // With an or, because either one being true is a fail
                DimType.Percent => !( Dim.Percent( requestedSize ).IsAbsolute( out int size ) || Is.EqualTo( requestedSize ).ApplyTo( size ).IsSuccess ),
                // With an or, because either one being true is a fail
                DimType.Fill => !( Dim.Fill( requestedSize ).IsAbsolute( out int size ) || Is.EqualTo( requestedSize ).ApplyTo( size ).IsSuccess ),

                DimType.Auto=> !Dim.Auto().IsAbsolute(out _),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsFill_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedMargin )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => !Dim.Absolute( requestedMargin ).IsFill( ),
                DimType.Percent => !Dim.Percent( requestedMargin ).IsFill( ),
                DimType.Fill => Dim.Fill( requestedMargin ).IsFill( ),
                DimType.Auto => !Dim.Auto().IsFill(),
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
                DimType.Absolute => !( Dim.Absolute( requestedMargin ).IsFill( out int margin ) || Is.EqualTo( requestedMargin ).ApplyTo( margin ).IsSuccess ),
                // With an or, because either one being true is a fail
                DimType.Percent => !( Dim.Percent( requestedMargin ).IsFill( out int margin ) || Is.EqualTo( requestedMargin ).ApplyTo( margin ).IsSuccess ),
                // With an and, because either one being false is a fail
                DimType.Fill => Dim.Fill( requestedMargin ).IsFill( out int margin ) && Is.EqualTo( requestedMargin ).ApplyTo( margin ).IsSuccess,
                DimType.Auto => !Dim.Auto().IsFill(out int margin) && Is.EqualTo(0).ApplyTo(margin).IsSuccess,
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    public void IsPercent_AsExpected_WhenCreatedAs( [Values] DimType dimType, [Values( 1, 6, 24, 60 )] int requestedSize )
    {
        Assert.That(
            dimType switch
            {
                DimType.Absolute => !Dim.Absolute( requestedSize ).IsPercent( ),
                DimType.Percent => Dim.Percent( requestedSize ).IsPercent( ),
                DimType.Fill => !Dim.Fill( requestedSize ).IsPercent( ),
                DimType.Auto => !Dim.Auto().IsPercent(),
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
                DimType.Absolute => !( Dim.Absolute( requestedSize ).IsPercent( out int percent ) || Is.EqualTo( requestedSize ).ApplyTo( percent ).IsSuccess ),
                // With an and, because either one being false is a fail
                DimType.Percent => Dim.Percent( requestedSize ).IsPercent( out int percent ) && Is.EqualTo( requestedSize ).ApplyTo( percent ).IsSuccess,
                // With an or, because either one being true is a fail
                DimType.Fill => !( Dim.Fill( requestedSize ).IsPercent( out int percent ) || Is.EqualTo( requestedSize ).ApplyTo( percent ).IsSuccess ),
                DimType.Auto => !(Dim.Auto().IsPercent(out int percent) && Is.EqualTo(0).ApplyTo(percent).IsSuccess),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, "Test needs case for newly-added enum member" )
            } );
    }

    [Test]
    [Sequential]
    public void ToCode_ReturnsExpectedString(
        [Values( DimType.Percent, DimType.Fill, DimType.Absolute )] DimType dimType,
        [Values( "Dim.Percent(50)", "Dim.Fill(5)", "5" )] string expectedCode )
    {
        Assert.That(
            dimType switch
            {
                DimType.Percent => Dim.Percent( 50 ).ToCode( ),
                DimType.Fill => Dim.Fill( 5 ).ToCode( ),
                DimType.Absolute => new DimAbsolute( 5 ).ToCode( ),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, null )
            },
            Is.EqualTo( expectedCode )
        );
    }

    [Test]
    [Sequential]
    public void ToCode_ReturnsExpectedString_WithOffset(
        [Values( DimType.Percent, DimType.Percent, DimType.Fill, DimType.Fill, DimType.Absolute, DimType.Absolute )] DimType dimType,
        [Values( 2, -2, 2, -2, 2, -2 )] int offset,
        [Values( "Dim.Percent(50) + 2", "Dim.Percent(50) - 2", "Dim.Fill(5) + 2", "Dim.Fill(5) - 2", "7","3" )] string expectedCode )
    {
        Assert.That(
            dimType switch
            {
                DimType.Percent => ( Dim.Percent( 50 ) + offset ).ToCode( ),
                DimType.Fill => ( Dim.Fill( 5 ) + offset ).ToCode( ),
                DimType.Absolute => (new DimAbsolute( 5 )+ offset).ToCode( ),
                _ => throw new ArgumentOutOfRangeException( nameof( dimType ), dimType, null )
            },
            Is.EqualTo( expectedCode )
        );
    }
}
