using System.IO;
using System.Linq;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( PosExtensions ) )]
[Category( "Core" )]
[Category( "Terminal.Gui Extensions" )]
[DefaultFloatingPointTolerance( 0.0001D )]
internal class PosTests : Tests
{
    private static IEnumerable<TestCaseData> GetCode_Cases
    {
        get
        {
            View v = new( );
            Design d = new( new( new FileInfo( "yarg.cs" ) ), "myView", v );
            return new[] { -10, -1, 0, 1, 10 }.SelectMany( offset =>
            {
                string offsetString = offset switch
                {
                    < 0 => $" - {-offset}",
                    0 => string.Empty,
                    > 0 => $" + {offset}"
                };
                return new TestCaseData[]
                {
                    new( Pos.At( 50 ) + offset, $"{50 + offset}", d, v ),
                    new( Pos.AnchorEnd( 5 ) + offset, $"Pos.AnchorEnd(5){offsetString}", d, v ),
                    new( Pos.Center( ) + offset, $"Pos.Center(){offsetString}", d, v ),
                    new( Pos.Percent( 50 ) + offset, $"Pos.Percent(50f){offsetString}", d, v ),
                    new( Pos.Top( v ) + offset, $"Pos.Top(myView){offsetString}", d, v ),
                    new( Pos.Bottom( v ) + offset, $"Pos.Bottom(myView){offsetString}", d, v ),
                    new( Pos.Left( v ) + offset, $"Pos.Left(myView){offsetString}", d, v ),
                    new( Pos.Right( v ) + offset, $"Pos.Right(myView){offsetString}", d, v ),
                    new( Pos.X( v ) + offset, $"Pos.Left(myView){offsetString}", d, v ),
                    new( Pos.Y( v ) + offset, $"Pos.Top(myView){offsetString}", d, v )
                };
            } );
        }
    }

