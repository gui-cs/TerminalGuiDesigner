
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DeleteViewOperation : Operation
{
    private readonly View delete;
    private readonly View from;

    public DeleteViewOperation(View delete)
    {
        this.delete = delete;
        this.from = delete.SuperView;
    }

    public override void Do()
    {
        if(from != null)
        {
            from.Remove(delete);
        }
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        if(from != null)
        {
            from.Add(delete);
        }
    }
}