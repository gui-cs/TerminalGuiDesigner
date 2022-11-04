namespace TerminalGuiDesigner.Operations;

public class RenameViewOperation : Operation
{
    public Design Design { get; }
    public string OldName { get; }
    public string NewName { get; }

    public RenameViewOperation(Design design, string oldName, string newName)
    {
        this.Design = design;
        this.OldName = oldName;
        this.NewName = newName;
    }

    public override bool Do()
    {
        this.Design.FieldName = this.NewName;
        return true;
    }

    public override void Redo()
    {
        this.Do();
    }

    public override void Undo()
    {
        this.Design.FieldName = this.OldName;
    }
}
