using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

public class AddTabOperation : Operation
{
    private Tab _tab;
    private TabView _tabView;

    public Design Design { get; }

    public AddTabOperation(Design design)
    {
        Design = design;

        // somehow user ran this command for a non table view
        if (Design.View is not TabView)
            throw new ArgumentException($"Design must be for a {nameof(TabView)} to support {nameof(AddTabOperation)}");

        _tabView = (TabView)Design.View;
    }

    public override void Do()
    {
        if (_tab != null)
        {
            throw new Exception("This command has already been performed once.  Use Redo instead of Do");
        }

        if (Modals.GetString("Add Tab", "Tab Name", "MyTab", out string newTabName))
        {
            _tabView.AddTab(_tab = new Tab(newTabName,null),true);
            _tabView.SetNeedsDisplay();
        }
    }

    public override void Redo()
    {
        // cannot redo (maybe user hit redo twice thats fine)
        if (_tab == null || _tabView.Tabs.Contains(_tab))
        {
            return;
        }

        _tabView.AddTab(_tab,true);
        _tabView.SetNeedsDisplay();
    }

    public override void Undo()
    {
        // cannot undo
        if (_tab == null || !_tabView.Tabs.Contains(_tab))
        {
            return;
        }

        _tabView.RemoveTab(_tab);
        _tabView.SetNeedsDisplay();
    }
}
