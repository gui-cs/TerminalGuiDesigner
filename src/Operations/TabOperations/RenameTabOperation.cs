using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Renames the <see cref="Tab.DisplayText"/> of the currently selected
/// <see cref="Tab"/> of a <see cref="TabView"/>.
/// </summary>
public class RenameTabOperation : RenameOperation<TabView, Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameTabOperation"/> class.
    /// This command changes the <see cref="Tab.DisplayText"/> on a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/>.</param>
    /// <param name="toRename">Tab to rename.</param>
    /// <param name="newName">New name to use or null to prompt.</param>
    public RenameTabOperation(Design design, Tab toRename, string? newName)
        : base(
            (t) => t.Tabs.ToArray(),
            (v, a) => v.ReOrderTabs(a),
            tab => tab.DisplayText.ToString() ?? "unnamed tab",
            (tab, n) => tab.DisplayText = n,
            design,
            toRename,
            newName)
    {
    }
}
