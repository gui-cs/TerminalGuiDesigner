using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGuiDesigner.ToCode
{
    public class DataTableToCode : ToCodeBase
    {
        public Design Design { get; }
        public DataTable Table { get; }

        public DataTableToCode(Design design, DataTable table)
        {
            Design = design;
            Table = table;
        }

        internal void ToCode(CodeDomArgs args)
        {
            var dataTableFieldName = $"{Design.FieldName}Table";

            // add a field to the class for the Table that is in the view
            AddLocalFieldToMethod(args, typeof(DataTable), dataTableFieldName);

            AddConstructorCall(dataTableFieldName, typeof(DataTable), args);

            foreach (DataColumn col in Table.Columns)
            {
                var colFieldName = dataTableFieldName +"Col"+ col.Ordinal;

                AddLocalFieldToMethod(args, typeof(DataColumn), colFieldName);
                AddConstructorCall(colFieldName, typeof(DataColumn), args);

                AddPropertyAssignment(args,$"{colFieldName}.{nameof(DataColumn.ColumnName)}" , col.ColumnName);

                AddTableColumnsAddCall(dataTableFieldName,colFieldName,args);
            }

            AddSetTableViewTableProperty(args, dataTableFieldName);
        }

        private void AddTableColumnsAddCall(string tableFieldName, string columnFieldName,CodeDomArgs args)
        {
            // Construct it
            var addColumnToTableStatement  = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeSnippetExpression($"{tableFieldName}.Columns"), "Add"),
                    new CodeSnippetExpression(columnFieldName));

            args.InitMethod.Statements.Add(addColumnToTableStatement);
        }

        private void AddSetTableViewTableProperty(CodeDomArgs args, string tableFieldName)
        {

            var setLhs = new CodeFieldReferenceExpression();
            setLhs.FieldName = $"this.{Design.FieldName}.Table";

            var setRhs = new CodeFieldReferenceExpression();
            setRhs.FieldName = $"{tableFieldName}";

            var assignStatement = new CodeAssignStatement();
            assignStatement.Left = setLhs;
            assignStatement.Right = setRhs;
            args.InitMethod.Statements.Add(assignStatement);
        }
    }
}
