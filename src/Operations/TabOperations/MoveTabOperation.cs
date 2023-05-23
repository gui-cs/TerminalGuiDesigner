using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Moves a <see cref="Tab"/> left or right within the ordering
/// of tabs in a <see cref="TabView"/>.
/// </summary>
public class MoveTabOperation : MoveOperation<TabView, Tab>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoveTabOperation"/> class.
    /// Creates an operation that will change the ordering of tabs within
    /// a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/>.</param>
    /// <param name="toMove">The Tab to move.</param>
    /// <param name="adjustment">Negative to move tab left, positive to move tab right.</param>
    public MoveTabOperation(Design design, Tab toMove, int adjustment)
        : base(
            (t) => t.Tabs.ToArray(),
            (v, a) => v.ReOrderTabs(a),
            tab => tab.Text.ToString() ?? "unnamed tab",
            design,
            toMove,
            adjustment)
    {
    }
}
