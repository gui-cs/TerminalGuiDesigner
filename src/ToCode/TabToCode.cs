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


    /// <summary>
    /// Adds code that constructs and initializes a single Tab of a 
    /// TabView and adds it to the TabView
    /// </summary>
    internal void ToCode(CodeDomArgs args)
    {
        var tabName = $"{Design.FieldName}{GetTabFieldName()}";

        // add a field to the class for the Tab
        AddLocalFieldToMethod(args, typeof(Tab), tabName);

        // initialize the field by calling its constructor in InitializeComponent
        AddConstructorCall(tabName, typeof(Tab), args,
            new CodePrimitiveExpression(Tab.Text.ToPrimitive()),
            new CodeSnippetExpression("new View()"));

        // make the Tab.View Dim.Fill
        AddPropertyAssignment(args,$"{tabName}.View.Width",new CodeSnippetExpression("Dim.Fill()"));
        AddPropertyAssignment(args,$"{tabName}.View.Height",new CodeSnippetExpression("Dim.Fill()"));

        // for each thing that is shown in the tab
        foreach(var v in Tab.View.Subviews)
        {
            // that is designable
            if(v.Data is Design d)
            {
                var toCode = new DesignToCode(d);
                toCode.ToCode(args,new CodeSnippetExpression($"{tabName}.View"));
            }
        }

        // add the constructed tab to the TabView
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
