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
    public void ScreenToMenuBarItem_AtOrigin_OneMenuItem_ReturnsNull_IfClickedBeforeAndAfterItems( [Values( 0, 7 )] int xCoordinate )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( xCoordinate ), Is.Null,
                         "Expected Terminal.Gui MenuBar to have 1 unit of whitespace before and 2 after any MenuBarItems (e.g. File) get rendered. This may change in future, if so then update this test." );
        }, out _ );
    }

    [Test]
    public void ScreenToMenuBarItem_AtOrigin_OneMenuItem_ReturnsExpectedMenuBarItem_IfItemClicked( [Range( 1, 4 )] int xCoordinate )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( xCoordinate ), Is.SameAs( v.Menus[ 0 ] ) );
        }, out _ );
    }

    [Test]
    public void ScreenToMenuBarItem_AtOrigin_OneMenuItem_ReturnsExpectedMenuBarItem_IfClickedWithin2AfterItem( [Values( 5, 6 )] int xCoordinate )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            // Expect a MenuBar to be rendered that is 
            // ".test.." (with 1 unit of preceding whitespace and 2 after)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            Assume.That( v.Menus, Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
            v.Menus[ 0 ].Title = "test";

            Assert.That( v.ScreenToMenuBarItem( xCoordinate ), Is.SameAs( v.Menus[ 0 ] ) );
        }, out _ );
    }

    [Test]
    public void ScreenToMenuBarItem_AtOrigin_MultipleMenuItems_ReturnsNull_IfClickedBeforeAndAfterItems( [Values( 0, 19 )] int xCoordinate )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            // Expect a MenuBar to be rendered that is 
            // ".test..next..more.." (with 1 unit of preceding whitespace and 2 after each)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            v.Menus[ 0 ].Title = "test";

            Assume.That( ( ) => new AddMenuOperation( d, "next" ).Do( ), Throws.Nothing );
            Assume.That( ( ) => new AddMenuOperation( d, "more" ).Do( ), Throws.Nothing );

            Assume.That( v.Menus, Has.Exactly( 3 ).InstanceOf<MenuBarItem>( ) );

            Assert.That( v.ScreenToMenuBarItem( xCoordinate ), Is.Null );
        }, out _ );
    }

    [Test]
    [NonParallelizable]
    public void ScreenToMenuBarItem_AtOrigin_MultipleMenuItems_ReturnsExpectedItem_IfItemsClicked(
        [Range( 1, 18 )] int xCoordinate,
        [Values( 6 )] int expectedItemWidth )
    {
        RoundTrip<View, MenuBar>( ( d, v ) =>
        {
            Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
            Assume.That( v, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

            // Expect a MenuBar to be rendered that is 
            // ".test..next..more.." (with 1 unit of preceding whitespace and 2 after each)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            v.Menus[ 0 ].Title = "test";

            Assume.That( ( ) => new AddMenuOperation( d, "next" ).Do( ), Throws.Nothing );
            Assume.That( ( ) => new AddMenuOperation( d, "more" ).Do( ), Throws.Nothing );

            Assume.That( v.Menus, Has.Exactly( 3 ).InstanceOf<MenuBarItem>( ) );

            // Clicks in the "test" region
            Assert.That( v.ScreenToMenuBarItem( xCoordinate ), Is.SameAs( v.Menus[ ( xCoordinate - 1 ) / expectedItemWidth ] ) );
        }, out _ );
    }

    [Test]
    public void TestScreenToMenuBarItem_WithOffset_MultipleMenuItem()
    {
        RoundTrip<View, MenuBar>((d, v) =>
        {
            // Start the MenuBar a bit along the screen
            v.X = 5;
            v.Y = 1;

            // Expect a MenuBar to be rendered that is 
            // ".test..next..more.." (with 1 unit of preceding whitespace and 2 after each)
            // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
            v.Menus[0].Title = "test";

            new AddMenuOperation(d, "next").Do();
            new AddMenuOperation(d, "more").Do();

            ClassicAssert.IsNull(v.ScreenToMenuBarItem(5+0));

            // Clicks in the "test" region
            ClassicAssert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+1));
            ClassicAssert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+2));
            ClassicAssert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+3));
            ClassicAssert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+4));
            ClassicAssert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+5));
            ClassicAssert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+6));

            // Clicks in the "next" region
            ClassicAssert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+7));
            ClassicAssert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+8));
            ClassicAssert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+9));
            ClassicAssert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+10));
            ClassicAssert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+11));
            ClassicAssert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+12));


            // Clicks in the "more" region
            ClassicAssert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+13));
            ClassicAssert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+14));
            ClassicAssert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+15));
            ClassicAssert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+16));
            ClassicAssert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+17));
            ClassicAssert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+18));

            // clicks off the end of the right
            ClassicAssert.IsNull(v.ScreenToMenuBarItem(5+19));
        }, out _);
    }

    [Test]
    public void TestSetShortcut()
    {
        var si = new StatusItem(Key.F, "ff", () => { });
        ClassicAssert.AreEqual(Key.F, si.Shortcut);
        
        si.SetShortcut(Key.B);
        ClassicAssert.AreEqual(Key.B, si.Shortcut);
    }
}
