#nullable disable
namespace TerminalGuiDesigner.UI.Windows;

public interface IValueGetterDialog
{
    public object? ActualResult { get; }
    public bool Cancelled { get; }
}