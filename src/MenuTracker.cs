using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using TerminalGuiDesigner.UI;

namespace TerminalGuiDesigner;

/// <summary>
/// Singleton class for tracking all <see cref="MenuBar"/> including which is open
/// and what <see cref="MenuItem"/> are in them.
/// </summary>
public class MenuTracker
{
    private readonly ConcurrentBag<MenuBar> bars = new( );

    private MenuTracker()
    {
    }

    /// <summary>
    /// Gets the Singleton instance access property.
    /// </summary>
    public static MenuTracker Instance { get; } = new();


    /// <summary>
    /// Registers listeners for <paramref name="mb"/> to track open/close.
    /// </summary>
    /// <param name="mb"><see cref="MenuBar"/> to track.</param>
    public void Register(MenuBar mb)
    {
        // if we already track this bar ignore a repeat registration
        if (this.bars.Contains(mb))
        {
            return;
        }

        this.bars.Add(mb);

        // Subscribe to menu open/close events for all MenuBarItems
        foreach (var menuBarItem in mb.SubViews.OfType<MenuBarItem>())
        {
            SubscribeToMenuBarItem(menuBarItem);
        }
    }

    private void SubscribeToMenuBarItem(MenuBarItem menuBarItem)
    {
        if (menuBarItem.PopoverMenu != null)
        {
            menuBarItem.PopoverMenuOpenChanged += OnPopoverMenuOpenChanged;
        }
    }

    private void UnsubscribeFromMenuBarItem(MenuBarItem menuBarItem)
    {
        if (menuBarItem.PopoverMenu != null)
        {
            menuBarItem.PopoverMenuOpenChanged -= OnPopoverMenuOpenChanged;
        }
    }

    private void OnPopoverMenuOpenChanged(object? sender, ValueChangedEventArgs<bool> e)
    {
        if (sender is MenuBarItem menuBarItem)
        {
            // Convert empty menus when closing
            if (!e.NewValue)
            {
                this.ConvertEmptyMenus();
            }
        }
    }

    /// <summary>
    /// Unregisters listeners for <paramref name="mb"/>.
    /// </summary>
    /// <param name="mb"><see cref="MenuBar"/> to stop tracking.</param>
    public void UnregisterMenuBar( MenuBar? mb )
    {
        if (mb == null || !bars.TryTake(out mb))
        {
            return;
        }

        // Unsubscribe from all MenuBarItems
        foreach (var menuBarItem in mb.SubViews.OfType<MenuBarItem>())
        {
            UnsubscribeFromMenuBarItem(menuBarItem);
        }
    }

    /// <summary>
    /// <para>
    /// Searches child items of all MenuBars tracked by this class
    /// to try and find the parent of the item passed.
    /// </para>
    /// <para>
    /// Note: Search is recursive and dips into sub-menus.  For sub-menus it is
    /// the immediate parent that is returned.
    /// </para>
    /// </summary>
    /// <param name="item">The item whose parent you want to find.</param>
    /// <param name="hostBar">The <see cref="MenuBar"/> that owns <paramref name="item"/> or.
    /// null if not found or parent not registered (see <see cref="Register(MenuBar)"/>).</param>
    /// <returns>The immediate parent of <paramref name="item"/>. Can be MenuBarItem or MenuItem.</returns>
    /// <remarks>Result may be a top level menu (e.g. File, View)
    /// or a sub-menu parent (e.g. View=>Windows).</remarks>
    private MenuItem? GetParent( MenuItem item, out MenuBar? hostBar )
    {
        foreach (var bar in this.bars)
        {
            foreach (var sub in bar.SubViews.OfType<MenuBarItem>())
            {
                var candidate = this.FindParentRecursive(item, sub);

                if (candidate != null)
                {
                    hostBar = bar;
                    return candidate;
                }
            }
        }

        hostBar = null;
        return null;
    }

    /// <summary>
    ///   Searches child items of all MenuBars tracked by this class to try and find the parent of the item passed.
    /// </summary>
    /// <param name="item">The item whose parent you want to find.</param>
    /// <param name="hostBar">
    ///   When this method returns true, the <see cref="MenuBar" /> that owns <paramref name="item" />.<br /> Otherwise, <see langword="null" /> if
    ///   not found or parent not registered (see <see cref="Register(MenuBar)" />).
    /// </param>
    /// <param name="parentItem">
    ///   When this method returns <see langword="true" />, the immediate parent of <paramref name="item" />.<br /> Otherwise,
    ///   <see langword="null" />. Can be either a MenuBarItem or a MenuItem with a SubMenu.
    /// </param>
    /// <remarks>
    ///   Search is recursive and dips into sub-menus.<br /> For sub-menus it is the immediate parent that is returned.
    /// </remarks>
    /// <returns>A <see langword="bool" /> indicating if the search was successful or not.</returns>
    public bool TryGetParent( MenuItem item, [NotNullWhen( true )] out MenuBar? hostBar, [NotNullWhen( true )] out MenuItem? parentItem )
    {
        var parentCandidate = GetParent( item, out hostBar );
        if ( parentCandidate is null )
        {
            hostBar = null;
            parentItem = null;
            return false;
        }

        parentItem = parentCandidate;
        return true;
    }

