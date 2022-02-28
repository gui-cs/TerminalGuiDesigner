using Terminal.Gui;

namespace TerminalGuiDesigner.Windows;

public static class ListViewExtensions
{
public static void EnsureSelectedItemVisible(this ListView list)
{
    if (list.SelectedItem < list.TopItem)
    {
        list.TopItem = list.SelectedItem;
    }
    else if (list.Frame.Height > 0 && list.SelectedItem >= list.TopItem + list.Frame.Height)
    {
        list.TopItem = Math.Max(list.SelectedItem - list.Frame.Height + 2, 0);
    }
}
}
