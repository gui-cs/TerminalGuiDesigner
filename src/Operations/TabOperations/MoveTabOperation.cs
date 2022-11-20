using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Moves a <see cref="TabView.Tab"/> left or right within the ordering
/// of tabs in a <see cref="TabView"/>.
/// </summary>
public class MoveTabOperation : MoveOperation<TabView, TabView.Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoveTabOperation"/> class.
    /// Creates an operation that will change the ordering of tabs within
    /// a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/>.</param>
    /// <param name="toMove">The Tab to move.</param>
    /// <param name="adjustment">Negative to move tab left, positive to move tab right.</param>
    public MoveTabOperation(Design design, TabView.Tab toMove, int adjustment)
        : base(
            (t) => t.Tabs.ToArray(),
            SetTabs,
            tab => tab.Text.ToString() ?? "unnamed tab",
            design,
            toMove,
            adjustment)
    {
    }

    private static void SetTabs(TabView tabView, TabView.Tab[] newOrder)
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
