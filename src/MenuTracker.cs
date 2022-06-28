using Terminal.Gui;

namespace TerminalGuiDesigner;

public class MenuTracker
{
    public static MenuTracker Instance = new();

    public MenuItem? CurrentlyOpenMenuItem { get; private set; }

    HashSet<MenuBar> bars = new ();


    private MenuTracker()
    {

    }

    public void Register(MenuBar mb)
    {
        mb.MenuAllClosed += MenuAllClosed;
        mb.MenuOpened += MenuOpened;
        mb.MenuClosing += MenuClosing;

        bars.Add(mb);
    }


    private void PruneEmptyBars(MenuBarItem parent, MenuBarItem child)
    {
        if(!child.Children.Any())
        {
            var newChildren = parent.Children.ToList<MenuItem>();
            newChildren.Remove(child);

            parent.Children = newChildren.ToArray();
        }
    }

    private void MenuClosing(MenuClosingEventArgs obj)
    {
        CurrentlyOpenMenuItem = null;
    }

    private void MenuOpened(MenuItem obj)
    {
        CurrentlyOpenMenuItem = obj;
        ConvertEmptyMenus();
    }

    private void MenuAllClosed()
    {
        CurrentlyOpenMenuItem = null;
    }

    /// <summary>
    /// Searches child items of all MenuBars tracked by this class
    /// to try and find the parent of the item passed
    /// </summary>
    public MenuBarItem? GetParent(MenuItem item, out MenuBar? hostBar)
    {
        foreach(var bar in bars)
        {
            foreach(var sub in bar.Menus)
            {
                var candidate = GetParent(item,sub);

                if(candidate != null)
                {
                    hostBar = bar;
                    return candidate;
                }
            }
        }
    
        hostBar = null;
        return null;
    }

    public void ConvertEmptyMenus()
    {
        foreach(var b in bars.ToArray())
            foreach(var bi in b.Menus.ToArray())
            {
                // if a top level menu has no children 
                if(bi.Children.Length == 0)
                {
                    // get rid of it
                    b.Menus = b.Menus.Except(new []{bi}).ToArray();
                    continue;
                }

                ConvertEmptyMenus(b,bi);
            }
                
    }

    private void ConvertEmptyMenus(MenuBar bar, MenuBarItem mbi)
    {
        foreach(var c in mbi.Children.OfType<MenuBarItem>())
        {
            ConvertEmptyMenus(bar, c);
            if(ConvertMenuBarItemToRegularItemIfEmpty(c))
            {
                bar.CloseMenu();
                bar.OpenMenu();
            }
        }
    }
    private bool ConvertMenuBarItemToRegularItemIfEmpty(MenuBarItem bar)
    {
        // bar still has more children so don't convert
        if(bar.Children.Any())
            return false;

        var parent = MenuTracker.Instance.GetParent(bar,out _);

        if(parent == null)
            return false;

        var children = parent.Children.ToList<MenuItem>();
        var idx = children.IndexOf(bar);

        if(idx < 0)
            return false;
        
        // bar has no children so convert to MenuItem
        var added = new MenuItem {Title = bar.Title};

        children.RemoveAt(idx);
        children.Insert(idx,added);

        parent.Children = children.ToArray();
        
        return true;
    }

    private MenuBarItem? GetParent(MenuItem item, MenuBarItem sub)
    {
        // if we have a reference to the item then
        // it means that we are the parent (we contain it)
        if(sub.Children.Contains(item))
        {
            return sub;
        }

        // recursively check dropdowns
        foreach(var dropdown in sub.Children.OfType<MenuBarItem>())
        {
            var candidate = GetParent(item,dropdown);

            if(candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }
}
