using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests
{
    internal class MenuBarExtensionsTests : Tests
    {
        [Test]
        public void TestScreenToMenuBarItem_AtOrigin_OneMenuItem()
        {
            RoundTrip<View, MenuBar>((d, v) => 
            {
                // Expect a MenuBar to be rendered that is 
                // ".test.." (with 1 unit of preceding whitespace and 2 after)
                // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
                v.Menus[0].Title = "test";

                Assert.IsNull(v.ScreenToMenuBarItem(0),
                    "Expected Terminal.Gui MenuBar to have 1 unit of whitespace before any MenuBarItems (e.g. File) get rendered. This may change in future, if so then update this test.");
                
                // Clicks in the "test" region
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(1));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(2));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(3));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(4));

                // Clicks in the whitespace after "test" but before any other menus (of which there are none in this test btw)
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(6));

                Assert.IsNull(v.ScreenToMenuBarItem(7), "Expected a click here to be off the end of the Menu 'test' + 2 whitespace characters");
            }, out _);
        }

        [Test]
        public void TestScreenToMenuBarItem_AtOrigin_MultipleMenuItem()
        {
            RoundTrip<View, MenuBar>((d, v) =>
            {
                // Expect a MenuBar to be rendered that is 
                // ".test..next..more.." (with 1 unit of preceding whitespace and 2 after each)
                // Note that this test is brittle and subject to changes in Terminal.Gui e.g. pushing menus closer together.
                v.Menus[0].Title = "test";

                new AddMenuOperation(d, "next").Do();
                new AddMenuOperation(d, "more").Do();

                Assert.IsNull(v.ScreenToMenuBarItem(0));

                // Clicks in the "test" region
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(1));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(2));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(3));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(4));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(6));

                // Clicks in the "next" region
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(7));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(8));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(9));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(10));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(11));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(12));


                // Clicks in the "more" region
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(13));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(14));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(15));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(16));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(17));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(18));

                // clicks off the end of the right
                Assert.IsNull(v.ScreenToMenuBarItem(19));
            }, out _);
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

                Assert.IsNull(v.ScreenToMenuBarItem(5+0));

                // Clicks in the "test" region
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+1));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+2));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+3));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+4));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+5));
                Assert.AreEqual(v.Menus[0], v.ScreenToMenuBarItem(5+6));

                // Clicks in the "next" region
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+7));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+8));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+9));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+10));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+11));
                Assert.AreEqual(v.Menus[1], v.ScreenToMenuBarItem(5+12));


                // Clicks in the "more" region
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+13));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+14));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+15));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+16));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+17));
                Assert.AreEqual(v.Menus[2], v.ScreenToMenuBarItem(5+18));

                // clicks off the end of the right
                Assert.IsNull(v.ScreenToMenuBarItem(5+19));
            }, out _);
        }
    }
}
