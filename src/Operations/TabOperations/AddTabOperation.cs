using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Adds a new tab to a <see cref="TabView"/>.
/// </summary>
public class AddTabOperation : AddOperation<TabView, Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddTabOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for <see cref="TabView"/> that will be operated on.</param>
    /// <param name="name">Name for the new tab or null to prompt user.</param>
    public AddTabOperation(Design design, string? name)
         : base(
            (t) => t.Tabs.ToArray(),
            (v, a) => v.ReOrderTabs(a),
            tab => tab.Text.ToString() ?? "unnamed tab",
            AddTab,
            design,
            name)
    {
    }

    private static Tab AddTab(TabView view, string name)
    {
        var tab = new Tab(name, new View { Width = Dim.Fill(), Height = Dim.Fill() });
        view.AddTab(tab, true);
        return tab;
    }
}
