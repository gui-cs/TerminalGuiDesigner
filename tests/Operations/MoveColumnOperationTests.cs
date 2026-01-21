using TerminalGuiDesigner.Operations.TableViewOperations;

namespace UnitTests.Operations;

internal class MoveColumnOperationTests : Tests
{
    [Test]
    public void TestMoveColumn_WrongViewType_Throws()
    {
        // Label is not a TableView so should get Exception
        RoundTrip<Window, Label>((d, v) =>
        {
            ClassicAssert.Throws<ArgumentException>(()=>new MoveColumnOperation(d,new System.Data.DataColumn(), 1));
        }, out _);
    }

    [Test]
    public void TestMoveColumn_ColumnNotInTableView_Throws()
    {
        
        RoundTrip<Window, TableView>((d, v) =>
        {
            // DataColumn is new and not in v.Table
            ClassicAssert.Throws<ArgumentException>(() =>
                new MoveColumnOperation(d, new System.Data.DataColumn(), 1));
        }, out _);
    }


    [Test]
    public void TestMoveColumn_OnlyOneColumn_IsImpossible()
    {
        RoundTrip<Window, TableView>((d, v) =>
        {
            var dt = v.GetDataTable();

            ClassicAssert.AreEqual(4, dt.Columns.Count, $"Expected {nameof(ViewFactory)} to create a {nameof(TableView)} with 4 example placeholder Columns");
            dt.Columns.RemoveAt(1);
            dt.Columns.RemoveAt(1);
            dt.Columns.RemoveAt(1);
            ClassicAssert.AreEqual(1, v.Table.Columns);

            ClassicAssert.IsTrue(new MoveColumnOperation(d, dt.Columns[0],1).IsImpossible,"Should be impossible to move Column when there is only one of them");
        }, out _);
    }

    [TestCase(3,-8,0,true)] // move left lots
    [TestCase(2, 55, 3, true)] // move right lots
    [TestCase(0, 0, 0, false)] // move 0 nowhere
    [TestCase(0, 1, 1, true)] // move 0 right 1
    [TestCase(1, -1, 0, true)] // move 1 left 1
    public void TestMoveColumn_DoUndo(int idxToMove, int adjustment, int expectedNewIndex, bool expectPossible)
    {
        RoundTrip<Window, TableView>((d, v) =>
        {
            var dt = v.GetDataTable();

            ClassicAssert.AreEqual(4, dt.Columns.Count, $"Expected {nameof(ViewFactory)} to create a {nameof(TableView)} with 4 example placeholder Columns");

            var toMove = dt.Columns[idxToMove];
            var originalIndex = toMove.Ordinal;

            var op = new MoveColumnOperation(d, toMove, adjustment);

            if(expectPossible)
            {
                ClassicAssert.IsFalse(op.IsImpossible);
                ClassicAssert.IsTrue(op.Do());
            }
            else
            {
                ClassicAssert.IsTrue(op.IsImpossible);
                ClassicAssert.False(op.Do());
            }

            ClassicAssert.AreEqual(expectedNewIndex, toMove.Ordinal);

            op.Undo();
            ClassicAssert.AreEqual(originalIndex, toMove.Ordinal);

            op.Redo();
            ClassicAssert.AreEqual(expectedNewIndex, toMove.Ordinal);

        }, out _);
    }
}
