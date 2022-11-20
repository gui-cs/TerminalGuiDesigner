using Terminal.Gui;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for <see cref="TabView"/>.
/// </summary>
public static class TabViewExtensions
{
    /// <summary>
    /// Inserts or moves <paramref name="tab"/> to <paramref name="atIndex"/> and makes it
    /// the <see cref="TabView.SelectedTab"/>.
    /// </summary>
    /// <param name="tv">The <see cref="TabView"/> you want to make changes to.</param>
    /// <param name="atIndex">The index you want to move/insert at.</param>
    /// <param name="tab">The tab to move/insert.</param>
    public static void InsertTab(this TabView tv, int atIndex, Tab tab)
    {
        var list = tv.Tabs.ToList();

        var newIndex = Math.Max(0, Math.Min(list.Count - 1, atIndex));

        list.Remove(tab);
        list.Insert(newIndex, tab);

        var origTabs = tv.Tabs;

        // View.Tabs is readonly so we have to
        // remove all the tabs
        foreach (var t in origTabs.ToArray())
        {
            tv.RemoveTab(t);
        }

        // then add them back in again in the new order
        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];

            // put all the tabs back in again and select
            // the tab in it's new position
            tv.AddTab(t, i == newIndex);
        }
    }

    /// <summary>
    /// Reorders <see cref="Tab"/> of <paramref name="tabView"/> to match <paramref name="newOrder"/>.
    /// </summary>
    /// <param name="tabView">The view whose tabs should be reordered.</param>
    /// <param name="newOrder">The new order to enforce.</param>
    public static void ReOrderTabs(this TabView tabView, TabView.Tab[] newOrder)
    {
        var selectedBefore = tabView.SelectedTab;

        foreach (var tab in tabView.Tabs.ToArray())
        {
            tabView.RemoveTab(tab);
        }

        foreach (var tab in newOrder)
        {
            tabView.AddTab(tab, true);
        }

        tabView.SelectedTab = selectedBefore;
    }
}
