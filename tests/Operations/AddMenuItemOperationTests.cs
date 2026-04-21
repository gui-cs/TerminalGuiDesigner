using Terminal.Gui.ViewBase;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

[TestFixture]
[TestOf(typeof(AddMenuItemOperation))]
[Category("UI")]
internal class AddMenuItemOperationTests : Tests
{
    [Test]
    public void TestAddingMenuItem_AtRoot_Do()
    {
        // The text of the placeholder MenuItem that was created by Designer
        // when the first MenuBar was added by ViewFactory
        string? firstMenuItemName = null;

        var viewIn = RoundTrip<View, MenuBar>((d, menuBar) =>
        {
            var fileMenu = menuBar.SubViews.OfType<MenuBarItem>().First();
            ClassicAssert.IsNotNull(fileMenu.GetMenuItems(out _)[0], "Expected a new MenuBar added in Designer to have a placeholder MenuItem entry");
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            var first = fileMenu.GetMenuItems(out _)[0];
            firstMenuItemName = first.Title;

            var add = new AddMenuItemOperation(App, first);

            ClassicAssert.IsNotNull(fileMenu.GetMenuItems(out _)[0], "Expected no changes until we actually run the operation");
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);

            add.Do();

            ClassicAssert.AreEqual(2, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreSame(first, fileMenu.GetMenuItems(out _)[0], "Expected new item to be below the original (unchanged) item");
            ClassicAssert.AreEqual("", fileMenu.GetMenuItems(out _)[1].Title.ToString(), "Expected new menu items to have no text initially (user Types to enter them)");
        }, out _);

        var viewInFileMenu = viewIn.SubViews.OfType<MenuBarItem>().First();
        ClassicAssert.AreEqual(2, viewInFileMenu.GetMenuItems(out _).Count);
        ClassicAssert.AreEqual(firstMenuItemName, viewInFileMenu.GetMenuItems(out _)[0].Title, "Expected save/reload to have no effect on menu names");
        ClassicAssert.AreEqual("", viewInFileMenu.GetMenuItems(out _)[1].Title.ToString());
    }

    [Test]
    public void TestAddingMenuItem_AtRoot_UnDo()
    {
        // The text of the placeholder MenuItem that was created by Designer
        // when the first MenuBar was added by ViewFactory
        string? firstMenuItemName = null;

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            var fileMenu = v.SubViews.OfType<MenuBarItem>().First();
            ClassicAssert.IsNotNull(fileMenu.GetMenuItems(out _)[0], "Expected a new MenuBar added in Designer to have a placeholder MenuItem entry");
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            var first = fileMenu.GetMenuItems(out _)[0];
            firstMenuItemName = first.Title;

            var add = new AddMenuItemOperation(App, first);

            ClassicAssert.IsNotNull(fileMenu.GetMenuItems(out _)[0], "Expected no changes until we actually run the operation");
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);

            add.Do();
            ClassicAssert.AreEqual(2, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreSame(first, fileMenu.GetMenuItems(out _)[0], "Expected new item to be below the original (unchanged) item");
            ClassicAssert.AreEqual("", fileMenu.GetMenuItems(out _)[1].Title.ToString(), "Expected new menu items to have no text initially (user Types to enter them)");

            // curve ball undo it a bunch of times and redo
            add.Undo();
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreSame(first, fileMenu.GetMenuItems(out _)[0], "Expected new item to be below the original (unchanged) item");
            add.Undo();
            add.Undo();

            add.Redo();
            ClassicAssert.AreEqual(2, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreSame(first, fileMenu.GetMenuItems(out _)[0], "Expected new item to be below the original (unchanged) item");
            ClassicAssert.AreEqual("", fileMenu.GetMenuItems(out _)[1].Title.ToString(), "Expected new menu items to have no text initially (user Types to enter them)");

            add.Undo();
            add.Undo();
            add.Undo();
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreSame(first, fileMenu.GetMenuItems(out _)[0], "Expected new item to be below the original (unchanged) item");

        }, out _);

        var viewInFileMenu = viewIn.SubViews.OfType<MenuBarItem>().First();
        ClassicAssert.AreEqual(1, viewInFileMenu.GetMenuItems(out _).Count);
        ClassicAssert.AreEqual(firstMenuItemName, viewInFileMenu.GetMenuItems(out _)[0].Title, "Expected save/reload to have no effect on menu names");
    }
}
