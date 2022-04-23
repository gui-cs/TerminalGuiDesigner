namespace TerminalGuiDesigner.Operations;

public interface IOperation
{
    bool IsImpossible { get; }

    bool Do();

    void Undo();

    void Redo();

}
