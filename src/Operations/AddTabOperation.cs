using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Adds a new tab to a <see cref="TabView"/>.
/// </summary>
public class AddTabOperation : TabViewOperation
{
    private readonly string? name;
    private Tab? tab;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTabOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for <see cref="TabView"/> that will be operated on.</param>
    /// <param name="name">Name for the new tab or null to prompt user.</param>
    public AddTabOperation(Design design, string? name)
        : base(design)
    {
        this.name = name;
    }

    /// <inheritdoc/>
    public override bool Do()
    {
        if (this.tab != null)
        {
            return false;
        }

        var newName = this.name;

        if (newName == null)
        {
            if (!Modals.GetString("Add Tab", "Tab Name", "MyTab", out newName))
            {
                return false;
            }
        }

        if (newName == null)
        {
            return false;
        }

        newName = newName.MakeUnique(
            this.View.Tabs.Select(t => t.Text.ToString())
            .Where(v => v != null)
            .Cast<string>());

        this.View.AddTab(this.tab = new Tab(newName, new View { Width = Dim.Fill(), Height = Dim.Fill() }), true);
        this.View.SetNeedsDisplay();

        return true;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Undo()
    {
        // cannot undo if operation hasn't been performed
        if (this.tab == null || !this.View.Tabs.Contains(this.tab))
        {
            return;
        }

        this.View.RemoveTab(this.tab);
        this.View.SetNeedsDisplay();
    }
}
