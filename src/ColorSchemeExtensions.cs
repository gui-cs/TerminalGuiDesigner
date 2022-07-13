using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class ColorSchemeExtensions
{
    public static bool AreEqual(this ColorScheme a, ColorScheme b)
    {
        return
            a.Normal.Value == b.Normal.Value &&
            a.HotNormal.Value == b.HotNormal.Value &&
            a.Focus.Value == b.Focus.Value &&
            a.HotFocus.Value == b.HotFocus.Value &&
            a.Disabled.Value == b.Disabled.Value;
    }
}
