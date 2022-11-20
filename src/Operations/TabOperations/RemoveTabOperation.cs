using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Removes (deletes) a <see cref="TabView.Tab"/> from a <see cref="TabView"/>.
/// </summary>
public class RemoveTabOperation : RemoveOperation<TabView, TabView.Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveTabOperation"/> class.
    /// Removes <paramref name="toRemove"/> from a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/> from which you want to remove the tab.</param>
    /// <param name="toRemove">The tab to remove.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TabView"/>.</exception>
    public RemoveTabOperation(Design design, TabView.Tab toRemove)
        : base(
            (t) => t.Tabs.ToArray(),
            (v, a) => v.ReOrderTabs(a),
            tab => tab.Text.ToString() ?? "unnamed tab",
            design,
            toRemove)
    {
    }

    protected override void SetNeedsDisplay()
    {
        if (!this.View.Tabs.Contains(this.View.SelectedTab))
        {
            this.View.SelectedTab = this.View.Tabs.FirstOrDefault();
        }

        base.SetNeedsDisplay();
    }
}
