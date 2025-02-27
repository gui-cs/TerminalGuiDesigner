using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( MenuBarExtensions ) )]
[Category( "Terminal.Gui Extensions" )]
[Category( "UI" )]
[NonParallelizable]
internal class MenuBarExtensionsTests : Tests
{
    /// <summary>
    /// Expects menu like
    /// 0123456789
    ///  test  next
    ///
    /// This tests that the click in screen space finds the menu (File, Edit etc.).
    /// Furthermore, it tests that works even when the MenuBar is not at the origin
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ScreenToMenuBarItem_MultipleMenuItems_ReturnsExpectedItem_IfItemsClicked(
        [Values( 1, 4 )] int clickXCoordinate,
        [Values( 0, 3 )] int xOffset,
        [Values( 0, 1 )] int yOffset,
        [Values(0)]int expectedMenuItem)
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.X = xOffset;
            v.Y = yOffset;

            v.SuperView!.LayoutSubviews();

            // Expect a MenuBar to be rendered that is 
            // ".test..next..more.." (with 1 unit of preceding whitespace and 1 after each)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            v.Menus[ 0 ].Title = "test";

            Assume.That( ( ) => new AddMenuOperation( d, "next" ).Do( ), Throws.Nothing );
            Assume.That( ( ) => new AddMenuOperation( d, "more" ).Do( ), Throws.Nothing );

            Assume.That( v.Menus, Has.Exactly( 3 ).InstanceOf<MenuBarItem>( ) );

            // Clicks in the "test" region
            var a = v.ScreenToMenuBarItem(clickXCoordinate + xOffset);
            var b = v.Menus[expectedMenuItem];
            Assert.That( a, Is.SameAs(b));
        }, out _ );
    }

    [Test]
    public void ScreenToMenuBarItem_MultipleMenuItems_ReturnsNull_IfClickedBeforeAndAfterItems(
        [Values( 0, 19 )] int clickXCoordinate,
        [Values( 0, 5 )] int xOffset,
        [Values( 0, 1 )] int yOffset )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.X = xOffset;
            v.Y = yOffset;

            v.SuperView!.LayoutSubviews();

            // Expect a MenuBar to be rendered that is 
            // ".test..next..more.." (with 1 unit of preceding whitespace and 2 after each)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            v.Menus[ 0 ].Title = "test";

            Assume.That( ( ) => new AddMenuOperation( d, "next" ).Do( ), Throws.Nothing );
            Assume.That( ( ) => new AddMenuOperation( d, "more" ).Do( ), Throws.Nothing );

            Assume.That( v.Menus, Has.Exactly( 3 ).InstanceOf<MenuBarItem>( ) );

            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.Null );
        }, out _ );
    }

    [Test]
    [Order( 3 )]
    public void ScreenToMenuBarItem_OneMenuItem_ReturnsExpectedMenuBarItem_IfClickedWithin2AfterItem(
        [Values( 5, 6 )] int clickXCoordinate,
        [Values( 0, 5 )] int xOffset,
        [Values( 0, 1 )] int yOffset )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.X = xOffset;
            v.Y = yOffset;

            v.SuperView!.LayoutSubviews();
             
            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.SameAs( v.Menus[ 0 ] ) );
        }, out _ );
    }

    [Test]
    [Order( 2 )]
    public void ScreenToMenuBarItem_OneMenuItem_ReturnsExpectedMenuBarItem_IfItemClicked(
        [Range( 1, 4 )] int clickXCoordinate,
        [Values( 0, 5 )] int xOffset,
        [Values( 0, 1 )] int yOffset )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.X = xOffset;
            v.Y = yOffset;

            v.SuperView!.LayoutSubviews();

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.SameAs( v.Menus[ 0 ] ) );
        }, out _ );
    }

    [Test]
    [Order( 1 )]
    public void ScreenToMenuBarItem_OneMenuItem_ReturnsNull_IfClickedBeforeAndAfterItems(
        [Values( 0, 7 )] int clickXCoordinate,
        [Values( 0, 5 )] int xOffset,
        [Values( 0, 1 )] int yOffset )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.X = xOffset;
            v.Y = yOffset;

            v.SuperView!.LayoutSubviews();

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.Null,
                         "Expected Terminal.Gui MenuBar to have 1 unit of whitespace before and 2 after any MenuBarItems (e.g. File) get rendered. This may change in future, if so then update this test." );
        }, out _ );
    }
}
