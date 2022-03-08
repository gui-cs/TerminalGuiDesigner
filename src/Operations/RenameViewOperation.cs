using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.Operations
{
    public class RenameViewOperation : IOperation
    {
        public Design Design { get; }
        public string OldName { get; }
        public string NewName { get; }

        public RenameViewOperation(Design design,string oldName,string newName)
        {
            Design = design;
            OldName = oldName;
            NewName = newName;
        }

        public void Do()
        {
            Design.FieldName = NewName;
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            Design.FieldName = OldName;
        }
    }
}