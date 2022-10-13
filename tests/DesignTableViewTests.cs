using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;


public class TableViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveColumns()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PreserveColumns.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Window), out var sourceCode);

        var factory = new ViewFactory();
        var tvOut = factory.Create(typeof(TableView));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, tvOut, designOut, "myTable"));

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Window));

        var tableOut = designOut.View.GetActualSubviews().OfType<TableView>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tableIn = designBackIn.View.GetActualSubviews().OfType<TableView>().Single();

        Assert.IsNotNull(tableIn.Table);

        Assert.AreEqual(tableOut.Table.Columns.Count, tableIn.Table.Columns.Count);
        Assert.AreEqual(tableOut.Table.Rows.Count, tableIn.Table.Rows.Count);
    }

    [Test]
    public void TestRoundTrip_TwoTablesWithDuplicatedColumns()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_TwoTablesWithDuplicatedColumns.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Window), out var sourceCode);

        var factory = new ViewFactory();
        var tvOut1 = factory.Create(typeof(TableView));
        var tvOut2 = factory.Create(typeof(TableView));

        // The column names exactly match 
        OperationManager.Instance.Do(new AddViewOperation(sourceCode, tvOut1, designOut, "myTable1"));
        OperationManager.Instance.Do(new AddViewOperation(sourceCode, tvOut2, designOut, "myTable2"));

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Window));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        Assert.AreEqual(2,designBackIn.View.GetActualSubviews().OfType<TableView>().Count());
    }
}
