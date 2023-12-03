namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for the <see cref="Attribute"/> class.
/// </summary>
public static class AttributeExtensions
{
    /// <summary>
    /// Returns code to construct a <see cref="Attribute"/>.
    /// </summary>
    /// <param name="a">To construct.</param>
    /// <returns>Code construct <paramref name="a"/>.</returns>
    public static string ToCode(this Terminal.Gui.Attribute a)
    {
        return $"new Terminal.Gui.Attribute(Color.{a.Foreground},Color.{a.Background})";
    }
}
