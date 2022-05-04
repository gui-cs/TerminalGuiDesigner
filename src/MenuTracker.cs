using Terminal.Gui;

namespace TerminalGuiDesigner;

internal class MenuTracker
{
    public static MenuTracker Instance = new();

    public MenuItem? CurrentlyOpenMenuItem { get; private set; }

    HashSet<MenuBar> bars = new ();


    private MenuTracker()
    {

    }

    internal void Register(MenuBar mb)
    {
        mb.MenuAllClosed += MenuAllClosed;
        mb.MenuOpened += MenuOpened;
        mb.MenuClosing += MenuClosing;

        bars.Add(mb);
    }

    private void MenuClosing(MenuClosingEventArgs obj)
    {
        CurrentlyOpenMenuItem = null;
    }

    private void MenuOpened(MenuItem obj)
    {
        CurrentlyOpenMenuItem = obj;
    }

    private void MenuAllClosed()
    {
        CurrentlyOpenMenuItem = null;
    }

    /// <summary>
    /// Searches child items of all MenuBars tracked by this class
    /// to try and find the parent of the item passed
    /// </summary>
    public MenuBarItem? GetParent(MenuItem item)
    {
        foreach(var bar in bars)
        {
            foreach(var sub in bar.Menus)
            {
                var candidate = GetParent(item,sub);

                if(candidate != null)
                {
                    return candidate;
                }
            }
        }

        return null;
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
