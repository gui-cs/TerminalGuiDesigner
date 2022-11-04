namespace TerminalGuiDesigner.Operations;

public class CopyOperation : Operation
{
    public static Design[]? LastCopiedDesign { get; private set; }

    private Design[] toCopy;

    public CopyOperation(params Design[] toCopy)
    {
        if (toCopy.Any())
        {
            this.toCopy = toCopy;
        }
        else
        {
            this.toCopy = new Design[0];
            this.IsImpossible = true;
            return;
        }

        this.SupportsUndo = false;

        // cannot copy a view if it is orphaned or root
        if (this.toCopy.Any(c => c.View.SuperView == null || c.IsRoot))
        {
            this.IsImpossible = true;
        }
    }

    public override string ToString()
    {
        if (this.toCopy.Length > 1)
        {
            return $"Copy {this.toCopy.Length} Items";
        }

        return this.toCopy.Length > 0 ? $"Copy {this.toCopy[0]}" : "Copy";
    }

    public override bool Do()
    {
        LastCopiedDesign = this.toCopy;
        return true;
    }

    public override void Undo()
    {
        throw new NotSupportedException();
    }

    public override void Redo()
    {
        throw new NotSupportedException();
    }
}
