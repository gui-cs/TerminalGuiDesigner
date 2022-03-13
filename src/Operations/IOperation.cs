namespace TerminalGuiDesigner.Operations;

public interface IOperation
{
    bool IsImpossible { get; }

    void Do();

    void Undo();

    void Redo();

}
