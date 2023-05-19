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
        return View.FindDeepestView(start,x,y,out _, out _);
    }
}
