using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for the <see cref="ColorScheme"/> class.
/// </summary>
public static class ColorSchemeExtensions
{
    /// <summary>
    /// Compares two <see cref="ColorScheme"/> by value (not reference).
    /// </summary>
    /// <param name="a">First to compare.</param>
    /// <param name="b">To compare with.</param>
    /// <returns><see langword="true"/> if both have the same <see cref="Terminal.Gui.Attribute.Value"/> for all fields.</returns>
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
