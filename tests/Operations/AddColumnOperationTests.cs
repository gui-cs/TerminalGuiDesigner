using NUnit.Framework;
using System;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.TableViewOperations;

namespace UnitTests.Operations;

internal class AddColumnOperationTests : Tests
{
    [Test]
    public void TestAddColumn_BadViewType()
    {
        var d = Get10By10View();
        var ex = Assert.Throws<ArgumentException>(() => new AddColumnOperation(d, null));

        Assert.AreEqual("Design must be for a TableView to support AddColumnOperation", ex?.Message);
    }
    
    [Test]
    public void Test_AddColumnOperation_Do()
    {
        int colsBefore = 0;
        TableView? vBefore = null;

        RoundTrip<View, TableView>((d, v) =>
        {
            vBefore = v;
            colsBefore = v.Table.Columns.Count;
            
            var op = new AddColumnOperation(d, "MyCol");
            op.Do();

            Assert.AreEqual(colsBefore + 1, v.Table.Columns.Count, "Expected AddColumnOperation to increase column count by 1");

        }, out var vAfter);

        Assert.AreEqual(colsBefore + 1, vAfter.Table.Columns.Count);
        Assert.AreEqual(colsBefore + 1, vBefore?.Table.Columns.Count);
    }

    [Test]
    public void Test_AddColumnOperation_UnDo()
    {
        int colsBefore = 0;
        TableView? vBefore = null;

        RoundTrip<View, TableView>((d, v) =>
        {
            vBefore = v;
            colsBefore = v.Table.Columns.Count;

            var op = new AddColumnOperation(d, "MyCol");
            op.Do();
            Assert.AreEqual(colsBefore + 1, v.Table.Columns.Count, "Expected AddColumnOperation to increase column count by 1");

            op.Undo();
            Assert.AreEqual(colsBefore, v.Table.Columns.Count, "Expected AddColumnOperation Undo to go back to original count");

        }, out var vAfter);

        Assert.AreEqual(colsBefore, vAfter.Table.Columns.Count);
        Assert.AreEqual(colsBefore, vBefore?.Table.Columns.Count);
    }

    [Test]
    public void Test_AddColumnOperation_DuplicateColumnName()
    {
        int colsBefore = 0;
        TableView? vBefore = null;

        RoundTrip<View, TableView>((d, v) =>
        {
            vBefore = v;
            colsBefore = v.Table.Columns.Count;

            // TableView comes with some free columns.  Try using that column name again
            v.Table.Columns[0].ColumnName = "Test";
            var op = new AddColumnOperation(d, "Test");
            op.Do();

            Assert.AreEqual("Test2", v.Table.Columns[colsBefore].ColumnName);
            Assert.AreEqual(colsBefore + 1, v.Table.Columns.Count, "Expected AddColumnOperation to increase column count by 1");

        }, out var vAfter);

        Assert.AreEqual(colsBefore+1, vAfter.Table.Columns.Count);
        Assert.AreEqual(colsBefore+1, vBefore?.Table.Columns.Count);
    }
}
