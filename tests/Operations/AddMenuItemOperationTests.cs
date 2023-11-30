using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.MenuOperations;

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
            ClassicAssert.IsNotNull(v.Menus[0].Children[0], "Expected a new MenuBar added in Designer to have a placeholder MenuItem entry");
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            var first = v.Menus[0].Children[0];
            firstMenuItemName = first.Title.ToString();

            var add = new AddMenuItemOperation(first);

            ClassicAssert.IsNotNull(v.Menus[0].Children[0], "Expected no changes until we actually run the operation");
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);

            add.Do();
            
            ClassicAssert.AreEqual(2, v.Menus[0].Children.Length);
            ClassicAssert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            ClassicAssert.AreEqual("", v.Menus[0].Children[1].Title.ToString(),"Expected new menu items to have no text initially (user Types to enter them)");
        }, out _);

        ClassicAssert.AreEqual(2, viewIn.Menus[0].Children.Length);
        ClassicAssert.AreEqual(firstMenuItemName, viewIn.Menus[0].Children[0].Title.ToString(), "Expected save/reload to have no effect on menu names");
        ClassicAssert.AreEqual("", viewIn.Menus[0].Children[1].Title.ToString());
    }

    [Test]
    public void TestAddingMenuItem_AtRoot_UnDo()
    {
        // The text of the placeholder MenuItem that was created by Designer
        // when the first MenuBar was added by ViewFactory
        string? firstMenuItemName = null;

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.IsNotNull(v.Menus[0].Children[0], "Expected a new MenuBar added in Designer to have a placeholder MenuItem entry");
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            var first = v.Menus[0].Children[0];
            firstMenuItemName = first.Title.ToString();

            var add = new AddMenuItemOperation(first);

            ClassicAssert.IsNotNull(v.Menus[0].Children[0], "Expected no changes until we actually run the operation");
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);

            add.Do();
            ClassicAssert.AreEqual(2, v.Menus[0].Children.Length);
            ClassicAssert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            ClassicAssert.AreEqual("", v.Menus[0].Children[1].Title.ToString(), "Expected new menu items to have no text initially (user Types to enter them)");

            // curve ball undo it a bunch of times and redo
            add.Undo();
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            add.Undo();
            add.Undo();

            add.Redo();
            ClassicAssert.AreEqual(2, v.Menus[0].Children.Length);
            ClassicAssert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");
            ClassicAssert.AreEqual("", v.Menus[0].Children[1].Title.ToString(), "Expected new menu items to have no text initially (user Types to enter them)");

            add.Undo();
            add.Undo();
            add.Undo();
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.AreSame(first, v.Menus[0].Children[0], "Expected new item to be below the original (unchanged) item");

        }, out _);

        ClassicAssert.AreEqual(1, viewIn.Menus[0].Children.Length);
        ClassicAssert.AreEqual(firstMenuItemName, viewIn.Menus[0].Children[0].Title.ToString(), "Expected save/reload to have no effect on menu names");
    }
}
