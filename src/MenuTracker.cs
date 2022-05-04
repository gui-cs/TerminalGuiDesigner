using Terminal.Gui;

namespace TerminalGuiDesigner;

internal class MenuTracker
{
    public static MenuTracker Instance = new();

    public MenuItem? CurrentlyOpenMenuItem { get; private set; }

    private MenuTracker()
    {

    }

    internal void Register(MenuBar mb)
    {
        mb.MenuAllClosed += MenuAllClosed;
        mb.MenuOpened += MenuOpened;
        mb.MenuClosing += MenuClosing;
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

}
