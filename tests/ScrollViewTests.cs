namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
internal class ScrollViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveContentSize()
    {
        var scrollViewIn = RoundTrip<View, ScrollView>(
            (d, s) =>
                s.ContentSize = new Size(10, 5),
            out var scrollViewOut);

        Assert.That( scrollViewIn, Is.Not.SameAs( scrollViewOut ) );
        Assert.That( scrollViewIn.ContentSize.Width, Is.EqualTo( 10 ) );
        Assert.That( scrollViewIn.ContentSize.Height, Is.EqualTo( 5 ) );
    }

    [Test]
    public void TestRoundTrip_PreserveContentViews()
    {
        var scrollViewIn = RoundTrip<View, ScrollView>(
            (d, s) =>
                {
                    var op = new AddViewOperation(new Label("blarggg"), d, "myLbl");
                    op.Do();
                }, out _);

        var child = scrollViewIn.GetActualSubviews().Single();
        Assert.That( child, Is.InstanceOf<Label>( ) );
        Assert.That( child.Data, Is.InstanceOf<Design>( ) );

        var lblIn = (Design)child.Data;
        Assert.That( lblIn.FieldName, Is.EqualTo( "myLbl" ) );
        Assert.That( lblIn.View.Text, Is.EqualTo( "blarggg" ) );
    }

    [Test]
    public void TestRoundTrip_ScrollViewInsideTabView_PreserveContentViews()
    {
        var scrollOut = ViewFactory.Create(typeof(ScrollView));
        var buttonOut = ViewFactory.Create(typeof(Button));

        var tabIn = RoundTrip<View, TabView>(
            (d, tab) =>
                {
                    // Add a ScrollView to the first Tab
                    new AddViewOperation(scrollOut, d, "myTabView")
                    .Do();

                    // Add a Button to the ScrollView
                    new AddViewOperation(buttonOut, (Design)scrollOut.Data, "myButton")
                    .Do();
                }, out _);

        // The ScrollView should contain the Button
        ClassicAssert.Contains(buttonOut, scrollOut.GetActualSubviews().ToArray());

        // The TabView read back in should contain the ScrollView
        var scrollIn = tabIn.Tabs.First()
                .View.GetActualSubviews()
                .OfType<ScrollView>()
                .Single();

        var butttonIn = scrollIn.GetActualSubviews().Single();
        Assert.That( butttonIn, Is.InstanceOf<Button>( ) );
        Assert.That( butttonIn, Is.Not.SameAs( buttonOut ) );
    }
}
