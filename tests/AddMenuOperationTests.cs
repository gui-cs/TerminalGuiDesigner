using NUnit.Framework;
using System;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace UnitTests
{
    internal class AddMenuOperationTests : Tests
    {
        [Test]
        public void TestAddMenu_InvalidViewType()
        {
            var d = Get10By10View();
            var ex = Assert.Throws<ArgumentException>(() => new AddMenuOperation(d,"haha!"));
            Assert.AreEqual("Design must be for a MenuBar to support AddMenuOperation", ex?.Message);
        }

        [Test]
        public void TestAddingMenu_AtRoot_Do()
        {
            const string expectedTopLevelMenuName = "_File (F9)";

            var viewIn = RoundTrip<View, MenuBar>((d, v) =>
            {
                Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
                Assert.AreEqual(1, v.Menus[0].Children.Length, "Expected 1 placeholder example MenuItem under that menu");
                var first = v.Menus[0];

                var add = new AddMenuOperation(d,"Blarg");
                Assert.AreEqual(1, v.Menus[0].Children.Length, "Expected no changes until we actually run the operation");

                add.Do();

                Assert.AreEqual(2, v.Menus.Length,"Expected a new top level menu to be added");
                Assert.AreSame(first, v.Menus[0], "Expected new item to be right of the original");
                Assert.AreEqual("Blarg", v.Menus[1].Title.ToString());

            }, out _);

            Assert.AreEqual(2, viewIn.Menus.Length);
            Assert.AreEqual(expectedTopLevelMenuName, viewIn.Menus[0].Title.ToString());
            Assert.AreEqual("Blarg", viewIn.Menus[1].Title.ToString());
        }

        [Test]
        public void TestAddingMenu_AtRoot_UnDo()
        {
            const string expectedTopLevelMenuName = "_File (F9)";

            var viewIn = RoundTrip<View, MenuBar>((d, v) =>
            {
                Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
                Assert.AreEqual(1, v.Menus[0].Children.Length, "Expected 1 placeholder example MenuItem under that menu");
                var first = v.Menus[0];

                var add = new AddMenuOperation(d, "Blarg");
                Assert.AreEqual(1, v.Menus[0].Children.Length, "Expected no changes until we actually run the operation");

                add.Do();
                Assert.AreEqual(2, v.Menus.Length, "Expected a new top level menu to be added");
                Assert.AreSame(first, v.Menus[0], "Expected new item to be right of the original");
                Assert.AreEqual("Blarg", v.Menus[1].Title.ToString());

                add.Undo();
                Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString());
                Assert.AreEqual(1, v.Menus[0].Children.Length);

                add.Redo();
                Assert.AreEqual(2, v.Menus.Length);
                Assert.AreSame(first, v.Menus[0]);
                Assert.AreEqual("Blarg", v.Menus[1].Title.ToString());

                add.Undo();
                add.Undo();
                add.Undo();
                add.Undo();
                Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString());
                Assert.AreEqual(1, v.Menus[0].Children.Length);

            }, out _);

            Assert.AreEqual(1, viewIn.Menus.Length);
            Assert.AreEqual(expectedTopLevelMenuName, viewIn.Menus[0].Title.ToString());
        }
    }
}
