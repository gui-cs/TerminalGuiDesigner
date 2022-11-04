using NStack;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner;

internal class RenameTabOperation : Operation
{
    private readonly TabView _tabView;
    private Tab? _tab;
    private readonly ustring? _originalName;
    private string? _newTabName;

    public Design Design { get; }

    public RenameTabOperation(Design design)
    {
        Design = design;

        // somehow user ran this command for a non tabview
        if (Design.View is not TabView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TabView)} to support {nameof(AddTabOperation)}");
        }

        _tabView = (TabView)Design.View;

        _tab = _tabView.SelectedTab;

        // user has no Tab selected
        if (_tab == null)
        {
            IsImpossible = true;
        }

        _originalName = _tab?.Text;
    }

    public override string ToString()
    {
        return $"Rename Tab '{_originalName}'";
    }

    public override bool Do()
    {
        if (_tab == null)
        {
            throw new Exception("No tab was selected so command cannot be run");
        }

        if (Modals.GetString("Rename Tab", "Tab Name", _originalName?.ToString(), out string? newTabName) && newTabName != null)
        {
            _newTabName = newTabName;
            _tab.Text = newTabName;
            _tabView.SetNeedsDisplay();
            return true;
        }

        return false;
    }

    public override void Redo()
    {
        if (_tab != null)
        {
            _tab.Text = _newTabName;
            _tabView.SetNeedsDisplay();
        }
    }

    public override void Undo()
    {
        if (_tab != null)
        {
            _tab.Text = _originalName;
            _tabView.SetNeedsDisplay();
        }
    }
}
