using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for the <see cref="Size"/> <see langword="struct"/>.
/// </summary>
public static class SizeExtensions
{
    /// <summary>
    /// Returns code snippet that can be used in CodeDOM to generate <paramref name="s"/>.
    /// </summary>
    /// <param name="s">The instance you want to find the code required to construct.</param>
    /// <returns>Constructor code to create <paramref name="s"/>.</returns>
    public static string ToCode(this Size s)
    {
        return $"new Size({s.Width},{s.Height})";
    }
}
