using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.Views;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for the <see cref="MenuBar"/> class.
/// </summary>
public static class MenuBarExtensions
{
    /// <summary>The title used to mark a <see cref="MenuItem"/> as a separator in the designer.</summary>
    public const string SeparatorTitle = "---";
    /// <summary>
    /// Gets the top level selected <see cref="MenuBarItem"/> in the <paramref name="menuBar"/>
    /// or null if it is not open/no selection is set.  Note that this is the top level menu item only
    /// (e.g. File, Edit).
    /// </summary>
    /// <param name="menuBar">Returns the currently selected <see cref="MenuItem"/> on the <paramref name="menuBar"/>.</param>
    /// <returns>Selected <see cref="MenuItem"/> or null if none.</returns>
    public static MenuBarItem? GetSelectedMenuItem(this MenuBar menuBar)
    {
        if (menuBar.Focused is MenuBarItem mbi)
            return mbi;

        return menuBar.SubViews.OfType<MenuBarItem>().FirstOrDefault();
    }

    /// <summary>
    /// Walks all menus in <paramref name="menuBar"/> and replaces any <see cref="Line"/> views
    /// (produced by code generation for separators) with sentinel <see cref="MenuItem"/> instances
    /// whose <see cref="MenuItem.Title"/> is <see cref="SeparatorTitle"/>.
    /// Call this after loading a <see cref="MenuBar"/> from generated code.
    /// </summary>
    public static void ConvertLineSeparatorsToSentinels(this MenuBar menuBar)
    {
        foreach (var mbi in menuBar.SubViews.OfType<MenuBarItem>())
        {
            if (mbi.PopoverMenu?.Root != null)
            {
                ConvertLineSeparatorsInMenu(mbi.PopoverMenu.Root);
            }
        }
    }

    private static void ConvertLineSeparatorsInMenu(Menu menu)
    {
        var allViews = menu.SubViews.ToList();

        if (allViews.Any(v => v is Line))
        {
            foreach (var v in allViews)
            {
                menu.Remove(v);
            }

            foreach (var v in allViews)
            {
                menu.Add(v is Line
                    ? new MenuItem { Title = SeparatorTitle }
                    : v);
            }
        }

        // Recurse into child submenus
        foreach (var child in menu.SubViews.OfType<MenuItem>())
        {
            if (child is MenuBarItem mbi && mbi.PopoverMenu?.Root != null)
            {
                ConvertLineSeparatorsInMenu(mbi.PopoverMenu.Root);
            }
            else if (child.SubMenu != null)
            {
                ConvertLineSeparatorsInMenu(child.SubMenu);
            }
        }
    }

    /// <summary>
    /// Returns the <see cref="MenuBarItem"/> that appears at the <paramref name="screenX"/> of the click.
    /// </summary>
    /// <param name="menuBar"><see cref="MenuBar"/> you want to find the clicked <see cref="MenuBarItem"/> (top level menu) for.</param>
    /// <param name="screenX">Screen coordinate of the click in X.</param>
    /// <returns>The <see cref="MenuBarItem"/> under the mouse at this position or null (only considers X).</returns>
    public static MenuBarItem? ScreenToMenuBarItem(this MenuBar menuBar, IApplication application, Mouse mouse)
    {
        var hit = menuBar.HitTest(application,mouse, out _, out _);

        if(hit == null)
        {
            return null;
        }

        if (hit is MenuBarItem mbi)
        {
            return mbi;
        }
        if (hit.SuperView is MenuBarItem super)
        {
            return super;
        }

        return null;
    }

    /// <summary>
    /// <para>
    /// A MenuItem can have 2 kinds of submenu.  If the MenuItem is an element on a
    /// is on a MenuBar (i.e. a top level menu item like File, Edit, View etc) then it will
    /// have a PopoverMenu.
    /// </para>
    /// <para>
    /// Otherwise if it is a regular MenuItem entry e.g. File->New then it may have an SubMenu
    /// ordinary SubMenu i.e. not a popover.
    /// </para>
    /// <param name="menuItem">The MenuItem to get the menu from.</param>
    /// 
    /// <returns>The Menu containing child items, or null if none exists.</returns>
    public static Menu? GetChildMenu(this MenuItem menuItem, out bool wasPopover)
    {
        if (menuItem is MenuBarItem mbi)
        {
            wasPopover = true;
            return mbi.PopoverMenu?.Root;
        }

        wasPopover = false;
        return menuItem.SubMenu;
    }

    /// <summary>
    /// Gets all MenuItem children from this MenuItem's menu.
    /// </summary>
    /// <param name="menuItem">The MenuItem to get children from.</param>
    /// <param name="wasPopover"></param>
    /// <returns>List of MenuItem children, or empty list if no menu exists.</returns>
    public static List<MenuItem> GetMenuItems(this MenuItem menuItem, out bool wasPopover)
    {
        var menu = menuItem.GetChildMenu(out wasPopover);
        return menu?.SubViews.OfType<MenuItem>().ToList() ?? new List<MenuItem>();
    }

    /// <summary>
    /// Replaces all MenuItem children in this MenuItem's menu with the specified items.
    /// This handles the complexity of removing and re-adding items in the correct order.
    /// </summary>
    /// <param name="menuItem">The MenuItem whose children should be replaced.</param>
    /// <param name="newItems">The new list of MenuItems in the desired order.</param>
    public static void SetMenuItems(this MenuItem menuItem, List<MenuItem> newItems)
    {
        var menu = menuItem.GetChildMenu(out _);
        if (menu == null)
        {
            return;
        }

        // Get all current views and separate MenuItems from non-MenuItems (like Lines)
        var allViews = menu.SubViews.ToList();
        var nonMenuItems = allViews.Where(v => v is not MenuItem).ToList();

        // Remove all MenuItems
        foreach (var item in allViews.OfType<MenuItem>().ToList())
        {
            menu.Remove(item);
        }

        // Add new MenuItems in order
        foreach (var item in newItems)
        {
            menu.Add(item);
        }

        // Re-add non-MenuItem views (like separators)
        foreach (var item in nonMenuItems)
        {
            menu.Add(item);
        }
    }

    /// <summary>
    /// Inserts a MenuItem at the specified index among other MenuItems.
    /// This handles finding the correct position among all SubViews.
    /// </summary>
    /// <param name="menuItem">The parent MenuItem to insert into.</param>
    /// <param name="index">The index among MenuItems (not SubViews) to insert at.</param>
    /// <param name="itemToInsert">The MenuItem to insert.</param>
    public static void InsertMenuItem(this MenuItem menuItem, int index, MenuItem itemToInsert)
    {
        var items = menuItem.GetMenuItems(out _);
        items.Insert(Math.Min(index, items.Count), itemToInsert);
        menuItem.SetMenuItems(items);

        itemToInsert.SetFocus();
    }

    /// <summary>
    /// Removes a MenuItem from this MenuItem's menu.
    /// </summary>
    /// <param name="menuItem">The parent MenuItem to remove from.</param>
    /// <param name="itemToRemove">The MenuItem to remove.</param>
    /// <returns>True if the item was found and removed.</returns>
    public static bool RemoveMenuItem(this MenuItem menuItem, MenuItem itemToRemove)
    {
        var menu = menuItem.GetChildMenu(out _);
        if (menu == null)
        {
            return false;
        }

        menu.Remove(itemToRemove);
        return true;
    }
}
