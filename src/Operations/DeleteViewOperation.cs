using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// <see cref="Operation"/> that removes one or more <see cref="Design"/> from their
/// current host containing <see cref="View"/> (effectively deleting them).
/// </summary>
public class DeleteViewOperation : Operation
{
    private readonly Design[] delete;
    private readonly View[] from;
    private readonly Design[] originalSelection;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteViewOperation"/> class.
    /// </summary>
    /// <param name="delete">Wrappers for the <see cref="View"/> you want to delete.</param>
    public DeleteViewOperation(params Design[] delete)
    {
        this.delete = delete;
        this.from = delete.Select(d => d.View.SuperView).ToArray();

        this.originalSelection = SelectionManager.Instance.Selected.ToArray();

        foreach (var design in delete)
        {
            // don't delete the root view!
            if (design.IsRoot)
            {
                this.IsImpossible = true;
            }

            // there are view(s) that depend on us (e.g. for positioning)
            // that are not also being deleted themselves
            if (design.GetDependantDesigns().Any(dep => !delete.Contains(dep)))
            {
                // Prevent deleting - so there are no orphan references from existing Views e.g. Pos.Right(thingIJustDeleted);
                this.IsImpossible = true;
            }
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        bool removedAny = false;

        for (int i = 0; i < this.delete.Length; i++)
        {
            if (this.from[i] != null)
            {
                this.from[i].Remove(this.delete[i].View);
                removedAny = true;
            }
        }

        this.ForceSelectionClear();

        return removedAny;
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        for (int i = 0; i < this.delete.Length; i++)
        {
            if (this.from[i] != null)
            {
                this.from[i].Add(this.delete[i].View);
            }
        }

        this.ForceSelection(this.originalSelection);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Delete";
    }
}
