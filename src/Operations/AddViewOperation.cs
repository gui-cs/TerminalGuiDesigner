using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class AddViewOperation : Operation
{
    private readonly SourceCodeFile sourceCode;
    private readonly View add;
    private readonly string fieldName;
    private readonly Design to;

    public AddViewOperation(SourceCodeFile sourceCode,View add, Design to,string fieldName)
    {
        this.sourceCode = sourceCode;
        this.add = add;
        this.fieldName = fieldName;
        this.to = to;
    }
    public override void Do()
    {
        add.Data = to.CreateSubControlDesign(sourceCode,fieldName, add);

        var v = GetViewToAddTo();
        v.Add(add);

        if(Application.Driver != null){
            add.SetFocus();
        }

        v.SetNeedsDisplay();
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
        var v = GetViewToAddTo();
        v.Add(add);
        v.SetNeedsDisplay();
    }

    public override void Undo()
    {
        var v = GetViewToAddTo();
        v.Remove(add);
        v.SetNeedsDisplay();
    }
}