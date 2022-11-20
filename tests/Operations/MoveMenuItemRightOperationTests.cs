using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

internal class MoveMenuItemRightOperationTests : Tests
{
    [Test]
    public void TestMoveMenuItemRightOperation_ImpossibleIfSolo()
    {
        RoundTrip<Toplevel, MenuBar>((d, v) =>
        {
            var op = new MoveMenuItemRightOperation(v.Menus[0].Children[0]);
            Assert.True(op.IsImpossible, "Expected it to be impossible to move first menu item to submenu when there is nothing above it");

        }, out _);
    }

    [Test]
    public void TestMoveMenuItemRightOperation_MoveToSubmenu()
    {
        RoundTrip<Toplevel, MenuBar>((d, v) =>
        {
            new AddMenuItemOperation(v.Menus[0].Children[0]).Do();


            Assert.IsTrue(
                new MoveMenuItemRightOperation(v.Menus[0].Children[0]).IsImpossible,
                "Expected you still not to be able to move index 0 (because there is nothing above it)");

            var toMove = v.Menus[0].Children[1];
            var op = new MoveMenuItemRightOperation(toMove);
            Assert.IsFalse(op.IsImpossible);

            Assert.AreEqual(2, v.Menus[0].Children.Length);
            Assert.IsInstanceOf<MenuItem>(v.Menus[0].Children[0]);
            op.Do();

            Assert.AreEqual(1, v.Menus[0].Children.Length);
            Assert.IsInstanceOf<MenuBarItem>(v.Menus[0].Children[0],"Expected top entry to be converted to the Type that has sub items");
            Assert.Contains(toMove,((MenuBarItem)v.Menus[0].Children[0]).Children);

        }, out _);
    }


    [Test]
    public void TestMoveMenuItemRightOperation_UndoRedo_RememberShortcut()
    {
        RoundTrip<Toplevel, MenuBar>((d, v) =>
        {
            new AddMenuItemOperation(v.Menus[0].Children[0]).Do();
            var toMove = v.Menus[0].Children[1];

            v.Menus[0].Children[0].Data = "yarg";
            v.Menus[0].Children[0].Shortcut = Key.CtrlMask | Key.Y;
            v.Menus[0].Children[1].Data = "blarg";
            v.Menus[0].Children[1].Shortcut = Key.CtrlMask | Key.B;

            // Move blarg to sub-menu of yarg
            var op = new MoveMenuItemRightOperation(toMove);
            op.Do();

            Assert.AreEqual("yarg", v.Menus[0].Children[0].Data);
            Assert.AreEqual(Key.CtrlMask | Key.Y, v.Menus[0].Children[0].Shortcut);
            Assert.AreEqual("blarg", ((MenuBarItem)v.Menus[0].Children[0]).Children[0].Data);
            Assert.AreEqual(Key.CtrlMask | Key.B, ((MenuBarItem)v.Menus[0].Children[0]).Children[0].Shortcut);

            op.Undo();
            Assert.AreEqual("yarg", v.Menus[0].Children[0].Data);
            Assert.AreEqual(Key.CtrlMask | Key.Y, v.Menus[0].Children[0].Shortcut);
            Assert.AreEqual("blarg", v.Menus[0].Children[1].Data);
            Assert.AreEqual(Key.CtrlMask | Key.B, v.Menus[0].Children[1].Shortcut);

            op.Redo();
            Assert.AreEqual("yarg", v.Menus[0].Children[0].Data);
            Assert.AreEqual(Key.CtrlMask | Key.Y, v.Menus[0].Children[0].Shortcut);
            Assert.AreEqual("blarg", ((MenuBarItem)v.Menus[0].Children[0]).Children[0].Data);
            Assert.AreEqual(Key.CtrlMask | Key.B, ((MenuBarItem)v.Menus[0].Children[0]).Children[0].Shortcut);

        }, out _);
    }
}
