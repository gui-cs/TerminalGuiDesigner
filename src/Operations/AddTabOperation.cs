using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

public class AddTabOperation : TabViewOperation
{
    private Tab? _tab;

    public AddTabOperation(Design design) : base(design)
    {
    }

    public override bool Do()
    {
        if (_tab != null)
        {
            throw new Exception("This command has already been performed once.  Use Redo instead of Do");
        }

        if (Modals.GetString("Add Tab", "Tab Name", "MyTab", out string? newTabName))
        {
            View.AddTab(_tab = new Tab(newTabName ?? "Unamed Tab", new View { Width = Dim.Fill(), Height = Dim.Fill() }), true);
            View.SetNeedsDisplay();
        }

        return true;
    }

    public override void Redo()
    {
        // cannot redo (maybe user hit redo twice thats fine)
        if (_tab == null || View.Tabs.Contains(_tab))
        {
            return;
        }

        View.AddTab(_tab, true);
        View.SetNeedsDisplay();
    }

    public override void Undo()
    {
        // cannot undo
        if (_tab == null || !View.Tabs.Contains(_tab))
        {
            return;
        }

        View.RemoveTab(_tab);
        View.SetNeedsDisplay();
    }
}
