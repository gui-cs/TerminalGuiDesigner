using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

internal class AddColumnOperation : Operation
{
    private DataColumn? _column;
    private TableView _tableView;

    public Design Design { get; }

    public AddColumnOperation(Design design)
    {
        Design = design;

        // somehow user ran this command for a non table view
        if (Design.View is not TableView)
            throw new ArgumentException($"Design must be for a {nameof(TableView)} to support {nameof(AddColumnOperation)}");

        _tableView = (TableView)Design.View;
    }

    public override bool Do()
    {
        if (_column != null)
        {
            throw new Exception("This command has already been performed once.  Use Redo instead of Do");
        }

        if (Modals.GetString("Add Column", "Column Name", "MyCol", out var newColumnName))
        {
            _column = _tableView.Table.Columns.Add(newColumnName);
            _tableView.Update();
        }
        
        return true;
    }

    public override void Redo()
    {
        // cannot redo (maybe user hit redo twice thats fine)
        if (_column == null || _tableView.Table.Columns.Contains(_column.ColumnName))
        {
            return;
        }

        _tableView.Table.Columns.Add(_column);
        _tableView.Update();

    }

    public override void Undo()
    {
        // cannot undo
        if (_column == null || !_tableView.Table.Columns.Contains(_column.ColumnName))
        {
            return;
        }

        _tableView.Table.Columns.Remove(_column);
        _tableView.Update();
    }
}
