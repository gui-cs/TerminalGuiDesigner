using Terminal.Gui;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Extension methods for the <see cref="Rect"/> <see langword="struct"/>.
/// </summary>
public static class RectExtensions
{
    /// <summary>
    /// Returns a <see cref="Rect"/> between the two points.  Points argument
    /// order does not matter (i.e. p2 can be above/below and/or
    /// left/right of p1).  Returns null if either point is null.
    /// </summary>
    /// <param name="p1">One corner of the <see cref="Rect"/> you want to create.</param>
    /// <param name="p2">Opposite corner to <paramref name="p1"/> of the <see cref="Rect"/>
    /// you want to create.</param>
    internal static Rect? FromBetweenPoints(Point? p1, Point? p2)
    {
        if (p1 == null || p2 == null)
        {
            return null;
        }

        return Rect.FromLTRB(
            Math.Min(p1.Value.X, p2.Value.X),
            Math.Min(p1.Value.Y, p2.Value.Y),
            Math.Max(p1.Value.X, p2.Value.X),
            Math.Max(p1.Value.Y, p2.Value.Y));
    }
}