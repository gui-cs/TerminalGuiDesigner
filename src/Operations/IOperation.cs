namespace TerminalGuiDesigner.Operations;

public interface IOperation
{
    Guid UniqueIdentifier { get; }

    bool IsImpossible { get; }

    bool SupportsUndo { get; }

    bool Do();

    void Undo();

    void Redo();
}
