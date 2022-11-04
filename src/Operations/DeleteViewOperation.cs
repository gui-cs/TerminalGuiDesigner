using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DeleteViewOperation : Operation
{
    private readonly View[] delete;
    private readonly View[] from;
    private readonly Design[] originalSelection;

    public DeleteViewOperation(params View[] delete)
    {
        this.delete = delete;
        this.from = delete.Select(d => d.SuperView).ToArray();

        this.originalSelection = SelectionManager.Instance.Selected.ToArray();

        foreach (var del in delete)
        {
            if (del.Data is Design design)
            {
                // don't delete the root view!
                if (design.IsRoot)
                {
                    this.IsImpossible = true;
                }

                // there are view(s) that depend on us (e.g. for positioning)
                // deleting us would go very badly
                if (design.GetDependantDesigns()
                        // unless we are also deleting those too in which case its fine
                        .Any(dep => !delete.Contains(dep.View)))
                {
                    this.IsImpossible = true;
                }
            }
        }
    }

    public override bool Do()
    {
        bool removedAny = false;

        for (int i = 0; i < this.delete.Length; i++)
        {
            if (this.from[i] != null)
            {
                this.from[i].Remove(this.delete[i]);
                removedAny = true;
            }
        }

        this.ForceSelectionClear();

        return removedAny;
    }

    public override void Redo()
    {
        this.Do();
    }

    public override void Undo()
    {
        for (int i = 0; i < this.delete.Length; i++)
        {
            if (this.from[i] != null)
            {
                this.from[i].Add(this.delete[i]);
            }
        }

        this.ForceSelection(this.originalSelection);
    }

    public override string ToString()
    {
        return "Delete";
    }
}
