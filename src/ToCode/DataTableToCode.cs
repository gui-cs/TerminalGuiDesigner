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
            // add a field to the class for the Table that is in the view
            AddFieldToClass(args, $"{Design.FieldName}Table", typeof(DataTable));
        }
    }
}
