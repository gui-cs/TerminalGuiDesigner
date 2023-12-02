using NUnit.Framework;
using System;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations.TableViewOperations;

namespace UnitTests.Operations;

internal class AddColumnOperationTests : Tests
{
    [Test]
    public void TestAddColumn_BadViewType()
    {
        var d = Get10By10View();
        var ex = ClassicAssert.Throws<ArgumentException>(() => new AddColumnOperation(d, null));

        ClassicAssert.AreEqual("Design must wrap a TableView to be used with this operation.", ex?.Message);
    }
    
    [Test]
    public void Test_AddColumnOperation_Do()
    {
        int colsBefore = 0;
        TableView? vBefore = null;

        RoundTrip<View, TableView>((d, v) =>
        {
            var dt = v.GetDataTable();

            vBefore = v;
            colsBefore = dt.Columns.Count;
            
            var op = new AddColumnOperation(d, "MyCol");
            op.Do();

            ClassicAssert.AreEqual(colsBefore + 1, dt.Columns.Count, "Expected AddColumnOperation to increase column count by 1");

        }, out var vAfter);

        ClassicAssert.AreEqual(colsBefore + 1, vAfter.GetDataTable().Columns.Count);
        ClassicAssert.AreEqual(colsBefore + 1, vBefore?.GetDataTable().Columns.Count);
    }

    [Test]
    public void Test_AddColumnOperation_UnDo()
    {
        int colsBefore = 0;
        TableView? vBefore = null;

        RoundTrip<View, TableView>((d, v) =>
        {
            var dt = v.GetDataTable();

            vBefore = v;
            colsBefore = dt.Columns.Count;

            var op = new AddColumnOperation(d, "MyCol");
            op.Do();
            ClassicAssert.AreEqual(colsBefore + 1, dt.Columns.Count, "Expected AddColumnOperation to increase column count by 1");

            op.Undo();
            ClassicAssert.AreEqual(colsBefore, dt.Columns.Count, "Expected AddColumnOperation Undo to go back to original count");

        }, out var vAfter);

        ClassicAssert.AreEqual(colsBefore, vAfter.GetDataTable().Columns.Count);
        ClassicAssert.AreEqual(colsBefore, vBefore?.GetDataTable().Columns.Count);
    }

    [Test]
    public void Test_AddColumnOperation_DuplicateColumnName()
    {
        int colsBefore = 0;
        TableView? vBefore = null;

        RoundTrip<View, TableView>((d, v) =>
        {
            vBefore = v;
            var dt = v.GetDataTable();

            colsBefore = dt.Columns.Count;

            // TableView comes with some free columns.  Try using that column name again
            dt.Columns[0].ColumnName = "Test";
            var op = new AddColumnOperation(d, "Test");
            op.Do();

            ClassicAssert.AreEqual("Test2", dt.Columns[colsBefore].ColumnName);
            ClassicAssert.AreEqual(colsBefore + 1, dt.Columns.Count, "Expected AddColumnOperation to increase column count by 1");

        }, out var vAfter);

        ClassicAssert.AreEqual(colsBefore+1, vAfter.GetDataTable().Columns.Count);
        ClassicAssert.AreEqual(colsBefore+1, vBefore?.GetDataTable().Columns.Count);
    }
}
