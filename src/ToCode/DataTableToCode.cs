using System.CodeDom;
using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Handles generating code for a <see cref="System.Data.DataTable"/> into .Designer.cs
/// file (See <see cref="CodeDomArgs"/>).  This will then be assigned to the
/// <see cref="TableView.Table"/> property of a <see cref="TableView"/>.
/// </summary>
public class DataTableToCode : ToCodeBase
{
    private readonly DataTable table;
    private readonly Design design;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTableToCode"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TableView"/>.</param>
    public DataTableToCode(Design design)
    {
        this.design = design;
        if (design.View is not TableView tv)
        {
            throw new ArgumentException(nameof(design), $"{nameof(DataTableToCode)} can only be used with {nameof(TerminalGuiDesigner.Design)} that wrap {nameof(TableView)}");
        }

        this.table = tv.Table;
    }

    /// <summary>
    /// Adds CodeDOM statements to .Designer.cs file (described by <paramref name="args"/>)
    /// to construct, initialize and assign <see cref="TableView.Table"/>.
    /// </summary>
    /// <param name="args">State object for the .Designer.cs file being generated.</param>
    internal void ToCode(CodeDomArgs args)
    {
        var dataTableFieldName = args.GetUniqueFieldName($"{this.design.FieldName}Table");

        // add a field to the class for the Table that is in the view
        this.AddLocalFieldToMethod(args, typeof(DataTable), dataTableFieldName);

        this.AddConstructorCall(args, dataTableFieldName, typeof(DataTable));

        foreach (DataColumn col in this.table.Columns)
        {
            var colFieldName = args.GetUniqueFieldName(dataTableFieldName + col.ColumnName);

            this.AddLocalFieldToMethod(args, typeof(DataColumn), colFieldName);
            this.AddConstructorCall(args, colFieldName, typeof(DataColumn));

            this.AddPropertyAssignment(args, $"{colFieldName}.{nameof(DataColumn.ColumnName)}", col.ColumnName);

            this.AddTableColumnsAddCall(dataTableFieldName, colFieldName, args);
        }

        this.AddSetTableViewTableProperty(args, dataTableFieldName);
    }

    private void AddTableColumnsAddCall(string tableFieldName, string columnFieldName, CodeDomArgs args)
    {
        // Construct it
        var addColumnToTableStatement = new CodeMethodInvokeExpression(
            new CodeMethodReferenceExpression(
                new CodeSnippetExpression($"{tableFieldName}.Columns"), "Add"),
            new CodeSnippetExpression(columnFieldName));

        args.InitMethod.Statements.Add(addColumnToTableStatement);
    }

    private void AddSetTableViewTableProperty(CodeDomArgs args, string tableFieldName)
    {
        var setLhs = new CodeFieldReferenceExpression();
        setLhs.FieldName = $"this.{this.design.FieldName}.Table";

        var setRhs = new CodeFieldReferenceExpression();
        setRhs.FieldName = $"{tableFieldName}";

        var assignStatement = new CodeAssignStatement();
        assignStatement.Left = setLhs;
        assignStatement.Right = setRhs;
        args.InitMethod.Statements.Add(assignStatement);
    }
}
