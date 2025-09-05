namespace TerminalGuiDesigner.UI;

public interface IErrorReporter
{
    public void ShowErrorThatViewIsUsedByOthers(Design[] usedBy);
}