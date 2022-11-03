using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class RemoveTabOperation : TabViewOperation
{
    public int RemovedAtIdx { get; private set; }

    public RemoveTabOperation(Design design) : base(design)
    {
        // user has no Tab selected
        if (SelectedTab == null)
            IsImpossible = true;
    }
    public override string ToString()
    {
        return $"Remove Tab '{SelectedTab?.Text}'";
    }
    public override bool Do()
    {
        if (SelectedTab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (View.Tabs.Contains(SelectedTab))
        {
            RemovedAtIdx = View.Tabs.ToList().IndexOf(SelectedTab);
            View.RemoveTab(SelectedTab);
            return true;
        }

        return false;
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        if (SelectedTab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (!View.Tabs.Contains(SelectedTab))
        {
            View.InsertTab(RemovedAtIdx,SelectedTab);
        }
    }
}
