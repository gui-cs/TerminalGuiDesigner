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

        internal void ToCode(CodeTypeDeclaration addTo, CodeMemberMethod initMethod)
        {

            AddFieldToClass(Design,addTo);
            AddConstructorCall(Design, initMethod);

            foreach (var prop in Design.GetDesignableProperties())
            {
                var val = Design.GetDesignablePropertyValue(prop);
                AddPropertyAssignment(Design, initMethod, prop.Name, val);
            }

            if (Design.View is TableView tv)
            {
                AddFieldToClass(addTo, $"{Design.FieldName}Table", typeof(DataTable));
            }

            // Set View.Data to the name of the field so that we can 
            // determine later on which View instances come from which
            // Fields in the class
            AddPropertyAssignment(Design,initMethod, nameof(View.Data), Design.FieldName);

            AddAddToViewStatement(Design, initMethod);
        }
    }
}
