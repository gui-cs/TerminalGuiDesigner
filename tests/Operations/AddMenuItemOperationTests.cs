using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations;

internal class AddMenuItemOperationTests : Tests
{
    [Test]
    public void TestAddingMenuItem_AtRoot_Do()
    {
        // The text of the placeholder MenuItem that was created by Designer
        // when the first MenuBar was added by ViewFactory
        string? firstMenuItemName = null;

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            Assert.IsNotNull(v.Menus[0].Children[0], "Expected a new MenuBar added in Designer to have a placeholder MenuItem entry");
            Assert.AreEqual(1, v.Menus[0].Children.Length);
            var first = v.Menus[0].Children[0];
            firstMenuItemName = first.Title.ToString();

            var add = new AddMenuItemOperation(first);

            Assert.IsNotNull(v.Menus[0].Children[0], "Expected no changes until we actually run the operation");
            Assert.AreEqual(1, v.Menus[0].Children.Length);

            add.Do();
            
            Assert.AreEqual(2, v.Menus[0].Children.Length);
            Assert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            Assert.AreEqual("", v.Menus[0].Children[1].Title.ToString(),"Expected new menu items to have no text initially (user Types to enter them)");
        }, out _);

        Assert.AreEqual(2, viewIn.Menus[0].Children.Length);
        Assert.AreEqual(firstMenuItemName, viewIn.Menus[0].Children[0].Title.ToString(), "Expected save/reload to have no effect on menu names");
        Assert.AreEqual("", viewIn.Menus[0].Children[1].Title.ToString());
    }

    [Test]
    public void TestAddingMenuItem_AtRoot_UnDo()
    {
        // The text of the placeholder MenuItem that was created by Designer
        // when the first MenuBar was added by ViewFactory
        string? firstMenuItemName = null;

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            Assert.IsNotNull(v.Menus[0].Children[0], "Expected a new MenuBar added in Designer to have a placeholder MenuItem entry");
            Assert.AreEqual(1, v.Menus[0].Children.Length);
            var first = v.Menus[0].Children[0];
            firstMenuItemName = first.Title.ToString();

            var add = new AddMenuItemOperation(first);

            Assert.IsNotNull(v.Menus[0].Children[0], "Expected no changes until we actually run the operation");
            Assert.AreEqual(1, v.Menus[0].Children.Length);

            add.Do();
            Assert.AreEqual(2, v.Menus[0].Children.Length);
            Assert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            Assert.AreEqual("", v.Menus[0].Children[1].Title.ToString(), "Expected new menu items to have no text initially (user Types to enter them)");

            // curve ball undo it a bunch of times and redo
            add.Undo();
            Assert.AreEqual(1, v.Menus[0].Children.Length);
            Assert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            add.Undo();
            add.Undo();

            add.Redo();
            Assert.AreEqual(2, v.Menus[0].Children.Length);
            Assert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            Assert.AreEqual("", v.Menus[0].Children[1].Title.ToString(), "Expected new menu items to have no text initially (user Types to enter them)");

            add.Undo();
            add.Undo();
            add.Undo();
            Assert.AreEqual(1, v.Menus[0].Children.Length);
            Assert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");

        }, out _);

        Assert.AreEqual(1, viewIn.Menus[0].Children.Length);
        Assert.AreEqual(firstMenuItemName, viewIn.Menus[0].Children[0].Title.ToString(), "Expected save/reload to have no effect on menu names");
    }
}
