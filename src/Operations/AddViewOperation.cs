using Terminal.Gui;
using TerminalGuiDesigner.UI;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

public class AddViewOperation : Operation
{
    private readonly SourceCodeFile sourceCode;
    private View? add;
    private string? fieldName;
    private readonly Design to;

    public AddViewOperation(SourceCodeFile sourceCode,View add, Design to,string fieldName)
    {
        this.sourceCode = sourceCode;
        this.add = add;
        this.fieldName = fieldName;
        this.to = to;
    }

    /// <summary>
    /// Constructor that asks users what view they want at runtime
    /// </summary>
    public AddViewOperation(SourceCodeFile sourceCode, Design design)
    {
        this.sourceCode = sourceCode;
        to = design;
    }

    public override bool Do()
    {
        if(add == null)
        {                
            var factory = new ViewFactory();
            var selectable = factory.GetSupportedViews().ToArray();
            
            if (Modals.Get("Type of Control", "Add", true, selectable, t => t?.Name ?? "Null", false, out var selected) && selected != null)
            {
                add = factory.Create(selected);
                fieldName = to.GetUniqueFieldName(selected);
            }   
        }

        // user cancelled picking a type
        if(add == null || string.IsNullOrWhiteSpace(fieldName))
            return false;
        
        add.Data = to.CreateSubControlDesign(sourceCode,fieldName, add);

        var v = GetViewToAddTo();
        v.Add(add);

        if(Application.Driver != null){
            add.SetFocus();
        }

        v.SetNeedsDisplay();
        return true;
    }

    private View GetViewToAddTo()
    {
        if(to.View is TabView tabView)
        {
            return tabView.SelectedTab.View;
        }

        return to.View;
    }

    public override void Redo()
    {
        if(add == null)
        {
            return;
        }

        var v = GetViewToAddTo();
        v.Add(add);
        v.SetNeedsDisplay();
    }

    public override void Undo()
    {
        if(add == null)
        {
            return;
        }
        var v = GetViewToAddTo();
        v.Remove(add);
        v.SetNeedsDisplay();
    }
}