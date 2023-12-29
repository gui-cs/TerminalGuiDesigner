using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;

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
    /// Gets the currently selected <see cref="MenuItem"/> if any.  To work
    /// you must subscribe all <see cref="MenuBar"/> to this class so that
    /// it can watch <see cref="MenuBar.MenuOpened"/> etc.
    /// </summary>
    public MenuItem? CurrentlyOpenMenuItem { get; private set; }

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

        mb.MenuAllClosed += this.MenuAllClosed;
        mb.MenuOpened += this.MenuOpened;
        mb.MenuClosing += this.MenuClosing;

        this.bars.Add(mb);
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
    /// <returns>The immediate parent of <paramref name="item"/>.</returns>
    /// <remarks>Result may be a top level menu (e.g. File, View)
    /// or a sub-menu parent (e.g. View=>Windows).</remarks>
    public MenuBarItem? GetParent( MenuItem item, out MenuBar? hostBar )
    {
        foreach (var bar in this.bars)
        {
            foreach (var sub in bar.Menus)
            {
                var candidate = this.GetParent(item, sub);

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

    public bool TryGetParent(MenuItem item, [NotNullWhen(true)]out MenuBar? hostBar, [NotNullWhen(true)] out MenuBarItem? parentItem)
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
        Dictionary<MenuBarItem, MenuItem> dictionary = new( );
        foreach (var b in this.bars)
        {
            foreach (var bi in b.Menus)
            {
                foreach (var converted in this.ConvertEmptyMenus(dictionary, b, bi))
                {
                    dictionary.TryAdd( converted.Key, converted.Value );
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

        // bar still has more children so don't convert
        if ( bar.Children.Length != 0 )
        {
            return false;
        }

        if ( !Instance.TryGetParent( bar, out _, out MenuBarItem? parent ) )
        {
            return false;
        }

        var children = parent.Children.ToList<MenuItem>();
        var idx = children.IndexOf(bar);

        if (idx < 0)
        {
            return false;
        }

        // bar has no children so convert to MenuItem
        added = new MenuItem { Title = bar.Title };
        added.Data = bar.Data;
        added.Shortcut = bar.Shortcut;

        children.RemoveAt(idx);
        children.Insert(idx, added);

        parent.Children = children.ToArray();

        return true;
    }

    /// <inheritdoc cref="ConvertEmptyMenus()"/>
    private Dictionary<MenuBarItem, MenuItem> ConvertEmptyMenus(Dictionary<MenuBarItem,MenuItem> dictionary, MenuBar bar, MenuBarItem mbi)
    {
        foreach (var c in mbi.Children.OfType<MenuBarItem>())
        {
            this.ConvertEmptyMenus(dictionary,bar, c);
            if ( ConvertMenuBarItemToRegularItemIfEmpty( c, out var added))
            {
                if (added != null)
                {
                    dictionary.TryAdd(c, added);
                }

                bar.CloseMenu();
                bar.OpenMenu();
            }
        }

        return dictionary;
    }

    private void MenuClosing(object? sender, MenuClosingEventArgs obj)
    {
        this.CurrentlyOpenMenuItem = null;
    }

    private void MenuOpened(object? sender, MenuOpenedEventArgs obj)
    {
        this.CurrentlyOpenMenuItem = obj.MenuItem;
        this.ConvertEmptyMenus( );
    }

    private void MenuAllClosed(object? sender, EventArgs e)
    {
        this.CurrentlyOpenMenuItem = null;
    }

    private MenuBarItem? GetParent(MenuItem item, MenuBarItem sub)
    {
        // if we have a reference to the item then
        // it means that we are the parent (we contain it)
        if (sub.Children.Contains(item))
        {
            return sub;
        }

        // recursively check dropdowns
        foreach (var dropdown in sub.Children.OfType<MenuBarItem>())
        {
            var candidate = this.GetParent(item, dropdown);

            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }
}
