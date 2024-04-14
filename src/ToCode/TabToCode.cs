using System.CodeDom;
using Terminal.Gui;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Handles generating code for a single <see cref="Tab"/> into .Designer.cs
/// file (See <see cref="CodeDomArgs"/>).  This will then be added to a <see cref="TabView"/>
/// via <see cref="TabView.AddTab(Tab, bool)"/>.
/// </summary>
public class TabToCode : ToCodeBase
{
    private readonly Design design;
    private readonly Tab tab;

    /// <summary>
    /// Initializes a new instance of the <see cref="TabToCode"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/>.</param>
    /// <param name="tab">A Tab within <paramref name="design"/> that you
    /// want added to the .Designer.cs file.</param>
    public TabToCode(Design design, Tab tab)
    {
        this.design = design;
        this.tab = tab;
    }

    /// <summary>
    /// Adds code that constructs and initializes a single Tab of a
    /// TabView and adds it to the TabView in .Designer.cs file.
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    public void ToCode(CodeDomArgs args)
    {
        var tabName = $"{this.design.FieldName}{this.GetTabFieldName(args)}";

        // add a field to the class for the Tab
        this.AddLocalFieldToMethod(args, typeof(Tab), tabName);

        // initialize the field by calling its constructor in InitializeComponent
        this.AddConstructorCall(
            args,
            tabName,
            typeof(Tab));

        this.AddPropertyAssignment(args, $"{tabName}.{nameof(Tab.DisplayText)}", this.tab.Text.ToCodePrimitiveExpression());
        this.AddPropertyAssignment(args, $"{tabName}.{nameof(Tab.View)}", new CodeSnippetExpression("new View()"));            

        // make the Tab.View Dim.Fill
        this.AddPropertyAssignment(args, $"{tabName}.View.Width", new CodeSnippetExpression("Dim.Fill()"));
        this.AddPropertyAssignment(args, $"{tabName}.View.Height", new CodeSnippetExpression("Dim.Fill()"));

        // create code statements for everything in the Tab (recursive)
        var viewToCode = new ViewToCode();
        viewToCode.AddSubViewsToDesignerCs(this.tab.View, args, new CodeSnippetExpression($"{tabName}.View"));

        // add the constructed tab to the TabView
        this.AddAddTabCall(tabName, args);
    }

    private string GetTabFieldName(CodeDomArgs args)
    {
        var tabname = this.tab.DisplayText?.ToString();
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
                new CodeSnippetExpression($"{this.design.FieldName}"), "AddTab"),
            new CodeSnippetExpression(tabFieldName),
            new CodeSnippetExpression("false"));

        args.InitMethod.Statements.Add(addColumnToTableStatement);
    }
}
