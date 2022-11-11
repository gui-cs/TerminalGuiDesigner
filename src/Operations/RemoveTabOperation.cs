using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Removes (deletes) a <see cref="TabView.Tab"/> from a <see cref="TabView"/>.
/// </summary>
public class RemoveTabOperation : TabViewOperation
{
    /// <summary>
    /// Gets the original location (index) of the tab that is being removed.
    /// </summary>
    private int removedAtIdx;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveTabOperation"/> class.
    /// Removes the <see cref="TabView.SelectedTab"/> from a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/> from which you want to remove the tab.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TabView"/>.</exception>
    public RemoveTabOperation(Design design)
        : base(design)
    {
        // user has no Tab selected
        if (this.SelectedTab == null)
        {
            this.IsImpossible = true;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Remove Tab '{this.SelectedTab?.Text}'";
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (this.SelectedTab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (!this.View.Tabs.Contains(this.SelectedTab))
        {
            this.View.InsertTab(this.removedAtIdx, this.SelectedTab);
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.SelectedTab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (this.View.Tabs.Contains(this.SelectedTab))
        {
            this.removedAtIdx = this.View.Tabs.ToList().IndexOf(this.SelectedTab);
            this.View.RemoveTab(this.SelectedTab);
            return true;
        }

        return false;
    }
}
