using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods to access private/internal functions of <see cref="Application"/>.
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Finds the deepest <see cref="View"/> at screen coordinates x,y.
    /// This is a private static method in the main Terminal.Gui library
    /// invoked via reflection.
    /// </summary>
    /// <param name="start">The top level <see cref="View"/> to start looking down from.</param>
    /// <param name="x">Screen X coordinate.</param>
    /// <param name="y">Screen Y coordinate.</param>
    /// <returns>The <see cref="View"/> that renders into the screen space (hit by the click).</returns>
    /// <exception cref="MissingMethodException">Thrown if Terminal.Gui private API changes.</exception>
    public static View? FindDeepestView(View start, int x, int y)
    {
        var method = typeof(Application).GetMethod(
            nameof(FindDeepestView),
            BindingFlags.Static | BindingFlags.NonPublic,
            new[] { typeof(View), typeof(int), typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() });

        if (method == null)
        {
            throw new MissingMethodException("Static method FindDeepestView not found on Application class");
        }

        int resx = 0;
        int resy = 0;

        return (View?)method.Invoke(null, new object[] { start, x, y, resx, resy });
    }
}
