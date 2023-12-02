using System.Reflection;
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
        var selected = (int)GetNonNullPrivateFieldValue("selected", menuBar, typeof(MenuBar));

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

    /// <summary>
    /// Changes the <see cref="StatusItem.Shortcut"/> even though it has no setter in Terminal.Gui.
    /// </summary>
    /// <param name="item"><see cref="StatusItem"/> to change <see cref="StatusItem.Shortcut"/> on.</param>
    /// <param name="newShortcut">The new value for <see cref="StatusItem.Shortcut"/>.</param>
    public static void SetShortcut(this StatusItem item, Key newShortcut)
    {
        // See: https://stackoverflow.com/a/40917899/4824531
        const string backingFieldName = "<Shortcut>k__BackingField";

        var field =
            typeof(StatusItem).GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Exception($"Could not find auto backing field '{backingFieldName}'");

        field.SetValue(item, newShortcut);
    }

    private static object GetNonNullPrivateFieldValue(string fieldName, object item, Type type)
    {
        var selectedField = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new Exception($"Expected private field {fieldName} was not present on {type.Name}");
        return selectedField.GetValue(item)
            ?? throw new Exception($"Private field {fieldName} was unexpectedly null on {type.Name}");
    }
}
