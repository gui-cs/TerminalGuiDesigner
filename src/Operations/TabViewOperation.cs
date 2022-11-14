using Terminal.Gui;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Abstract base class for all <see cref="Operation"/> which operate
/// on the <see cref="TabView.SelectedTab"/> of a <see cref="TabView"/>.
/// </summary>
public abstract class TabViewOperation : Operation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabViewOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for <see cref="TabView"/> on which you want to operate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TabView"/>.</exception>
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

    /// <summary>
    /// Gets the <see cref="TabView.Tab"/> that was selected when the operation
    /// was constructed.  This should be what you operate on if you
    /// are removing or reordering etc the tabs.
    /// </summary>
    public Tab? SelectedTab { get; }

    /// <summary>
    /// Gets the <see cref="TabView"/> that will be operated on.
    /// </summary>
    protected TabView View { get; }

    /// <summary>
    /// Gets the <see cref="Design"/> wrapper for <see cref="View"/>.
    /// </summary>
    protected Design Design { get; }
}
