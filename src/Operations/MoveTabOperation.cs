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
    public MoveTabOperation(Design design, int adjustment) : base(design)
    {
        if (SelectedTab == null)
            IsImpossible = true;

        // they are moving it nowhere?!
        if (adjustment == 0)
            IsImpossible = true;

        Adjustment = adjustment;
    }
    public override string ToString()
    {
        if (Adjustment == 0 || SelectedTab == null)
            return $"Bad Command '{GetType().Name}'";

        if (Adjustment < 0)
            return $"Move '{SelectedTab.Text}' Left";

        if (Adjustment > 0)
            return $"Move '{SelectedTab.Text}' Right";

        return base.ToString();
    }
    public override bool Do()
    {
        return ApplyAdjustment(Adjustment);
    }
    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        ApplyAdjustment(-Adjustment);
    }

    private bool ApplyAdjustment(int adjustment)
    {
        var originalIdx = View.Tabs.ToList().IndexOf(SelectedTab);
        
        if (SelectedTab == null || originalIdx == -1)
            return false;

        var list = View.Tabs.ToList();

        var newIndex = Math.Min(Math.Max(0, originalIdx + adjustment), list.Count - 1);

        // if we would end up putting it back where it was then abandon this operation
        if (originalIdx == newIndex)
            return false;

        list.Remove(SelectedTab);
        list.Insert(newIndex, SelectedTab);

        // View.Tabs is readonly so we have to
        // remove all the tabs
        foreach (var t in View.Tabs.ToArray())
        {
            View.RemoveTab(t);
        }

        // then add them back in again in the new order
        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];

            // put all the tabs back in again and select
            // the tab in it's new position
            View.AddTab(t, i == newIndex);
        }

        return true;
    }
}
