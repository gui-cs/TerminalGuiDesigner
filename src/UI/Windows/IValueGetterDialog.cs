#nullable disable
namespace TerminalGuiDesigner.UI.Windows;

public interface IValueGetterDialog
{
    public object? Result { get; }
    public bool Cancelled { get; }
}