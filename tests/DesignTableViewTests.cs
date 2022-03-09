using NUnit.Framework;
using System.IO;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;

public class TableViewTests
{
    [Test]
    public void TestRoundTrip_PreserveColumns()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PreserveColumns.cs");
        var designOut = viewToCode.GenerateNewWindow(file, "YourNamespace", out var sourceCode);

        var factory = new ViewFactory();
        var tvOut = factory.Create(typeof(TableView));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, tvOut, designOut, "myTable"));

        viewToCode.GenerateDesignerCs(designOut.View, sourceCode);

        var tableOut = designOut.View.GetActualSubviews().OfType<TableView>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tableIn = designBackIn.View.GetActualSubviews().OfType<TableView>().Single();

        Assert.IsNotNull(tableIn.Table);

        Assert.AreEqual(tableOut.Table.Columns.Count, tableIn.Table.Columns.Count);
        Assert.AreEqual(tableOut.Table.Rows.Count, tableIn.Table.Rows.Count);
    }
}
