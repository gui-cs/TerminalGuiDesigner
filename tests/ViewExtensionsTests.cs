namespace UnitTests;

[TestFixture]
[TestOf( typeof( ViewExtensions ) )]
[Category( "Terminal.Gui Extensions" )]
internal class ViewExtensionsTests : Tests
{
    // These values represent a view with corners at 2,3 and 6,5
    private const int TestViewLeftEdge = 2;
    private const int TestViewTopEdge = 3;
    private const int TestViewWidth = 5;
    private const int TestViewHeight = 3;
    private const int TestViewRightEdge = TestViewLeftEdge + TestViewWidth - 1;
    private const int TestViewBottomEdge = TestViewTopEdge + TestViewHeight - 1;
    private const int OneLeftOfTestViewLeftEdge = TestViewLeftEdge - 1;
    private const int OneRightOfTestViewRightEdge = TestViewRightEdge + 1;
    private const int OneAboveTestViewTopEdge = TestViewTopEdge - 1;
    private const int OneBelowTestViewBottomEdge = TestViewBottomEdge + 1;

    [Test]
    public void HitTest_HitsWhenExpected( [Range( OneLeftOfTestViewLeftEdge, OneRightOfTestViewRightEdge, 1 )] int clickX, [Range( OneAboveTestViewTopEdge, OneBelowTestViewBottomEdge, 1 )] int clickY )
    {
        using View v = Get10By10View( ).View;

        v.X = TestViewLeftEdge;
        v.Y = TestViewTopEdge;
        v.Width = TestViewWidth;
        v.Height = TestViewHeight;

        Application.Top.Add( v );

        var result = v.HitTest( new( )
        {
            X = clickX,
            Y = clickY,
        }, out _, out _ );

        Assert.That( ( clickX, clickY ) switch
        {
            // Beyond any edge, result should be null
            (< TestViewLeftEdge, _) => result is null,
            (_, < TestViewTopEdge) => result is null,
            (> TestViewRightEdge, _) => result is null,
            (_, > TestViewBottomEdge) => result is null,
            // Everything else is a hit and result should be a reference to the view
            _ => ReferenceEquals( v, result )
        } );
    }

    [Test]
    public void HitTest_IsBorderWhenExpected( [Range( OneLeftOfTestViewLeftEdge, OneRightOfTestViewRightEdge, 1 )] int clickX, [Range( OneAboveTestViewTopEdge, OneBelowTestViewBottomEdge, 1 )] int clickY )
    {
        using View v = Get10By10View().View;

        v.X = TestViewLeftEdge;
        v.Y = TestViewTopEdge;
        v.Width = TestViewWidth;
        v.Height = TestViewHeight;

        Application.Top.Add( v );

        _ = v.HitTest(
            new ()
            {
                X = clickX,
                Y = clickY,
            }, out bool isBorder, out _ );

        switch ( clickX, clickY )
        {
            // If exactly on the left or right edge, and within the vertical bounds, it's a border
            case (TestViewLeftEdge, >= TestViewTopEdge and <= TestViewBottomEdge):
            case (TestViewRightEdge, >= TestViewTopEdge and <= TestViewBottomEdge):
            // If exactly on the top or bottom edge, and within the horizontal bounds, it's a border
            case (>= TestViewLeftEdge and <= TestViewRightEdge, TestViewTopEdge):
            case (>= TestViewLeftEdge and <= TestViewRightEdge, TestViewBottomEdge):
                Assert.That( isBorder );
                break;
            // Otherwise, it's not
            default:
                Assert.That( isBorder, Is.False );
                break;
        }
    }

    [Test]
    public void HitTest_IsLowerRightWhenExpected( [Range( OneLeftOfTestViewLeftEdge, OneRightOfTestViewRightEdge, 1 )] int clickX, [Range( OneAboveTestViewTopEdge, OneBelowTestViewBottomEdge, 1 )] int clickY )
    {
        using View v = Get10By10View( ).View;

        v.X = TestViewLeftEdge;
        v.Y = TestViewTopEdge;
        v.Width = TestViewWidth;
        v.Height = TestViewHeight;

        Application.Top.Add( v );

        _ = v.HitTest(
            new( )
            {
                X = clickX,
                Y = clickY,
            }, out _, out bool isLowerRight );

        switch ( clickX, clickY )
        {
            // Only true when within 2 units of the lower left, and still in bounds.
            // The 2 constant should probably be exposed internally for testing.
            case (> TestViewRightEdge - 2 and <= TestViewRightEdge, > TestViewBottomEdge - 2 and <= TestViewBottomEdge):
                Assert.That( isLowerRight );
                break;
            // Otherwise, it's not
            default:
                Assert.That( isLowerRight, Is.False );
                break;
        }
    }

    [TestCase(typeof(Label), false)]
    [TestCase(typeof(TableView), false)]
    [TestCase(typeof(TabView), true)]
    [TestCase(typeof(View), true)]
    [TestCase(typeof(Window), true)]
    public void TestIsContainerView(Type viewType, bool expectIsContainerView)
    {
        var inst = (View?)Activator.CreateInstance(viewType)
            ?? throw new Exception("CreateInstance returned null!");

        ClassicAssert.AreEqual(expectIsContainerView, inst.IsContainerView());
    }

    [TestCase(typeof(Label), false)]
    [TestCase(typeof(TableView), false)]
    [TestCase(typeof(TabView), false)]
    [TestCase(typeof(Window), false)]
    [TestCase(typeof(View), true)]
    public void TestOutOfBox_IsBorderlessContainerView(Type viewType, bool expectResult)
    {
        var inst = (View?)Activator.CreateInstance(viewType)
            ?? throw new Exception("CreateInstance returned null!");

        ClassicAssert.AreEqual(expectResult, inst.IsBorderlessContainerView());
    }

    [Test]
    public void TestHitTest_WindowWithFrameView_InBorder()
    {
        var w = new Window();
        var f = new FrameView()
        {
            Width = 5,
            Height = 5,
        };

        w.Add(f);
        Application.Begin(w);
        w.LayoutSubviews();

        ClassicAssert.AreSame(w, w.HitTest(new MouseEvent { X = 0, Y = 0 }, out var isBorder, out _),
            "Expected 0,0 to be the window border (its client area should start at 1,1)");
        ClassicAssert.IsTrue(isBorder);

        // 1,1
        ClassicAssert.AreSame(f, w.HitTest(new MouseEvent { X = 1, Y = 1 }, out isBorder, out _),
            "Expected 1,1 to be the Frame border (its client area should start at 1,1)");
        ClassicAssert.IsTrue(isBorder);
    }
}
