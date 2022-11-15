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

    private static object GetNonNullPrivateFieldValue(string fieldName, object item, Type type)
    {
        var selectedField = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new Exception($"Expected private field {fieldName} was not present on {type.Name}");
        return selectedField.GetValue(item)
            ?? throw new Exception($"Private field {fieldName} was unexpectedly null on {type.Name}");
    }
}
