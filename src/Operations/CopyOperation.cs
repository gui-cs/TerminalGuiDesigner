namespace TerminalGuiDesigner.Operations;

public class CopyOperation : Operation
{
    public static Design? LastCopiedDesign {get; private set;}
    private Design _toCopy;

    public CopyOperation(Design toCopy)
    {
        _toCopy = toCopy;
        SupportsUndo = false;

        // cannot copy a view if it is orphaned
        if(toCopy.View.SuperView == null)
            IsImpossible = true;
    }

    // TODO: override ToString to indicate what they are actually copying

    public override bool Do()
    {
        LastCopiedDesign = _toCopy;
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