    private static IEnumerable<TestCaseData> GetPosType_OutputsCorrectOffset_Cases
    {
        get
        {
            View v = new( );
            Design d = new( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedTrueTestCaseData( Pos.At( 50 ), 0, null ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( 5 ), 0, null ),
                new ExpectedTrueTestCaseData( Pos.Center( ), 0, null ),
                new ExpectedTrueTestCaseData( Pos.Percent( 5 ), 0, null ),
                new ExpectedTrueTestCaseData( Pos.Top( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Bottom( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Left( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Right( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Y( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.X( v ), 0, d ),
                // Now with an offset
                new ExpectedTrueTestCaseData( Pos.At( 50 ) + 5, 0, null ),
                new ExpectedTrueTestCaseData( Pos.At( 50 ) - 5, 0, null ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( 5 ) + 5, 5, null ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( 5 ) - 5, -5, null ),
                new ExpectedTrueTestCaseData( Pos.Center( ) + 5, +5, null ),
                new ExpectedTrueTestCaseData( Pos.Center( ) - 5, -5, null ),
                new ExpectedTrueTestCaseData( Pos.Percent( 5 ) + 5, 5, null ),
                new ExpectedTrueTestCaseData( Pos.Percent( 5 ) - 5, -5, null ),
                new ExpectedTrueTestCaseData( Pos.Top( v ) + 5, 5, d ),
                new ExpectedTrueTestCaseData( Pos.Top( v ) - 5, -5, d ),
                new ExpectedTrueTestCaseData( Pos.Bottom( v ) + 5, 5, d ),
                new ExpectedTrueTestCaseData( Pos.Bottom( v ) - 5, -5, d ),
                new ExpectedTrueTestCaseData( Pos.Left( v ) + 5, 5, d ),
                new ExpectedTrueTestCaseData( Pos.Left( v ) - 5, -5, d ),
                new ExpectedTrueTestCaseData( Pos.Right( v ) + 5, 5, d ),
                new ExpectedTrueTestCaseData( Pos.Right( v ) - 5, -5, d ),
                new ExpectedTrueTestCaseData( Pos.Y( v ) + 5, 5, d ),
                new ExpectedTrueTestCaseData( Pos.Y( v ) - 5, -5, d ),
                new ExpectedTrueTestCaseData( Pos.X( v ) + 5, 5, d ),
                new ExpectedTrueTestCaseData( Pos.X( v ) - 5, -5, d )
            };
        }
    }

    private static IEnumerable<TestCaseData> GetPosType_OutputsCorrectType_Cases
    {
        get
        {
            View v = new( );
            Design d = new( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedTrueTestCaseData( Pos.At( 50 ), PosType.Absolute, null ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( 5 ), PosType.AnchorEnd, null ),
                new ExpectedTrueTestCaseData( Pos.Center( ), PosType.Center, null ),
                new ExpectedTrueTestCaseData( Pos.Percent( 5 ), PosType.Percent, null ),
                new ExpectedTrueTestCaseData( Pos.Top( v ), PosType.Relative, d ),
                new ExpectedTrueTestCaseData( Pos.Bottom( v ), PosType.Relative, d ),
                new ExpectedTrueTestCaseData( Pos.Left( v ), PosType.Relative, d ),
                new ExpectedTrueTestCaseData( Pos.Right( v ), PosType.Relative, d ),
                new ExpectedTrueTestCaseData( Pos.Y( v ), PosType.Relative, d ),
                new ExpectedTrueTestCaseData( Pos.X( v ), PosType.Relative, d )
            };
        }
    }

    private static IEnumerable<TestCaseData> GetPosType_OutputsCorrectValue_Cases
    {
        get
        {
            View v = new( );
            Design d = new( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedTrueTestCaseData( Pos.At( 50 ), 50, null ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( 5 ), 5, null ),
                new ExpectedTrueTestCaseData( Pos.Center( ), 0, null ),
                new ExpectedTrueTestCaseData( Pos.Percent( 5 ), 5, null ),
                new ExpectedTrueTestCaseData( Pos.Top( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Bottom( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Left( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Right( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.Y( v ), 0, d ),
                new ExpectedTrueTestCaseData( Pos.X( v ), 0, d )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsAbsolute_Cases
    {
        get
        {
            View v = new( );
            _ = new Design( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedTrueTestCaseData( Pos.At( 50 ) ),
                new ExpectedTrueTestCaseData( null ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Center( ) ),
                new ExpectedFalseTestCaseData( Pos.Percent( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Top( v ) ),
                new ExpectedFalseTestCaseData( Pos.Bottom( v ) ),
                new ExpectedFalseTestCaseData( Pos.Left( v ) ),
                new ExpectedFalseTestCaseData( Pos.Right( v ) ),
                new ExpectedFalseTestCaseData( Pos.Y( v ) ),
                new ExpectedFalseTestCaseData( Pos.X( v ) )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsAbsolute_WithOutParam_Cases
    {
        get
        {
            return new TestCaseData[]
            {
                new ExpectedTrueTestCaseData( Pos.At( 50 ), 50 ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( 5 ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Center( ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Percent( 5 ), 0 )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsAnchorEnd_Cases
    {
        get
        {
            View v = new( );
            _ = new Design( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedFalseTestCaseData( Pos.At( 50 ) ),
                new ExpectedFalseTestCaseData( null ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( ) ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Center( ) ),
                new ExpectedFalseTestCaseData( Pos.Percent( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Top( v ) ),
                new ExpectedFalseTestCaseData( Pos.Bottom( v ) ),
                new ExpectedFalseTestCaseData( Pos.Left( v ) ),
                new ExpectedFalseTestCaseData( Pos.Right( v ) ),
                new ExpectedFalseTestCaseData( Pos.Y( v ) ),
                new ExpectedFalseTestCaseData( Pos.X( v ) )
            };
        }
    }
    
    private static IEnumerable<TestCaseData> IsCenter_Cases
    {
        get
        {
            View v = new( );
            _ = new Design( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedFalseTestCaseData( Pos.At( 50 ) ),
                new ExpectedFalseTestCaseData( null ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( ) ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( 5 ) ),
                new ExpectedTrueTestCaseData( Pos.Center( ) ),
                new ExpectedFalseTestCaseData( Pos.Percent( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Top( v ) ),
                new ExpectedFalseTestCaseData( Pos.Bottom( v ) ),
                new ExpectedFalseTestCaseData( Pos.Left( v ) ),
                new ExpectedFalseTestCaseData( Pos.Right( v ) ),
                new ExpectedFalseTestCaseData( Pos.Y( v ) ),
                new ExpectedFalseTestCaseData( Pos.X( v ) )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsAnchorEnd_WithOutParam_Cases
    {
        get
        {
            View v = new( );
            _ = new Design( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedFalseTestCaseData( Pos.At( 50 ), 0 ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( ), 0 ),
                new ExpectedTrueTestCaseData( Pos.AnchorEnd( 5 ), 5 ),
                new ExpectedFalseTestCaseData( Pos.Center( ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Percent( 5 ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Top( v ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Bottom( v ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Left( v ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Right( v ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Y( v ), 0 ),
                new ExpectedFalseTestCaseData( Pos.X( v ), 0 )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsPercent_Cases
    {
        get
        {
            View v = new( );
            _ = new Design( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedFalseTestCaseData( Pos.At( 50 ) ),
                new ExpectedFalseTestCaseData( null ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Center( ) ),
                new ExpectedTrueTestCaseData( Pos.Percent( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Top( v ) ),
                new ExpectedFalseTestCaseData( Pos.Bottom( v ) ),
                new ExpectedFalseTestCaseData( Pos.Left( v ) ),
                new ExpectedFalseTestCaseData( Pos.Right( v ) ),
                new ExpectedFalseTestCaseData( Pos.Y( v ) ),
                new ExpectedFalseTestCaseData( Pos.X( v ) )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsPercent_WithOutParam_Cases
    {
        get
        {
            return new TestCaseData[]
            {
                new ExpectedFalseTestCaseData( Pos.At( 50 ), 0 ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( 5 ), 0 ),
                new ExpectedFalseTestCaseData( Pos.Center( ), 0 ),
                new ExpectedTrueTestCaseData( Pos.Percent( 5 ), 5 )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsRelative_Cases
    {
        get
        {
            View v = new( );
            _ = new Design( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedFalseTestCaseData( Pos.At( 50 ) ),
                new ExpectedFalseTestCaseData( null ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( 5 ) ),
                new ExpectedFalseTestCaseData( Pos.Center( ) ),
                new ExpectedFalseTestCaseData( Pos.Percent( 5 ) ),
                new ExpectedTrueTestCaseData( Pos.Top( v ) ),
                new ExpectedTrueTestCaseData( Pos.Bottom( v ) ),
                new ExpectedTrueTestCaseData( Pos.Left( v ) ),
                new ExpectedTrueTestCaseData( Pos.Right( v ) ),
                new ExpectedTrueTestCaseData( Pos.Y( v ) ),
                new ExpectedTrueTestCaseData( Pos.X( v ) )
            };
        }
    }

    private static IEnumerable<TestCaseData> IsRelative_WithOutParams_Cases
    {
        get
        {
            View v = new( );
            Design d = new( new( new FileInfo( "yarg.cs" ) ), "myView", v );

            return new TestCaseData[]
            {
                new ExpectedFalseTestCaseData( Pos.At( 50 ), null, Side.Left ),
                new ExpectedFalseTestCaseData( Pos.AnchorEnd( 5 ), null, Side.Left ),
                new ExpectedFalseTestCaseData( Pos.Center( ), null, Side.Left ),
                new ExpectedFalseTestCaseData( Pos.Percent( 5 ), null, Side.Left ),
                new ExpectedTrueTestCaseData( Pos.Top( v ), d, Side.Top ),
                new ExpectedTrueTestCaseData( Pos.Bottom( v ), d, Side.Bottom ),
                new ExpectedTrueTestCaseData( Pos.Left( v ), d, Side.Left ),
                new ExpectedTrueTestCaseData( Pos.Right( v ), d, Side.Right ),
                new ExpectedTrueTestCaseData( Pos.Y( v ), d, Side.Top ),
                new ExpectedTrueTestCaseData( Pos.X( v ), d, Side.Left )
            };
        }
    }

    [Test]
    [Category( "Code Generation" )]
    [TestCaseSource( nameof( GetCode_Cases ) )]
    public void GetCode( Pos testPos, string expectedCodeString, Design d, View v )
    {
        Assert.That( testPos.ToCode( new( ) { d } ), Is.EqualTo( expectedCodeString ) );
    }

    [Test]
    [TestCaseSource( nameof( GetPosType_OutputsCorrectOffset_Cases ) )]
    public bool GetPosType_OutputsCorrectOffset( Pos testValue, int expectedOffset, Design? d )
    {
        List<Design> knownDesigns = new( );
        if ( d is not null )
        {
            knownDesigns.Add( d );
        }

        bool getPosTypeSucceeded = testValue.GetPosType( knownDesigns, out _, out _, out _, out _, out int actualOffset );
        Assert.That( actualOffset, Is.EqualTo( expectedOffset ) );
        return getPosTypeSucceeded;
    }

    [Test]
    [TestCaseSource( nameof( GetPosType_OutputsCorrectType_Cases ) )]
    public bool GetPosType_OutputsCorrectType( Pos testValue, PosType expectedPosType, Design? d )
    {
        List<Design> knownDesigns = new( );
        if ( d is not null )
        {
            knownDesigns.Add( d );
        }

        bool getPosTypeSucceeded = testValue.GetPosType( knownDesigns, out PosType actualPosType, out _, out _, out _, out _ );
        Assert.That( actualPosType, Is.EqualTo( expectedPosType ) );
        return getPosTypeSucceeded;
    }

    [Test]
    [TestCaseSource( nameof( GetPosType_OutputsCorrectValue_Cases ) )]
    public bool GetPosType_OutputsCorrectValue( Pos testValue, float expectedValue, Design? d )
    {
        List<Design> knownDesigns = new( );
        if ( d is not null )
        {
            knownDesigns.Add( d );
        }

        bool getPosTypeSucceeded = testValue.GetPosType( knownDesigns, out _, out float actualValue, out _, out _, out _ );
        Assert.That( actualValue, Is.EqualTo( expectedValue ) );
        return getPosTypeSucceeded;
    }

    [Test]
    [TestCaseSource( nameof( IsAbsolute_Cases ) )]
    [NonParallelizable]
    public bool IsAbsolute( Pos testValue )
    {
        return testValue.IsAbsolute( );
    }

    [Test]
    [TestCaseSource( nameof( IsAbsolute_WithOutParam_Cases ) )]
    [NonParallelizable]
    public bool IsAbsolute_WithOutParam( Pos testValue, int expectedOutValue )
    {
        bool isAbsolute = testValue.IsAbsolute( out int actualOutValue );
        Assert.That( actualOutValue, Is.EqualTo( expectedOutValue ) );
        return isAbsolute;
    }

    [Test]
    [TestCaseSource( nameof( IsAnchorEnd_Cases ) )]
    [NonParallelizable]
    public bool IsAnchorEnd( Pos? testValue )
    {
        if ( testValue is null )
        {
            Assert.Ignore( "BUG: Null returns true for this, when it shouldn't" );
        }
        return testValue.IsAnchorEnd( );
    }

    [Test]
    [TestCaseSource( nameof( IsAnchorEnd_WithOutParam_Cases ) )]
    [NonParallelizable]
    public bool IsAnchorEnd_WithOutParam( Pos testValue, int expectedOutValue )
    {
        bool isAnchorEnd = testValue.IsAnchorEnd( out int actualOutValue );
        Assert.That( actualOutValue, Is.EqualTo( expectedOutValue ) );
        return isAnchorEnd;
    }

    [Test]
    [TestCaseSource( nameof( IsCenter_Cases ) )]
    [NonParallelizable]
    public bool IsCenter( Pos? testValue )
    {
        if ( testValue is null )
        {
            Assert.Warn( "BUG: Null returns true for this, when it shouldn't" );
            Assert.Ignore( "BUG: Null returns true for this, when it shouldn't" );
        }
        return testValue.IsCenter( );
    }

    [Test]
    [TestCaseSource( nameof( IsPercent_Cases ) )]
    [NonParallelizable]
    public bool IsPercent( Pos testValue )
    {
        return testValue.IsPercent( );
    }

    [Test]
    [TestCaseSource( nameof( IsPercent_WithOutParam_Cases ) )]
    [NonParallelizable]
    public bool IsPercent_WithOutParam( Pos testValue, float expectedOutValue )
    {
        bool isPercent = testValue.IsPercent( out float actualOutValue );
        Assert.That( actualOutValue, Is.EqualTo( expectedOutValue ) );
        return isPercent;
    }

    [Test]
    [TestCaseSource( nameof( IsRelative_Cases ) )]
    [NonParallelizable]
    public bool IsRelative( Pos? testValue )
    {
        if ( testValue is null )
        {
            Assert.Ignore( "BUG: Null returns true for this, when it shouldn't" );
        }

        return testValue.IsRelative( );
    }

    [Test]
    [TestCaseSource( nameof( IsRelative_WithOutParams_Cases ) )]
    [NonParallelizable]
    public bool IsRelative_WithOutParams( Pos testValue, Design? expectedOutDesign, Side expectedOutSide )
    {
        List<Design> knownDesigns = new( );
        if ( expectedOutDesign is not null )
        {
            knownDesigns.Add( expectedOutDesign );
        }

        bool isRelative = testValue.IsRelative( knownDesigns, out Design? actualOutDesign, out Side actualOutSide );
        Assert.That( actualOutDesign, Is.SameAs( expectedOutDesign ) );
        Assert.That( actualOutSide, Is.EqualTo( expectedOutSide ) );
        return isRelative;
    }

    [Test]
    public void CreatePosRelative( [Values] Side side, [Values( -10, -5, 0, 5, 10 )] int offset )
    {
        View v = new( );
        Design d = new( new( new FileInfo( "yarg.cs" ) ), "myView", v );

        Pos p = d.CreatePosRelative( side, offset );
        if ( offset != 0 )
        {
            Assert.Ignore( "Bug results in offsets returning absolute positions. See PosExtensions.cs" );
        }

        Assert.Multiple( ( ) =>
        {
            Assert.That( p.IsRelative );
            Assert.That( p.IsRelative( new List<Design> { d }, out Design outDesign, out Side outSide ) );
            Assert.That( outDesign, Is.SameAs( d ) );
            Assert.That( outSide, Is.EqualTo( side ) );
        } );
    }

    [Test]
    public void NullPos( )
    {
        var v = new View( );

        Assume.That( v.X, Is.Null, "As of v1.7.0 a new View started getting null for its X, if this assert fails it means that behaviour was reverted and this test can be altered or suppressed" );

        Pos? nullPos = null;
        Assert.That( nullPos.IsAbsolute );
        Assert.That( nullPos.IsAbsolute( out int n ) );
        Assert.That( n, Is.Zero );

        Assert.That( v.X.GetPosType( new List<Design>( ), out var type, out var val, out _, out _, out _ ) );

        Assert.That( type, Is.EqualTo( PosType.Absolute ) );
        Assert.That( val, Is.Zero );
    }

    [Test]
    [Ignore( "Code generation is tested in other tests here" )]
    public void TestRoundTrip_PosAnchorEnd( )
    {
        var viewToCode = new ViewToCode( );

        var file = new FileInfo( "TestRoundTrip_PosAnchorEnd.cs" );
        var designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Window ) );

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var lbl = ViewFactory.Create<Label>( );
        lbl.X = Pos.AnchorEnd( 1 );
        lbl.Y = Pos.AnchorEnd( 4 ); // length of "Heya"

        new AddViewOperation( lbl, designOut, "label1" ).Do( );

        viewToCode.GenerateDesignerCs( designOut, typeof( Window ) );

        var codeToView = new CodeToView( designOut.SourceCode );
        var designBackIn = codeToView.CreateInstance( );

        var lblIn = designBackIn.View.GetActualSubviews( ).OfType<Label>( ).Single( );

        lblIn.X.GetPosType( designBackIn.GetAllDesigns( ).ToList( ), out var backInType, out var backInValue, out _, out _, out var backInOffset );
        Assert.That( backInOffset, Is.Zero );
        Assert.That( backInType, Is.EqualTo( PosType.AnchorEnd ) );
        Assert.That( backInValue, Is.EqualTo( 1 ) );

        lblIn.Y.GetPosType( designBackIn.GetAllDesigns( ).ToList( ), out backInType, out backInValue, out _, out _, out backInOffset );
        Assert.That( backInOffset, Is.Zero );
        Assert.That( backInType, Is.EqualTo( PosType.AnchorEnd ) );
        Assert.That( backInValue, Is.EqualTo( 14 ) );
    }

    [Test]
    [Ignore( "Code generation is tested in other tests here" )]
    public void TestRoundTrip_PosAnchorEnd_WithOffset( )
    {
        var viewToCode = new ViewToCode( );

        var file = new FileInfo( "TestRoundTrip_PosAnchorEnd.cs" );
        var designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Window ) );

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var lbl = ViewFactory.Create<Label>( );
        lbl.X = Pos.AnchorEnd( 1 ) + 5;
        lbl.Y = Pos.AnchorEnd( 4 ) - 3; // length of "Heya"

        new AddViewOperation( lbl, designOut, "label1" ).Do( );

        viewToCode.GenerateDesignerCs( designOut, typeof( Window ) );

        var codeToView = new CodeToView( designOut.SourceCode );
        var designBackIn = codeToView.CreateInstance( );

        var lblIn = designBackIn.View.GetActualSubviews( ).OfType<Label>( ).Single( );

        lblIn.X.GetPosType( designBackIn.GetAllDesigns( ).ToList( ), out var backInType, out var backInValue, out _, out _, out var backInOffset );
        Assert.That( backInOffset, Is.EqualTo( 5 ) );
        Assert.That( backInType, Is.EqualTo( PosType.AnchorEnd ) );
        Assert.That( backInValue, Is.EqualTo( 1 ) );

        lblIn.Y.GetPosType( designBackIn.GetAllDesigns( ).ToList( ), out backInType, out backInValue, out _, out _, out backInOffset );
        Assert.That( backInOffset, Is.EqualTo( -3 ) );
        Assert.That( backInType, Is.EqualTo( PosType.AnchorEnd ) );
        Assert.That( backInValue, Is.EqualTo( 4 ) );
    }

    [Test]
    [TestCase( Side.Left, -2, "X" )]
    [TestCase( Side.Right, 1, "X" )]
    [TestCase( Side.Top, -2, "Y" )]
    [TestCase( Side.Bottom, 5, "Y" )]
    [Ignore( "Code generation is tested in other tests here" )]
    public void TestRoundTrip_PosRelative( Side side, int offset, string property )
    {
        var viewToCode = new ViewToCode( );

        var file = new FileInfo( "TestRoundTrip_PosRelative.cs" );
        var designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Window ) );

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var lbl = ViewFactory.Create<Label>( );
        lbl.X = 50;
        lbl.Y = 50;

        var btn = ViewFactory.Create( typeof( Button ) );

        new AddViewOperation( lbl, designOut, "label1" ).Do( );
        new AddViewOperation( btn, designOut, "btn" ).Do( );

        if ( property == "X" )
        {
            btn.X = PosExtensions.CreatePosRelative( (Design)lbl.Data, side, offset );
        }
        else if ( property == "Y" )
        {
            btn.Y = PosExtensions.CreatePosRelative( (Design)lbl.Data, side, offset );
        }
        else
        {
            throw new ArgumentException( $"Unknown property for test '{property}'" );
        }

        viewToCode.GenerateDesignerCs( designOut, typeof( Window ) );

        var codeToView = new CodeToView( designOut.SourceCode );
        var designBackIn = codeToView.CreateInstance( );

        var btnIn = designBackIn.View.GetActualSubviews( ).OfType<Button>( ).Single( );

        PosType backInType;
        Design? backInRelativeTo;
        Side backInSide;
        int backInOffset;

        if ( property == "X" )
        {
            btnIn.X.GetPosType( designBackIn.GetAllDesigns( ).ToList( ), out backInType, out _, out backInRelativeTo, out backInSide, out backInOffset );
        }
        else
        {
            btnIn.Y.GetPosType( designBackIn.GetAllDesigns( ).ToList( ), out backInType, out _, out backInRelativeTo, out backInSide, out backInOffset );
        }

        Assert.Multiple( ( ) =>
        {
            Assert.That( backInSide, Is.EqualTo( side ) );
            Assert.That( backInType, Is.EqualTo( PosType.Relative ) );
            Assert.That( backInOffset, Is.EqualTo( offset ) );
            Assert.That( backInRelativeTo, Is.Not.Null );
            Assert.That( backInRelativeTo!.View, Is.Not.Null.And.InstanceOf<Label>( ) );
        } );
    }
}
