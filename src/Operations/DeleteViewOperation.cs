
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DeleteViewOperation : IOperation
{
    private readonly View delete;
    private readonly View from;

    public DeleteViewOperation(View delete)
    {
        this.delete = delete;
        this.from = delete.SuperView;
    }

    public void Do()
    {
        if(from != null)
        {
            from.Remove(delete);
        }
    }

    public void Redo()
    {
        Do();
    }

    public void Undo()
    {
        if(from != null)
        {
            from.Add(delete);
        }
    }
}
