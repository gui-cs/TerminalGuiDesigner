using NStack;
using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.TabOperations;

/// <summary>
/// Renames the <see cref="TabView.Tab.Text"/> of the currently selected
/// <see cref="TabView.Tab"/> of a <see cref="TabView"/>.
/// </summary>
public class RenameTabOperation : TabViewOperation
{
    private readonly ustring? originalName;
    private string? newTabName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameTabOperation"/> class.
    /// This command changes the <see cref="TabView.Tab.Text"/> on a <see cref="TabView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TabView"/>.</param>
    public RenameTabOperation(Design design)
        : base(design)
    {
        // user has no Tab selected
        if (this.SelectedTab == null)
        {
            this.IsImpossible = true;
            return;
        }

        this.originalName = this.SelectedTab.Text;
        this.Category = this.SelectedTab?.Text.ToString() ?? Operation.Unnamed;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Rename Tab '{this.originalName}'";
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        if (this.SelectedTab != null)
        {
            this.SelectedTab.Text = this.newTabName;
            this.View.SetNeedsDisplay();
        }
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (this.SelectedTab != null)
        {
            this.SelectedTab.Text = this.originalName;
            this.View.SetNeedsDisplay();
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.SelectedTab == null)
        {
            throw new Exception("No tab was selected so command cannot be run");
        }

        if (Modals.GetString("Rename Tab", "Tab Name", this.originalName?.ToString(), out string? newTabName) && newTabName != null)
        {
            this.newTabName = newTabName;
            this.SelectedTab.Text = newTabName;
            this.View.SetNeedsDisplay();
            return true;
        }

        return false;
    }
}
