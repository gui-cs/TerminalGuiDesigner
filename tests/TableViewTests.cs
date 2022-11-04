using System.Data;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace tests;

public class TableViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveColumns()
    {
        TableView tableIn = this.RoundTrip<Window, TableView>(
            (d, v) => Assert.IsNotEmpty(v.Table.Columns, "Default ViewFactory should create some columns for new TableViews"),
            out TableView tableOut);

        Assert.IsNotNull(tableIn.Table);

        Assert.AreEqual(tableOut.Table.Columns.Count, tableIn.Table.Columns.Count);
        Assert.AreEqual(tableOut.Table.Rows.Count, tableIn.Table.Rows.Count);
    }

    [Test]
    public void TestRoundTrip_TwoTablesWithDuplicatedColumns()
    {
        // Create a TableView
        TableView tableIn = this.RoundTrip<Window, TableView>(
            (d, v) =>
            {
                // create a second TableView also on the root
                var factory = new ViewFactory();
                var tvOut2 = factory.Create(typeof(TableView));
                OperationManager.Instance.Do(new AddViewOperation(tvOut2, d.GetRootDesign(), "myTable2"));
            },
        out TableView tableOut);

        // Views should collide on column name but still compile
        var designBackIn = ((Design)tableIn.Data).GetRootDesign();
        var tables = designBackIn.View.GetActualSubviews().OfType<TableView>().ToArray();

        Assert.AreEqual(2, tables.Length);

        Assert.That(
            tables[0].Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName),
            Is.EquivalentTo(
                tables[1].Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)),
            "Expected both TableViews to have columns with the same names");
    }
}
