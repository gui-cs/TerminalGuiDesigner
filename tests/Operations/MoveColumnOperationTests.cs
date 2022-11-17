using NUnit.Framework;
using System;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations.TableViewOperations;

namespace UnitTests.Operations
{
    internal class MoveColumnOperationTests : Tests
    {
        [Test]
        public void TestMoveColumn_WrongViewType_Throws()
        {
            // Label is not a TableView so should get Exception
            RoundTrip<Window, Label>((d, v) =>
            {
                Assert.Throws<ArgumentException>(()=>new MoveColumnOperation(d,new System.Data.DataColumn(), 1));
            }, out _);
        }

        [Test]
        public void TestMoveColumn_ColumnNotInTableView_Throws()
        {
            
            RoundTrip<Window, TableView>((d, v) =>
            {
                // DataColumn is new and not in v.Table
                Assert.Throws<ArgumentException>(() =>
                    new MoveColumnOperation(d, new System.Data.DataColumn(), 1));
            }, out _);
        }


        [Test]
        public void TestMoveColumn_OnlyOneColumn_IsImpossible()
        {
            RoundTrip<Window, TableView>((d, v) =>
            {
                Assert.AreEqual(4, v.Table.Columns.Count, $"Expected {nameof(ViewFactory)} to create a {nameof(TableView)} with 4 example placeholder Columns");
                v.Table.Columns.RemoveAt(1);
                v.Table.Columns.RemoveAt(1);
                v.Table.Columns.RemoveAt(1);
                Assert.AreEqual(1, v.Table.Columns.Count);

                Assert.IsTrue(new MoveColumnOperation(d, v.Table.Columns[0],1).IsImpossible,"Should be impossible to move Column when there is only one of them");
            }, out _);
        }

        [TestCase(3,-8,0,true)] // move left lots
        [TestCase(2, 55, 4, true)] // move right lots
        [TestCase(0, 0, 0, false)] // move 0 nowhere
        [TestCase(0, 1, 1, true)] // move 0 right 1
        [TestCase(1, -1, 0, true)] // move 1 left 1
        public void TestMoveColumn_DoUndo(int idxToMove, int adjustment, int expectedNewIndex, bool expectPossible)
        {
            RoundTrip<Window, TableView>((d, v) =>
            {
                Assert.AreEqual(4, v.Table.Columns.Count, $"Expected {nameof(ViewFactory)} to create a {nameof(TableView)} with 4 example placeholder Columns");

                var toMove = v.Table.Columns[idxToMove];
                var originalIndex = toMove.Ordinal;

                var op = new MoveColumnOperation(d, toMove, adjustment);

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

                Assert.AreEqual(expectedNewIndex, toMove.Ordinal);

                op.Undo();
                Assert.AreEqual(originalIndex, toMove.Ordinal);

                op.Redo();
                Assert.AreEqual(expectedNewIndex, toMove.Ordinal);

            }, out _);
        }
    }
}
