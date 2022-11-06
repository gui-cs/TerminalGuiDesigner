using Terminal.Gui;

namespace TerminalGuiDesigner;

public class MenuTracker
{
    public static MenuTracker Instance = new ();

    public MenuItem? CurrentlyOpenMenuItem { get; private set; }

    HashSet<MenuBar> bars = new ();

    private MenuTracker()
    {
    }

    public void Register(MenuBar mb)
    {
        mb.MenuAllClosed += this.MenuAllClosed;
        mb.MenuOpened += this.MenuOpened;
        mb.MenuClosing += this.MenuClosing;

        this.bars.Add(mb);
    }

    private void PruneEmptyBars(MenuBarItem parent, MenuBarItem child)
    {
        if (!child.Children.Any())
        {
            var newChildren = parent.Children.ToList<MenuItem>();
            newChildren.Remove(child);

            parent.Children = newChildren.ToArray();
        }
    }

    private void MenuClosing(MenuClosingEventArgs obj)
    {
        this.CurrentlyOpenMenuItem = null;
    }

    private void MenuOpened(MenuItem obj)
    {
        this.CurrentlyOpenMenuItem = obj;
        this.ConvertEmptyMenus();
    }

    private void MenuAllClosed()
    {
        this.CurrentlyOpenMenuItem = null;
    }

    /// <summary>
    /// Searches child items of all MenuBars tracked by this class
    /// to try and find the parent of the item passed
    /// </summary>
    public MenuBarItem? GetParent(MenuItem item, out MenuBar? hostBar)
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

    /// <summary>
    /// Iterates all menus (e.g. 'File F9', 'View' etc) of a MenuBar and
    /// identifies any entries that have empty submenus (MenuBarItem)
    /// .  Each of those are converted to 'no submenu' Type node MenuItem
    /// </summary>
    public Dictionary<MenuBarItem, MenuItem> ConvertEmptyMenus()
    {
        var toReturn = new Dictionary<MenuBarItem, MenuItem>();

        foreach (var b in this.bars)
        {
            foreach (var bi in b.Menus)
            {
                foreach (var converted in this.ConvertEmptyMenus(b, bi))
                {
                    toReturn.Add(converted.Key, converted.Value);
                }
            }
        }

        return toReturn;
    }

    /// <summary>
    /// Considers a single menu (e.g. 'File F9') of a MenuBar and
    /// identifies any entries that have empty submenus (MenuBarItem)
    /// .  Each of those are converted to 'no submenu' Type node MenuItem
    /// </summary>
    public Dictionary<MenuBarItem, MenuItem> ConvertEmptyMenus(MenuBar bar, MenuBarItem mbi)
    {
        var toReturn = new Dictionary<MenuBarItem, MenuItem>();

        foreach (var c in mbi.Children.OfType<MenuBarItem>())
        {
            this.ConvertEmptyMenus(bar, c);
            if (this.ConvertMenuBarItemToRegularItemIfEmpty(c, out var added))
            {
                if (added != null)
                {
                    toReturn.Add(c, added);
                }

                bar.CloseMenu();
                bar.OpenMenu();
            }
        }

        return toReturn;
    }

    public bool ConvertMenuBarItemToRegularItemIfEmpty(MenuBarItem bar, out MenuItem? added)
    {
        added = null;
        // bar still has more children so don't convert
        if (bar.Children.Any())
        {
            return false;
        }

        var parent = MenuTracker.Instance.GetParent(bar, out _);

        if (parent == null)
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
