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
    public static StatusItem? ScreenToMenuBarItem(this StatusBar statusBar, int screenX)
    {
        // These might be changed in Terminal.Gui library
        const int initialWhitespace = 1;
        const int afterEachItemWhitespace = 3; /* currently a space then a '|' then another space*/

        if (statusBar.Items.Length == 0)
        {
            return null;
        }

        var clientPoint = statusBar.ScreenToBounds(screenX, 0);

        // if click is not in our client area
        if (clientPoint.X < initialWhitespace)
        {
            return null;
        }

        // Calculate the x display positions of each menu
        int distance = initialWhitespace;
        Dictionary<int, StatusItem?> xLocations = new();

        foreach (var si in statusBar.Items)
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
}