using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Adds a new tab to a <see cref="TabView"/>.
/// </summary>
public class AddTabOperation : AddOperation<TabView, Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddTabOperation"/> class.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="design">Wrapper for <see cref="TabView"/> that will be operated on.</param>
    /// <param name="name">Name for the new tab or null to prompt user.</param>
    public AddTabOperation(IApplication app, Design design, string? name)
         : base(
            app,
            (t) => t.Tabs.ToArray(),
            (v, a) => v.ReOrderTabs(a),
            tab => tab.DisplayText.ToString() ?? "unnamed tab",
            AddTab,
            design,
            name)
    {
    }

    private static Tab AddTab(TabView view, string name)
    {
        var tab = new Tab()
        {
            DisplayText = name,
            View = new View { Width = Dim.Fill(), Height = Dim.Fill() }
        };
        view.AddTab(tab, true);
        return tab;
    }
}
