using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class RemoveTabOperation : TabViewOperation
{
    public int RemovedAtIdx { get; private set; }

    public RemoveTabOperation(Design design)
        : base(design)
    {
        // user has no Tab selected
        if (this.SelectedTab == null)
        {
            this.IsImpossible = true;
        }
    }

    public override string ToString()
    {
        return $"Remove Tab '{this.SelectedTab?.Text}'";
    }

    protected override bool DoImpl()
    {
        if (this.SelectedTab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (this.View.Tabs.Contains(this.SelectedTab))
        {
            this.RemovedAtIdx = this.View.Tabs.ToList().IndexOf(this.SelectedTab);
            this.View.RemoveTab(this.SelectedTab);
            return true;
        }

        return false;
    }

    public override void Redo()
    {
        this.Do();
    }

    public override void Undo()
    {
        if (this.SelectedTab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (!this.View.Tabs.Contains(this.SelectedTab))
        {
            this.View.InsertTab(this.RemovedAtIdx, this.SelectedTab);
        }
    }
}
