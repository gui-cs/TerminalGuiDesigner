using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class AddViewOperation : IOperation
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
    public void Do()
    {
        add.Data = to.CreateSubControlDesign(sourceCode,fieldName, add);
        to.View.Add(add);
    }
    public void Redo()
    {
        to.View.Add(add);
    }

    public void Undo()
    {
        to.View.Remove(add);
    }
}