using NUnit.Framework;
using System;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.TabOperations;

namespace UnitTests.Operations
{
    internal class MoveTabOperationTests : Tests
    {
        [Test]
        public void TestMoveTab_WrongViewType_Throws()
        {
            // Label is not a TabView so should get Exception
            RoundTrip<Window, Label>((d, v) =>
            {
                Assert.Throws<ArgumentException>(()=>new MoveTabOperation(d, 1));
            }, out _);
        }

        [Test]
        public void TestMoveTab_OnlyOneTab_IsImpossible()
        {
            RoundTrip<Window, TabView>((d, v) =>
            {
                Assert.AreEqual(2, v.Tabs.Count, $"Expected {nameof(ViewFactory)} to create a {nameof(TabView)} with 2 example placeholder tabs");
                v.RemoveTab(v.Tabs.ElementAt(1));
                Assert.AreEqual(1, v.Tabs.Count);

                Assert.IsTrue(new MoveTabOperation(d,1).IsImpossible,"Should be impossible to move tab when there is only one of them");
            }, out _);
        }



        [TestCase(3,-8,0,true)] // move left lots
        [TestCase(3, 55, 5, true)] // move right lots
        [TestCase(0, 0, 0, false)] // move 0 nowhere
        [TestCase(0, 1, 1, true)] // move 0 right 1
        [TestCase(1, -1, 0, true)] // move 1 left 1
        public void TestMoveTab_DoUndo(int idxToMove, int adjustment, int expectedNewIndex, bool expectPossible)
        {
            RoundTrip<Window, TabView>((d, v) =>
            {
                new AddTabOperation(d, "NewTab").Do();
                new AddTabOperation(d, "NewTab").Do();
                new AddTabOperation(d, "NewTab").Do();
                new AddTabOperation(d, "NewTab").Do();

                var toMove = v.Tabs.ElementAt(idxToMove);
                var originalIndex = v.Tabs.IndexOf(toMove);

                v.SelectedTab = toMove;
                var op = new MoveTabOperation(d, adjustment);

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

                Assert.AreEqual(expectedNewIndex, v.Tabs.IndexOf(toMove));

                op.Undo();
                Assert.AreEqual(originalIndex, v.Tabs.IndexOf(toMove));

                op.Redo();
                Assert.AreEqual(expectedNewIndex, v.Tabs.IndexOf(toMove));

            }, out _);
        }
    }
}
