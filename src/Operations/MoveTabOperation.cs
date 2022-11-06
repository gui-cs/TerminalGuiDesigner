using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveTabOperation : TabViewOperation
{
    public int Adjustment { get; }

    /// <summary>
    /// Creates an operation that will change the ordering of tabs within
    /// a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design"></param>
    /// <param name="adjustment">Negative to move tab left, positive to move tab right</param>
    public MoveTabOperation(Design design, int adjustment)
        : base(design)
    {
        if (this.SelectedTab == null)
        {
            this.IsImpossible = true;
        }

        // they are moving it nowhere?!
        if (adjustment == 0)
        {
            this.IsImpossible = true;
        }

        this.Adjustment = adjustment;
    }

    public override string ToString()
    {
        if (this.Adjustment == 0 || this.SelectedTab == null)
        {
            return $"Bad Command '{this.GetType().Name}'";
        }

        if (this.Adjustment < 0)
        {
            return $"Move '{this.SelectedTab.Text}' Left";
        }

        if (this.Adjustment > 0)
        {
            return $"Move '{this.SelectedTab.Text}' Right";
        }

        return base.ToString();
    }

    public override bool Do()
    {
        return this.ApplyAdjustment(this.Adjustment);
    }

    public override void Redo()
    {
        this.Do();
    }

    public override void Undo()
    {
        this.ApplyAdjustment(-this.Adjustment);
    }

    private bool ApplyAdjustment(int adjustment)
    {
        var originalIdx = this.View.Tabs.ToList().IndexOf(this.SelectedTab);

        if (this.SelectedTab == null || originalIdx == -1)
        {
            return false;
        }

        var newIndex = Math.Max(0, Math.Min(this.View.Tabs.Count - 1, originalIdx + adjustment));

        // if we would end up putting it back where it was then abandon this operation
        if (originalIdx == newIndex)
        {
            return false;
        }

        this.View.InsertTab(newIndex, this.SelectedTab);
        return true;
    }
}
