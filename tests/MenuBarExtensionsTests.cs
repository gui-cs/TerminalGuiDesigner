using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( MenuBarExtensions ) )]
[Category( "Terminal.Gui Extensions" )]
[Category( "UI" )]
[NonParallelizable]
internal class MenuBarExtensionsTests : Tests
{
    private static Mouse At( int x, int y = 0 ) => new Mouse { Position = new Point( x, y ) };

    /// <summary>
    /// Expects menu like
    /// 0123456789
    ///  test  next
    ///
    /// This tests that a click in screen space finds the correct top-level menu item.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ScreenToMenuBarItem_MultipleMenuItems_ReturnsExpectedItem_IfItemsClicked(
        [Values( 1, 4 )] int clickXCoordinate,
        [Values( 0 )] int expectedMenuItem )
    {
        RoundTrip<View, MenuBar>(( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.SuperView!.LayoutSubViews();

            v.SubViews.OfType<MenuBarItem>().First().Title = "test";
            Assume.That( ( ) => new AddMenuOperation(App, d, "next" ).Do( ), Throws.Nothing );
            Assume.That( ( ) => new AddMenuOperation(App, d, "more" ).Do( ), Throws.Nothing );
            Assume.That( v.SubViews.OfType<MenuBarItem>(), Has.Exactly( 3 ).InstanceOf<MenuBarItem>( ) );

            var a = v.ScreenToMenuBarItem( App, At( clickXCoordinate ) );
            var b = v.SubViews.OfType<MenuBarItem>().ElementAt( expectedMenuItem );
            Assert.That( a, Is.SameAs( b ) );
        }, out _ );
    }

    [Test]
    public void ScreenToMenuBarItem_MultipleMenuItems_ReturnsNull_IfClickedBeforeAndAfterItems(
        [Values( 0, 19 )] int clickXCoordinate )
    {
        RoundTrip<View, MenuBar>(( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.SuperView!.LayoutSubViews();

            v.SubViews.OfType<MenuBarItem>().First().Title = "test";
            Assume.That( ( ) => new AddMenuOperation(App, d, "next" ).Do( ), Throws.Nothing );
            Assume.That( ( ) => new AddMenuOperation(App, d, "more" ).Do( ), Throws.Nothing );
            Assume.That( v.SubViews.OfType<MenuBarItem>(), Has.Exactly( 3 ).InstanceOf<MenuBarItem>( ) );

            Assert.That( v.ScreenToMenuBarItem( App, At( clickXCoordinate ) ), Is.Null );
        }, out _ );
    }

    [Test]
    [Order( 3 )]
    public void ScreenToMenuBarItem_OneMenuItem_ReturnsExpectedMenuBarItem_IfClickedWithin2AfterItem(
        [Values( 5, 6 )] int clickXCoordinate )
    {
        RoundTrip<View, MenuBar>(( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.SuperView!.LayoutSubViews();

            Assume.That( v.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.SubViews.OfType<MenuBarItem>().First().Title = "test";

            Assert.That( v.ScreenToMenuBarItem( App, At( clickXCoordinate ) ), Is.SameAs( v.SubViews.OfType<MenuBarItem>().First() ) );
        }, out _ );
    }

    [Test]
    [Order( 2 )]
    public void ScreenToMenuBarItem_OneMenuItem_ReturnsExpectedMenuBarItem_IfItemClicked(
        [Range( 1, 4 )] int clickXCoordinate )
    {
        RoundTrip<View, MenuBar>(( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.SuperView!.LayoutSubViews();

            Assume.That( v.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.SubViews.OfType<MenuBarItem>().First().Title = "test";

            Assert.That( v.ScreenToMenuBarItem( App, At( clickXCoordinate ) ), Is.SameAs( v.SubViews.OfType<MenuBarItem>().First() ) );
        }, out _ );
    }

    [Test]
    [Order( 1 )]
    public void ScreenToMenuBarItem_OneMenuItem_ReturnsNull_IfClickedBeforeAndAfterItems(
        [Values( 0, 7 )] int clickXCoordinate )
    {
        RoundTrip<View, MenuBar>(( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.SuperView!.LayoutSubViews();

            Assume.That( v.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.SubViews.OfType<MenuBarItem>().First().Title = "test";

            Assert.That( v.ScreenToMenuBarItem( App, At( clickXCoordinate ) ), Is.Null,
                         "Expected click before/after items to return null." );
        }, out _ );
    }
}
