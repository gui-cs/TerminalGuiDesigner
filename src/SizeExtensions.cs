using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class SizeExtensions
{
    public static string ToCode(this Size s)
    {
        return $"new Size({s.Width},{s.Height})";
    }
}
