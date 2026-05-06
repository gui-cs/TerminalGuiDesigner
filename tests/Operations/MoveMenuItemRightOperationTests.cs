using Terminal.Gui.Input;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

internal class MoveMenuItemRightOperationTests : Tests
{
    [Test]
    public void TestMoveMenuItemRightOperation_ImpossibleIfSolo()
    {
        RoundTrip<Runnable, MenuBar>((d, v) =>
        {
            var op = new MoveMenuItemRightOperation(App, v.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0]);
            ClassicAssert.True(op.IsImpossible, "Expected it to be impossible to move first menu item to submenu when there is nothing above it");

        }, out _);
    }

    [Test]
    public void TestMoveMenuItemRightOperation_MoveToSubmenu()
    {
        RoundTrip<Runnable, MenuBar>((d, v) =>
        {
            var fileMenu = v.SubViews.OfType<MenuBarItem>().First();
            new AddMenuItemOperation(App, fileMenu.GetMenuItems(out _)[0]).Do();

            ClassicAssert.IsTrue(
                new MoveMenuItemRightOperation(App, fileMenu.GetMenuItems(out _)[0]).IsImpossible,
                "Expected you still not to be able to move index 0 (because there is nothing above it)");

            var toMove = fileMenu.GetMenuItems(out _)[1];
            var op = new MoveMenuItemRightOperation(App, toMove);
            ClassicAssert.IsFalse(op.IsImpossible);

            ClassicAssert.AreEqual(2, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.IsInstanceOf<MenuItem>(fileMenu.GetMenuItems(out _)[0]);
            op.Do();

            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.IsInstanceOf<MenuBarItem>(fileMenu.GetMenuItems(out _)[0], "Expected top entry to be converted to the Type that has sub items");
            ClassicAssert.Contains(toMove, ((MenuBarItem)fileMenu.GetMenuItems(out _)[0]).GetMenuItems(out _));

        }, out _);
    }


    [Test]
    public void TestMoveMenuItemRightOperation_UndoRedo_RememberShortcut()
    {
        RoundTrip<Runnable, MenuBar>((d, v) =>
        {
            var fileMenu = v.SubViews.OfType<MenuBarItem>().First();
            new AddMenuItemOperation(App, fileMenu.GetMenuItems(out _)[0]).Do();

            var items = fileMenu.GetMenuItems(out _);
            var toMove = items[1];

            items[0].Data = "yarg";
            items[0].Key = Key.Y.WithCtrl;
            items[1].Data = "blarg";
            items[1].Key = Key.B.WithCtrl;

            // Move blarg to sub-menu of yarg
            var op = new MoveMenuItemRightOperation(App, toMove);
            op.Do();

            var afterDo = fileMenu.GetMenuItems(out _);
            ClassicAssert.AreEqual("yarg", afterDo[0].Data);
            ClassicAssert.AreEqual(Key.Y.WithCtrl, afterDo[0].Key);
            ClassicAssert.AreEqual("blarg", ((MenuBarItem)afterDo[0]).GetMenuItems(out _)[0].Data);
            ClassicAssert.AreEqual(Key.B.WithCtrl, ((MenuBarItem)afterDo[0]).GetMenuItems(out _)[0].Key);

            op.Undo();
            var afterUndo = fileMenu.GetMenuItems(out _);
            ClassicAssert.AreEqual("yarg", afterUndo[0].Data);
            ClassicAssert.AreEqual(Key.Y.WithCtrl, afterUndo[0].Key);
            ClassicAssert.AreEqual("blarg", afterUndo[1].Data);
            ClassicAssert.AreEqual(Key.B.WithCtrl, afterUndo[1].Key);

            op.Redo();
            var afterRedo = fileMenu.GetMenuItems(out _);
            ClassicAssert.AreEqual("yarg", afterRedo[0].Data);
            ClassicAssert.AreEqual(Key.Y.WithCtrl, afterRedo[0].Key);
            ClassicAssert.AreEqual("blarg", ((MenuBarItem)afterRedo[0]).GetMenuItems(out _)[0].Data);
            ClassicAssert.AreEqual(Key.B.WithCtrl, ((MenuBarItem)afterRedo[0]).GetMenuItems(out _)[0].Key);

        }, out _);
    }
}
