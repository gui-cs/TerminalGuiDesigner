using System.CodeDom;
using System.Text.RegularExpressions;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.ToCode;

public class TabToCode : ToCodeBase
{

    public Design Design { get; }
    public Tab Tab { get; }

    public TabToCode(Design design, Tab tab)
    {
        Design = design;
        Tab = tab;
    }


    internal void ToCode(CodeDomArgs args)
    {
        var tabName = $"{Design.FieldName}{GetTabFieldName()}";

        // add a field to the class for the Table that is in the view
        AddLocalFieldToMethod(args, typeof(Tab), tabName);

        AddConstructorCall(tabName, typeof(Tab), args);

        AddPropertyAssignment(args,$"{tabName}.Text",Tab.Text);

        // TODO : set the View that is hosted by the Tab

        AddAddTabCall(tabName,args);
    }

    private string GetTabFieldName()
    {
        return Regex.Replace(Tab.Text.ToString() 
            ?? throw new Exception("Could not generate Tab variable name because its Text was blank or null"),
            @"\W","");
    }

    private void AddAddTabCall(string tabFieldName, CodeDomArgs args)
    {
        // Construct it
        var addColumnToTableStatement = new CodeMethodInvokeExpression(
            new CodeMethodReferenceExpression(
                new CodeSnippetExpression($"{Design.FieldName}"), "AddTab"),
                new CodeSnippetExpression(tabFieldName),
                new CodeSnippetExpression("false"));

        args.InitMethod.Statements.Add(addColumnToTableStatement);
    }
}
