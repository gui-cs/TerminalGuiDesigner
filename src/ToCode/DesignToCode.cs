using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

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
            // if we have a snippet use that code instead
            if (Design.SnippetProperties.ContainsKey(prop.PropertyInfo))
            {
                Design.SnippetProperties[prop.PropertyInfo].ToCode(args);
            }
            else
            {
                prop.ToCode(args);
            }
        }


        // if the current component is a TableView we should persist the table too
        if (Design.View is TableView tv && tv.Table != null)
        {
            var designTable = new DataTableToCode(Design, tv.Table);
            designTable.ToCode(args);
        }

        AddAddToViewStatement(args, Design);
    }
}
