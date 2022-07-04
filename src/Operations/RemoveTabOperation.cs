using Terminal.Gui;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

public class RemoveTabOperation : TabViewOperation
{
    private readonly Tab? _tab;

    public RemoveTabOperation(Design design) : base(design)
    {
        // user has no Tab selected
        if (_tab == null)
            IsImpossible = true;
    }
    public override string ToString()
    {
        return $"Remove Tab '{_tab?.Text}'";
    }
    public override bool Do()
    {
        if (_tab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (View.Tabs.Contains(_tab))
        {
            View.RemoveTab(_tab);
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
        if (_tab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (!View.Tabs.Contains(_tab))
        {
            View.AddTab(_tab,true);
        }
    }
}
