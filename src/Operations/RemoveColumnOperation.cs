using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

internal class RemoveColumnOperation : Operation
{
    private TableView _tableView;
    private DataColumn? _column;

    public Design Design { get; }

    public RemoveColumnOperation(Design design, DataColumn? column)
    {
        Design = design;

        // somehow user ran this command for a non table view
        if (Design.View is not TableView)
            throw new ArgumentException($"Design must be for a {nameof(TableView)} to support {nameof(RemoveColumnOperation)}");

        _tableView = (TableView)Design.View;

        // user specified column, or the currently selected cell in the table
        _column = column ??
            (_tableView.SelectedColumn < 0 ? null : _tableView.Table.Columns[_tableView.SelectedColumn]);

        // user has no column selected
        if (_column == null)
            IsImpossible = true;

        // TODO: currently this crashes TableView in its ReDraw (calculate viewport) metho
        // don't let them remove the last column
        if (_tableView.Table.Columns.Count == 1)
            IsImpossible = true;
    }
    public override string ToString()
    {
        return $"Remove Column '{_column}'";
    }
    public override void Do()
    {
        if (_column == null)
        {
            throw new Exception("No column selected");
        }

        if (_tableView.Table.Columns.Contains(_column.ColumnName))
        {
            _tableView.Table.Columns.Remove(_column.ColumnName);
            _tableView.Update();
        }
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        if (_column == null)
        {
            throw new Exception("No column selected");
        }

        if (!_tableView.Table.Columns.Contains(_column.ColumnName))
        {
            _tableView.Table.Columns.Add(_column.ColumnName);
            _tableView.Update();
        }
    }
}
