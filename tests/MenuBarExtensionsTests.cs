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
    [Test]
    [NonParallelizable]
    public void ScreenToMenuBarItem_MultipleMenuItems_ReturnsExpectedItem_IfItemsClicked(
        [Values( 5, 6 )] int clickXCoordinate,
        [Values( 0, 5 )] int xOffset,
        [Values( 0, 1 )] int yOffset )
    {
        const int expectedItemWidth = 6;

        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            v.X = xOffset;
            v.Y = yOffset;

            // Expect a MenuBar to be rendered that is 
            // ".test..next..more.." (with 1 unit of preceding whitespace and 2 after each)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            v.Menus[ 0 ].Title = "test";

            Assume.That( ( ) => new AddMenuOperation( d, "next" ).Do( ), Throws.Nothing );
            Assume.That( ( ) => new AddMenuOperation( d, "more" ).Do( ), Throws.Nothing );

            Assume.That( v.Menus, Has.Exactly( 3 ).InstanceOf<MenuBarItem>( ) );

            // Clicks in the "test" region
            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.SameAs( v.Menus[ ( clickXCoordinate - 1 ) / expectedItemWidth ] ) );
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

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.SameAs( v.Menus[ 0 ] ) );
        }, out _ );
    }

    [Test]
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

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.SameAs( v.Menus[ 0 ] ) );
        }, out _ );
    }

    [Test]
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

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( clickXCoordinate + xOffset ), Is.Null,
                         "Expected Terminal.Gui MenuBar to have 1 unit of whitespace before and 2 after any MenuBarItems (e.g. File) get rendered. This may change in future, if so then update this test." );
        }, out _ );
    }
    
    [Test]
    public void TestSetShortcut( )
    {
        var si = new StatusItem( Key.F, "ff", ( ) => { } );
        ClassicAssert.AreEqual( Key.F, si.Shortcut );

        si.SetShortcut( Key.B );
        ClassicAssert.AreEqual( Key.B, si.Shortcut );
    }
}
