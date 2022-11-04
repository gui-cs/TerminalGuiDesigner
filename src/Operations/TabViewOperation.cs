using Terminal.Gui;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

public abstract class TabViewOperation : Operation
{
    protected readonly TabView View;

    public Design Design { get; }

    /// <summary>
    /// The <see cref="Tab"/> that was selected when the operation
    /// was constructed.  This should be what you operate on if you
    /// are removing or reordering etc the tabs
    /// </summary>
    public Tab? SelectedTab { get; }

    public TabViewOperation(Design design)
    {
        this.Design = design;

        // somehow user ran this command for a non tab view
        if (this.Design.View is not TabView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TabView)} to support this Operation");
        }

        this.View = (TabView)this.Design.View;

        this.SelectedTab = this.View.SelectedTab;
    }
}
