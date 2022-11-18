using Terminal.Gui;
using TerminalGuiDesigner;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Moves a <see cref="TabView.Tab"/> left or right within the ordering
/// of tabs in a <see cref="TabView"/>.
/// </summary>
public class MoveTabOperation : TabViewOperation
{
    /// <summary>
    /// Gets the number of index positions the <see cref="TabView.Tab"/> will
    /// be moved. Negative for left, positive for right.
    /// </summary>
    private readonly int adjustment;
    private readonly int originalIdx;
    private readonly int newIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveTabOperation"/> class.
    /// Creates an operation that will change the ordering of tabs within
    /// a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/>.</param>
    /// <param name="adjustment">Negative to move tab left, positive to move tab right.</param>
    public MoveTabOperation(Design design, int adjustment)
        : base(design)
    {
        if (this.SelectedTab == null)
        {
            this.IsImpossible = true;
        }

        this.originalIdx = this.View.Tabs.ToList().IndexOf(this.SelectedTab);

        if (this.originalIdx == -1)
        {
            this.IsImpossible = true;
            return;
        }

        // calculate new index without falling off array
        this.newIndex = Math.Max(0, Math.Min(this.View.Tabs.Count - 1, this.originalIdx + adjustment));

        // they are moving it nowhere?!
        if (this.newIndex == this.originalIdx)
        {
            this.IsImpossible = true;
        }

        this.adjustment = adjustment;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this.adjustment == 0 || this.SelectedTab == null)
        {
            return $"Bad Command '{this.GetType().Name}'";
        }

        if (this.adjustment < 0)
        {
            return $"Move '{this.SelectedTab.Text}' Left";
        }

        if (this.adjustment > 0)
        {
            return $"Move '{this.SelectedTab.Text}' Right";
        }

        return base.ToString();
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
            return;
        }

        this.View.InsertTab(this.originalIdx, this.SelectedTab);
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.SelectedTab == null)
        {
            return false;
        }

        this.View.InsertTab(this.newIndex, this.SelectedTab);
        return true;
    }
}
