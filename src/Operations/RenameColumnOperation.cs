using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner;

internal class RenameColumnOperation : Operation
{
    private TableView _tableView;
    private DataColumn? _column;
    private string? _originalName;
    private string? _newColumnName;

    public Design Design { get; }

    public RenameColumnOperation(Design design, DataColumn? column)
    {
        Design = design;

        // somehow user ran this command for a non table view
        if (Design.View is not TableView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TableView)} to support {nameof(AddColumnOperation)}");
        }

        _tableView = (TableView)Design.View;

        // user specified column, or the currently selected cell in the table
        _column = column ??
            (_tableView.SelectedColumn < 0 ? null : _tableView.Table.Columns[_tableView.SelectedColumn]);

        // user has no column selected
        if (_column == null)
        {
            IsImpossible = true;
        }

        _originalName = _column?.ColumnName;
    }

    public override string ToString()
    {
        return $"Rename Column '{_originalName}'";
    }

    public override bool Do()
    {
        if (_column == null)
        {
            throw new Exception("No column was selected so command cannot be run");
        }

        if (Modals.GetString("Rename Column", "Column Name", _originalName, out var newColumnName))
        {
            _newColumnName = newColumnName;
            _column.ColumnName = newColumnName;
            _tableView.Update();
            return true;
        }

        return false;
    }

    public override void Redo()
    {
        if (_column != null)
        {
            _column.ColumnName = _newColumnName;
            _tableView.Update();
        }
    }

    public override void Undo()
    {
        if (_column != null)
        {
            _column.ColumnName = _originalName;
            _tableView.Update();
        }
    }
}
