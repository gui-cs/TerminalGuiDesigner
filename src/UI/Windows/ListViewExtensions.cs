using Terminal.Gui;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Extension methods for the <see cref="ListView"/> class.
/// </summary>
public static class ListViewExtensions
{
    /// <summary>
    /// Adjusts <see cref="ListView.TopItem"/> so that <see cref="ListView.SelectedItem"/> is
    /// within the view bounds (scrolls to selected item if not in current view area).
    /// </summary>
    /// <param name="list">The <see cref="ListView"/> to scroll.</param>
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
