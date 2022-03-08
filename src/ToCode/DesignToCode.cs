using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using static Terminal.Gui.TableView;

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

            AddFieldToClass(args, Design);
            AddConstructorCall(args, Design);

            foreach (var prop in Design.GetDesignableProperties())
            {
                var val = Design.GetDesignablePropertyValue(prop);

                // if the property being designed exists on the 
                // View directly and is not a subproperty e.g. MyView.Border.X
                if(prop.DeclaringType.IsAssignableFrom(Design.View.GetType()))
                {
                    AddPropertyAssignment(args,Design, prop.Name, val);
                }

                if(prop.DeclaringType == typeof(TableStyle))
                {
                    AddPropertyAssignment(args, Design, $"{nameof(TableView.Style)}.{prop.Name}", val);
                }
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
            AddPropertyAssignment(args, Design, nameof(View.Data), Design.FieldName);

            AddAddToViewStatement(args, Design);
        }
    }
}
