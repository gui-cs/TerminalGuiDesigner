using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

internal class DesignToCode : ToCodeBase
{
    public DesignToCode(Design design)
    {
        Design = design;
    }

    public Design Design { get; }

    internal void ToCode(CodeDomArgs args, CodeExpression parentView)
    {

        AddFieldToClass(args, Design);
        AddConstructorCall(args, Design);
    
        // Add border if we have one
        if(Design.View.Border != null)
            AddBorderConstruct(args, Design);

        foreach (var prop in Design.GetDesignableProperties())
        {
            prop.ToCode(args);
        }


        // if the current component is a TableView we should persist the table too
        if (Design.View is TableView tv && tv.Table != null)
        {
            var designTable = new DataTableToCode(Design, tv.Table);
            designTable.ToCode(args);
        }

        if(Design.View is TabView tabView)
        {
            foreach(var tab in tabView.Tabs)
            {
                var designTab = new TabToCode(Design,tab);
                designTab.ToCode(args);
            }
        }

        // call this.Add(someView)
        AddAddToViewStatement(args, Design, parentView);
    }

    private void AddBorderConstruct(CodeDomArgs args, Design design)
    {
        AddConstructorCall($"{design.FieldName}.{nameof(View.Border)}", typeof(Border), args);
        AddPropertyAssignment(args, 
            $"{design.FieldName}.{nameof(View.Border)}.{nameof(Border.Child)}",
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), design.FieldName));
    }
}
