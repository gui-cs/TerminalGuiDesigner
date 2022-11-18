using NUnit.Framework;
using System;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations
{
    internal class MoveMenuOperationTests : Tests
    {
        [Test]
        public void TestMoveMenu_WrongViewType_Throws()
        {
            // Label is not a MenuBar so should get Exception
            RoundTrip<Window, Label>((d, v) =>
            {
                Assert.Throws<ArgumentException>(()=>new MoveMenuOperation(d,new MenuBarItem(), 1));
            }, out _);
        }

        [Test]
        public void TestMoveMenu_MenuBarItemDoesNotBelongToMenuBar_Throws()
        {
            RoundTrip<Window, MenuBar>((d, v) =>
            {
                // we passed a new MenuBarItem which will not belong to d and therefore should be an Exception
                Assert.Throws<ArgumentException>(() => new MoveMenuOperation(d, new MenuBarItem(), 1));
            }, out _);
        }

        [Test]
        public void TestMoveMenu_OnlyOneMenu_IsImpossible()
        {
            RoundTrip<Window, MenuBar>((d, v) =>
            {
                Assert.AreEqual(1, v.Menus.Length, $"Expected {nameof(ViewFactory)} to create a {nameof(MenuBar)} with 1 example placeholder Menu");
                Assert.IsTrue(new MoveMenuOperation(d, v.Menus[0], 1).IsImpossible,"Should be impossible to move Menu when there is only one of them");
            }, out _);
        }

        [TestCase(3,-8,0,true)] // move left lots
        [TestCase(3, 55, 5, true)] // move right lots
        [TestCase(0, 0, 0, false)] // move 0 nowhere
        [TestCase(0, 1, 1, true)] // move 0 right 1
        [TestCase(1, -1, 0, true)] // move 1 left 1
        public void TestMoveMenu_DoUndo(int idxToMove, int adjustment, int expectedNewIndex, bool expectPossible)
        {
            RoundTrip<Window, MenuBar>((d, v) =>
            {
                new AddMenuOperation(d, "NewMenu").Do();
                new AddMenuOperation(d, "NewMenu").Do();
                new AddMenuOperation(d, "NewMenu").Do();
                new AddMenuOperation(d, "NewMenu").Do();
                new AddMenuOperation(d, "NewMenu").Do();

                var toMove = v.Menus.ElementAt(idxToMove);
                var originalIndex = v.Menus.IndexOf(toMove);
                var op = new MoveMenuOperation(d, toMove, adjustment);

                if(expectPossible)
                {
                    Assert.IsFalse(op.IsImpossible);
                    Assert.IsTrue(op.Do());
                }
                else
                {
                    Assert.IsTrue(op.IsImpossible);
                    Assert.False(op.Do());
                }

                Assert.AreEqual(expectedNewIndex, v.Menus.IndexOf(toMove));

                op.Undo();
                Assert.AreEqual(originalIndex, v.Menus.IndexOf(toMove));

                op.Redo();
                Assert.AreEqual(expectedNewIndex, v.Menus.IndexOf(toMove));

            }, out _);
        }
    }
}
