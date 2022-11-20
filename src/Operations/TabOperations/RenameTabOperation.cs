using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Renames the <see cref="TabView.Tab.Text"/> of the currently selected
/// <see cref="TabView.Tab"/> of a <see cref="TabView"/>.
/// </summary>
public class RenameTabOperation : RenameOperation<TabView, TabView.Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameTabOperation"/> class.
    /// This command changes the <see cref="TabView.Tab.Text"/> on a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/>.</param>
    public RenameTabOperation(Design design, TabView.Tab toRename, string? newName)
        : base(
            (t) => t.Tabs.ToArray(),
            (v, a) => v.ReOrderTabs(a),
            tab => tab.Text.ToString() ?? "unnamed tab",
            (tab, n) => tab.Text = n,
            design,
            toRename,
            newName)
    {
    }
}
