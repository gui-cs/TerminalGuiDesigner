using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

public class AddTabOperation : TabViewOperation
{
    private Tab? tab;

    public AddTabOperation(Design design)
        : base(design)
    {
    }

    public override bool Do()
    {
        if (this.tab != null)
        {
            throw new Exception("This command has already been performed once.  Use Redo instead of Do");
        }

        if (Modals.GetString("Add Tab", "Tab Name", "MyTab", out string? newTabName))
        {
            this.View.AddTab(this.tab = new Tab(newTabName ?? "Unamed Tab", new View { Width = Dim.Fill(), Height = Dim.Fill() }), true);
            this.View.SetNeedsDisplay();
        }

        return true;
    }

    public override void Redo()
    {
        // cannot redo (maybe user hit redo twice thats fine)
        if (this.tab == null || this.View.Tabs.Contains(this.tab))
        {
            return;
        }

        this.View.AddTab(this.tab, true);
        this.View.SetNeedsDisplay();
    }

    public override void Undo()
    {
        // cannot undo
        if (this.tab == null || !this.View.Tabs.Contains(this.tab))
        {
            return;
        }

        this.View.RemoveTab(this.tab);
        this.View.SetNeedsDisplay();
    }
}
