using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Removes (deletes) a <see cref="Tab"/> from a <see cref="TabView"/>.
/// </summary>
public class RemoveTabOperation : RemoveOperation<TabView, Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveTabOperation"/> class.
    /// Removes <paramref name="toRemove"/> from a <see cref="TabView"/>.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="design">Wrapper for a <see cref="TabView"/> from which you want to remove the tab.</param>
    /// <param name="toRemove">The tab to remove.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TabView"/>.</exception>
    public RemoveTabOperation(IApplication app, Design design, Tab toRemove)
        : base(
            app,
            (t) => t.Tabs.ToArray(),
            (v, a) => v.ReOrderTabs(a),
            tab => tab.Text.ToString() ?? "unnamed tab",
            design,
            toRemove)
    {
    }

    /// <inheritdoc/>
    protected override void SetNeedsDraw()
    {
        if (!this.View.Tabs.Contains(this.View.SelectedTab))
        {
            this.View.SelectedTab = this.View.Tabs.FirstOrDefault();
        }

        base.SetNeedsDraw();
    }
}