    /// <summary>
    /// Iterates all menus (e.g. 'File F9', 'View' etc) of a MenuBar and
    /// identifies any entries that have empty sub-menus (MenuBarItem).
    /// Each of those are converted to 'no sub-menu' Type node MenuItem.
    /// </summary>
    /// <returns>Dictionary of all converted <see cref="MenuBarItem"/> and
    /// the substitution object (<see cref="MenuItem"/>).  See
    /// <see cref="ConvertMenuBarItemToRegularItemIfEmpty(MenuBarItem, out MenuItem?)"/>
    /// for more information.</returns>
    public Dictionary<MenuBarItem, MenuItem> ConvertEmptyMenus( )
    {
        Dictionary<MenuBarItem, MenuItem> dictionary = [];
        foreach (var b in this.bars)
        {
            foreach (var bi in b.SubViews.OfType<MenuBarItem>())
            {
                foreach ( ( MenuBarItem? convertedMenuBarItem, MenuItem? convertedMenuItem ) in this.ConvertEmptyMenus( dictionary, b, bi ) )
                {
                    dictionary.TryAdd( convertedMenuBarItem, convertedMenuItem );
                }
            }
        }

        return dictionary;
    }

    /// <summary>
    /// <para>Converts <paramref name="bar"/> from a <see cref="MenuBarItem"/> (menu entry with a sub-menu)
    /// to a <see cref="MenuItem"/> (menu entry without a sub-menu).
    /// </para>
    /// <para>
    /// Note: This method only works when <paramref name="bar"/> is empty.  This prevents accidentally loosing
    /// users menus.  So to use it you must first clear items from the sub-menu.
    /// </para>
    /// </summary>
    /// <param name="bar">To convert.</param>
    /// <param name="added">The result of the conversion (same text, same index etc but
    /// <see cref="MenuItem"/> instead of <see cref="MenuBarItem"/>).</param>
    /// <returns><see langword="true"/> if conversion was possible (menu was empty and belonged to tracked menu).</returns>
    internal static bool ConvertMenuBarItemToRegularItemIfEmpty( MenuBarItem bar, [NotNullWhen( true )] out MenuItem? added )
    {
        added = null;

        // In the new API, MenuBarItem can only exist at the top level of a MenuBar
        // So this conversion doesn't apply the same way
        // However, we can check if a MenuItem with a SubMenu has become empty
        // and should have its SubMenu removed

        // Check if the bar is actually a top-level MenuBarItem (which should keep its structure)
        // or if it's being used in a submenu context (which shouldn't happen in the new API)

        // For now, we'll check if it has any menu items in its PopoverMenu
        if (bar.PopoverMenu?.Root?.SubViews.OfType<MenuItem>().Any() == true)
        {
            // bar still has children so don't convert
            return false;
        }

        // In the new API, we don't convert MenuBarItems to MenuItems
        // MenuBarItems stay as MenuBarItems even if empty (they're top-level)
        // This method is less relevant in the new structure
        return false;
    }

    /// <inheritdoc cref="ConvertEmptyMenus()"/>
    private Dictionary<MenuBarItem, MenuItem> ConvertEmptyMenus(Dictionary<MenuBarItem,MenuItem> dictionary, MenuBar bar, MenuBarItem mbi)
    {
        // In the new API, we need to look for MenuItems with empty SubMenus
        // and potentially remove those SubMenus
        if (mbi.PopoverMenu?.Root != null)
        {
            foreach (var menuItem in mbi.PopoverMenu.Root.SubViews.OfType<MenuItem>())
            {
                // Recursively check for empty submenus
                ConvertEmptySubMenus(menuItem);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Helper method to recursively convert empty submenus
    /// In the new API, this removes empty SubMenus from MenuItems
    /// </summary>
    private void ConvertEmptySubMenus(MenuItem menuItem)
    {
        if (menuItem.SubMenu != null)
        {
            // First recursively process children
            foreach (var child in menuItem.SubMenu.SubViews.OfType<MenuItem>())
            {
                ConvertEmptySubMenus(child);
            }

            // If the submenu is now empty, remove it
            if (menuItem.SubMenu.SubViews.OfType<MenuItem>().Any() == false)
            {
                menuItem.SubMenu = null;
            }
        }
    }

    /// <summary>
    /// Recursively searches for the immediate parent of a MenuItem within a MenuItem hierarchy.
    /// Uses the extension methods to simplify the logic.
    /// </summary>
    /// <param name="item">The MenuItem to find the parent of</param>
    /// <param name="potentialParent">The MenuItem to search within (could be MenuBarItem or MenuItem)</param>
    /// <returns>The immediate parent MenuItem, or null if not found in this branch</returns>
    private MenuItem? FindParentRecursive(MenuItem item, MenuItem potentialParent)
    {
        // Check if the item is directly in this MenuItem's children
        var children = potentialParent.GetMenuItems(out _);
        if (children.Contains(item))
        {
            return potentialParent;
        }

        // Recursively check each child's submenu
        foreach (var child in children)
        {
            if (child.SubMenu != null)
            {
                var result = FindParentRecursive(item, child);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    internal static MenuItem? GetFocusedMenuItemIfAny(IApplication app)
    {
        var m = app.Popovers?.Popovers?.FirstOrDefault(p => p.Visible) as PopoverMenu;
        
        // Don't let user edit the literal popup context menu in main app (that appears
        // when right clicking in empty space).
        if(m?.Data is string s && s == Editor.DesignerCorePopoverName)
        {
            return null;
        }

        var focused = m?.Focused;

        int maxIterations = 10;
        while(focused != null && focused is Menu menu && maxIterations-- > 0)
        {
            focused = menu.Focused;
        }

        return focused as MenuItem;
    }
}
