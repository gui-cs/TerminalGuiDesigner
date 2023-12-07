using System.Data;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests;

internal class TableViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveColumns()
    {
        TableView tableIn = RoundTrip<Window, TableView>(

            (d, v) => ClassicAssert.IsNotEmpty(v.GetDataTable().Columns, "Default ViewFactory should create some columns for new TableViews"),
            out TableView tableOut);

        ClassicAssert.IsNotNull(tableIn.Table);

        ClassicAssert.AreEqual(tableOut.Table.Columns, tableIn.Table.Columns);
        ClassicAssert.AreEqual(tableOut.Table.Rows, tableIn.Table.Rows);
    }

    [Test]
    public void TestRoundTrip_TwoTablesWithDuplicatedColumns()
    {
        // Create a TableView
        TableView tableIn = RoundTrip<Window, TableView>(
            (d, v) =>
            {
                // create a second TableView also on the root
                TableView tvOut2 = ViewFactory.Create<TableView>( );
                OperationManager.Instance.Do(new AddViewOperation(tvOut2, d.GetRootDesign(), "myTable2"));
            },
            out TableView tableOut);

        // Views should collide on column name but still compile
        var designBackIn = ((Design)tableIn.Data).GetRootDesign();
        var tables = designBackIn.View.GetActualSubviews().OfType<TableView>().ToArray();

        ClassicAssert.AreEqual(2, tables.Length);

        ClassicAssert.That(
            tables[0].GetDataTable().Columns.Cast<DataColumn>().Select(c => c.ColumnName),
            Is.EquivalentTo(
                tables[1].GetDataTable().Columns.Cast<DataColumn>().Select(c => c.ColumnName)),
            "Expected both TableViews to have columns with the same names");
    }
}
