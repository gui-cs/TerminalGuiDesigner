
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

        if (delete.Data is Design d)
        {
            // there are view(s) that depend on us (e.g. for positioning)
            // deleting us would go very badly
            if (d.GetDependantDesigns().Any())
                IsImpossible = true;
        }
    }

    public override bool Do()
    {
        if(from != null)
        {
            from.Remove(delete);
            return true;
        }

        return false;
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
