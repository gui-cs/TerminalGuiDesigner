using System.CodeDom;
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
        var tabName = $"{Design.FieldName}{GetTabFieldName(args)}";

        // add a field to the class for the Tab
        AddLocalFieldToMethod(args, typeof(Tab), tabName);

        // initialize the field by calling its constructor in InitializeComponent
        AddConstructorCall(args, tabName, typeof(Tab),
            new CodePrimitiveExpression(Tab.Text.ToPrimitive()),
            new CodeSnippetExpression("new View()"));

        // make the Tab.View Dim.Fill
        AddPropertyAssignment(args, $"{tabName}.View.Width", new CodeSnippetExpression("Dim.Fill()"));
        AddPropertyAssignment(args, $"{tabName}.View.Height", new CodeSnippetExpression("Dim.Fill()"));

        // create code statements for everything in the Tab (recursive)
        var viewToCode = new ViewToCode();
        viewToCode.AddSubViewsToDesignerCs(Tab.View, args, new CodeSnippetExpression($"{tabName}.View"));

        // add the constructed tab to the TabView
        AddAddTabCall(tabName, args);
    }

    private string GetTabFieldName(CodeDomArgs args)
    {
        var tabname = Tab.Text?.ToString();
        if (string.IsNullOrWhiteSpace(tabname))
        {
            throw new Exception("Could not generate Tab variable name because its Text was blank or null");
        }

        return args.GetUniqueFieldName(tabname);
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
