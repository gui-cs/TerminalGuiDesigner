namespace TerminalGuiDesigner.Operations;

public interface IOperation
{
    void Do();

    void Undo();

    void Redo();

}
