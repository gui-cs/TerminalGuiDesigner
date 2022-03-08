using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode
{
    internal class DesignToCode : ToCodeBase
    {
        public DesignToCode(Design design)
        {
            Design = design;
        }

        public Design Design { get; }

        internal void ToCode(CodeDomArgs args)
        {

            AddFieldToClass(Design,args);
            AddConstructorCall(Design, args);

            foreach (var prop in Design.GetDesignableProperties())
            {
                var val = Design.GetDesignablePropertyValue(prop);
                AddPropertyAssignment(Design, args, prop.Name, val);
            }

            
            // if the current component is a TableView we should persist the table too
            if (Design.View is TableView tv && tv.Table != null)
            {
                var designTable = new DataTableToCode(Design,tv.Table);
                designTable.ToCode(args);
            }

            // Set View.Data to the name of the field so that we can 
            // determine later on which View instances come from which
            // Fields in the class
            AddPropertyAssignment(Design,args, nameof(View.Data), Design.FieldName);

            AddAddToViewStatement(Design, args);
        }
    }
}
