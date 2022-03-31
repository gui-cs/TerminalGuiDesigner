using Terminal.Gui;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

public class RemoveTabOperation : Operation
{
    private readonly Tab? _tab;
    private readonly TabView _tabView;

    public Design Design { get; }

    public RemoveTabOperation(Design design)
    {
        Design = design;

        // somehow user ran this command for a non TabView
        if (Design.View is not TabView)
            throw new ArgumentException($"Design must be for a {nameof(TabView)} to support {nameof(RemoveTabOperation)}");

        _tabView = (TabView)Design.View;

        _tab = _tabView.SelectedTab;

        // user has no Tab selected
        if (_tab == null)
            IsImpossible = true;
    }
    public override string ToString()
    {
        return $"Remove Tab '{_tab?.Text}'";
    }
    public override void Do()
    {
        if (_tab == null)
        {
            throw new Exception("No Tab selected");
        }

        if (_tabView.Tabs.Contains(_tab))
        {
            _tabView.RemoveTab(_tab);
        }
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

        if (!_tabView.Tabs.Contains(_tab))
        {
            _tabView.AddTab(_tab,true);
        }
    }
}
