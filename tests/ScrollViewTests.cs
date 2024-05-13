namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
internal class ScrollViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveContentSize( [Values( 1, 5, 25, 100 )] int width, [Values( 1, 5, 25, 100 )] int height )
    {
        using ScrollView scrollViewIn = RoundTrip<View, ScrollView>(
            ( _, s ) => { s.ContentSize = new( width, height ); }, out ScrollView? scrollViewOut );

        Assert.Multiple( ( ) =>
        {
            Assert.That( scrollViewIn, Is.Not.SameAs( scrollViewOut ) );
            Assert.That( scrollViewIn.ContentSize.Value.Width, Is.EqualTo( width ) );
            Assert.That( scrollViewIn.ContentSize.Value.Height, Is.EqualTo( height ) );
        } );
    }

    [Test]
    public void TestRoundTrip_PreserveContentViews( [Values( "blarggg" )] string text, [Values( "myLbl" )] string fieldName )
    {
        using Label lbl = new (){ Text = text };
        using ScrollView scrollViewIn = RoundTrip<View, ScrollView>(
            ( d, _ ) =>
            {
                AddViewOperation op = new ( lbl, d, fieldName );
                op.Do( );
            }, out _ );

        using View child = scrollViewIn.GetActualSubviews( ).Single( );
        Assert.That( child, Is.InstanceOf<Label>( ) );
        Assert.That( child.Data, Is.InstanceOf<Design>( ) );

        Design? lblIn = (Design)child.Data;
        Assert.That( lblIn.FieldName, Is.EqualTo( fieldName ) );
        Assert.That( lblIn.View.Text, Is.EqualTo( text ) );
    }

    [Test]
    public void TestRoundTrip_ScrollViewInsideTabView_PreserveContentViews( )
    {
        using ScrollView scrollOut = ViewFactory.Create<ScrollView>( );
        using Button buttonOut = ViewFactory.Create<Button>( );

        using TabView tabIn = RoundTrip<View, TabView>(
            ( d, _ ) =>
            {
                // Add a ScrollView to the first Tab
                new AddViewOperation( scrollOut, d, "myTabView" ).Do( );

                // Add a Button to the ScrollView
                new AddViewOperation( buttonOut, (Design)scrollOut.Data, "myButton" ).Do( );
            }, out _ );

        // The ScrollView should contain the Button
        Assert.That( scrollOut.GetActualSubviews( ).ToArray( ), Does.Contain( buttonOut ) );

        // The TabView read back in should contain the ScrollView
        using ScrollView scrollIn = tabIn.Tabs.First( )
                                         .View.GetActualSubviews( )
                                         .OfType<ScrollView>( )
                                         .Single( );

        using View buttonIn = scrollIn.GetActualSubviews( ).Single( );
        Assert.That( buttonIn, Is.InstanceOf<Button>( ) );
        Assert.That( buttonIn, Is.Not.SameAs( buttonOut ) );
    }
}