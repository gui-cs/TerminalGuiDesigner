using System.CodeDom;
using System.Data;

namespace TerminalGuiDesigner.ToCode;

public class DataTableToCode : ToCodeBase
{
    public Design Design { get; }
    public DataTable Table { get; }

    public DataTableToCode(Design design, DataTable table)
    {
        this.Design = design;
        this.Table = table;
    }

    internal void ToCode(CodeDomArgs args)
    {
        var dataTableFieldName = args.GetUniqueFieldName($"{this.Design.FieldName}Table");

        // add a field to the class for the Table that is in the view
        this.AddLocalFieldToMethod(args, typeof(DataTable), dataTableFieldName);

        this.AddConstructorCall(args, dataTableFieldName, typeof(DataTable));

        foreach (DataColumn col in this.Table.Columns)
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
        setLhs.FieldName = $"this.{this.Design.FieldName}.Table";

        var setRhs = new CodeFieldReferenceExpression();
        setRhs.FieldName = $"{tableFieldName}";

        var assignStatement = new CodeAssignStatement();
        assignStatement.Left = setLhs;
        assignStatement.Right = setRhs;
        args.InitMethod.Statements.Add(assignStatement);
    }
}
