namespace TerminalGuiDesigner.Operations;

public class RenameViewOperation : Operation
{
    public Design Design { get; }
    public string OldName { get; }
    public string NewName { get; }

    public RenameViewOperation(Design design, string oldName, string newName)
    {
        Design = design;
        OldName = oldName;
        NewName = newName;
    }

    public override bool Do()
    {
        Design.FieldName = NewName;
        return true;
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        Design.FieldName = OldName;
    }
}
