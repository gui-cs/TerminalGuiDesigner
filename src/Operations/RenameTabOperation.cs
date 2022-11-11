using NStack;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner;

/// <summary>
/// Renames the <see cref="TabView.Tab.Text"/> of the currently selected
/// <see cref="TabView.Tab"/> of a <see cref="TabView"/>.
/// </summary>
internal class RenameTabOperation : Operation
{
    private readonly TabView tabView;
    private Tab? tab;
    private readonly ustring? originalName;
    private string? newTabName;

    public Design Design { get; }

    public RenameTabOperation(Design design)
    {
        this.Design = design;

        // somehow user ran this command for a non tabview
        if (this.Design.View is not TabView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TabView)} to support {nameof(RenameTabOperation)}");
        }

        this.tabView = (TabView)this.Design.View;

        this.tab = this.tabView.SelectedTab;

        // user has no Tab selected
        if (this.tab == null)
        {
            this.IsImpossible = true;
        }

        this.originalName = this.tab?.Text;
    }

    public override string ToString()
    {
        return $"Rename Tab '{this.originalName}'";
    }

    protected override bool DoImpl()
    {
        if (this.tab == null)
        {
            throw new Exception("No tab was selected so command cannot be run");
        }

        if (Modals.GetString("Rename Tab", "Tab Name", this.originalName?.ToString(), out string? newTabName) && newTabName != null)
        {
            this.newTabName = newTabName;
            this.tab.Text = newTabName;
            this.tabView.SetNeedsDisplay();
            return true;
        }

        return false;
    }

    public override void Redo()
    {
        if (this.tab != null)
        {
            this.tab.Text = this.newTabName;
            this.tabView.SetNeedsDisplay();
        }
    }

    public override void Undo()
    {
        if (this.tab != null)
        {
            this.tab.Text = this.originalName;
            this.tabView.SetNeedsDisplay();
        }
    }
}
