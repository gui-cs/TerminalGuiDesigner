namespace TerminalGuiDesigner.Operations;

public class CopyOperation : Operation
{
    public static Design[]? LastCopiedDesign {get; private set;}
    private Design[] _toCopy;

    public CopyOperation(Design? single, MultiSelectionManager multi)
    {
        if(multi.Selected.Any())
        {
            _toCopy = multi.Selected.ToArray();
        }
        else if(single != null)
        {
            _toCopy = new []{single};
        }
        else
        {
            _toCopy = new Design[0];
            IsImpossible = true;
            return;
        }

        SupportsUndo = false;

        // cannot copy a view if it is orphaned or root
        if(_toCopy.Any(c=>c.View.SuperView == null || c.IsRoot))
            IsImpossible = true;
    }

    public override string ToString()
    {
        if(_toCopy.Length > 1)
            return $"Copy {_toCopy.Length} Items";

        return _toCopy.Length > 0 ? $"Copy {_toCopy[0]}" : "Copy";
    }

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
