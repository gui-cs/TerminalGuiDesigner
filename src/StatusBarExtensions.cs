using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Contains extension methods for the <see cref="StatusBar"/> class.
/// </summary>
public static class StatusBarExtensions
{
    /// <summary>
    /// Returns the <see cref="StatusItem"/> that appears at the <paramref name="screenX"/> of the click.
    /// </summary>
    /// <param name="statusBar"><see cref="StatusBar"/> you want to find the clicked <see cref="MenuBarItem"/> (top level menu) for.</param>
    /// <param name="screenX">Screen coordinate of the click in X.</param>
    /// <returns>The <see cref="StatusItem"/> under the mouse at this position or null (only considers X).</returns>
    public static Shortcut? ScreenToMenuBarItem(this StatusBar statusBar, int screenX)
    {
        // These might be changed in Terminal.Gui library
        const int initialWhitespace = 1;
        const int afterEachItemWhitespace = 3; /* currently a space then a '|' then another space*/


        if (statusBar.CountShortcuts() == 0)
        {
            return null;
        }

        var clientPoint = statusBar.ScreenToContent(new Point(screenX, 0));

        // if click is not in our client area
        if (clientPoint.X < initialWhitespace)
        {
            return null;
        }

        // Calculate the x display positions of each menu
        int distance = initialWhitespace;
        Dictionary<int, Shortcut?> xLocations = new();

        foreach (var si in statusBar.Subviews.OfType<Shortcut>())
        {
            xLocations.Add(distance, si);
            distance += si.Title.GetColumns() + afterEachItemWhitespace;
        }

        // anything after this is not a click on a menu
        xLocations.Add(distance, null);

        // LastOrDefault does not work with Dictionaries, if we somehow still have a point outside bounds
        // of anything then just return null;
        if (!xLocations.Any(m => m.Key <= clientPoint.X))
        {
            return null;
        }

        // Return the last menu item that begins rendering before this X point
        return xLocations.Last(m => m.Key <= clientPoint.X).Value;
    }

    /// <summary>
    /// Return a count of the number of <see cref="Shortcut"/> in the <see cref="StatusBar"/>.
    /// </summary>
    /// <param name="bar"></param>
    /// <returns></returns>
    public static int CountShortcuts(this StatusBar bar)
    {
        return bar.Subviews.OfType<Shortcut>().Count();
    }

    public static Shortcut[] GetShortcuts(this StatusBar bar)
    {
        return bar.Subviews.OfType<Shortcut>().ToArray();
    }

    public static void SetShortcuts(this StatusBar bar, Shortcut[] shortcuts)
    {
        foreach(var old in bar.GetShortcuts())
        {
            bar.Remove(old);
        }

        foreach (var shortcut in shortcuts)
        {
            bar.Add(shortcut);
        }
    }
}