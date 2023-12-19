using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for the <see cref="MenuBar"/> class.
/// </summary>
public static class MenuBarExtensions
{
    /// <summary>
    /// Gets the top level selected <see cref="MenuBarItem"/> in the <paramref name="menuBar"/>
    /// or null if it is not open/no selection is set.  Note that this is the top level menu item only
    /// (e.g. File, Edit).
    /// </summary>
    /// <param name="menuBar">Returns the currently selected <see cref="MenuItem"/> on the <paramref name="menuBar"/>.</param>
    /// <returns>Selected <see cref="MenuItem"/> or null if none.</returns>
    public static MenuBarItem? GetSelectedMenuItem(this MenuBar menuBar)
    {
        int selected = menuBar.GetNonNullPrivateFieldValue<int,>( "selected" );

        if (selected < 0 || selected >= menuBar.Menus.Length)
        {
            return null;
        }

        return menuBar.Menus[selected];
    }

    /// <summary>
    /// Returns the <see cref="MenuBarItem"/> that appears at the <paramref name="screenX"/> of the click.
    /// </summary>
    /// <param name="menuBar"><see cref="MenuBar"/> you want to find the clicked <see cref="MenuBarItem"/> (top level menu) for.</param>
    /// <param name="screenX">Screen coordinate of the click in X.</param>
    /// <returns>The <see cref="MenuBarItem"/> under the mouse at this position or null (only considers X).</returns>
    public static MenuBarItem? ScreenToMenuBarItem(this MenuBar menuBar, int screenX)
    {
        // These might be changed in Terminal.Gui library
        // TODO: Maybe load these from a config file, so we aren't at TG's mercy
        const int initialWhitespace = 1;
        const int afterEachItemWhitespace = 2;

        if (menuBar.Menus.Length == 0)
        {
            return null;
        }

        var clientPoint = menuBar.ScreenToBounds(screenX, 0);

        // if click is not in our client area
        if (clientPoint.X < initialWhitespace)
        {
            return null;
        }

        // Calculate the x display positions of each menu
        int distance = initialWhitespace;
        Dictionary<int, MenuBarItem?> menuXLocations = new();

        foreach (var mb in menuBar.Menus)
        {
            menuXLocations.Add(distance, mb);
            distance += mb.Title.GetColumns() + afterEachItemWhitespace;
        }

        // anything after this is not a click on a menu
        menuXLocations.Add(distance, null);

        // LastOrDefault does not work with Dictionaries, if we somehow still have a point outside bounds
        // of anything then just return null;
        if (!menuXLocations.Any(m => m.Key <= clientPoint.X))
        {
            return null;
        }

        // Return the last menu item that begins rendering before this X point
        return menuXLocations.Last(m => m.Key <= clientPoint.X).Value;
    }
}
