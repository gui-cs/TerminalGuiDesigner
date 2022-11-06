using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

internal class AddColumnOperation : Operation
{
    private DataColumn? column;
    private TableView tableView;

    public Design Design { get; }

    public AddColumnOperation(Design design)
    {
        this.Design = design;

        // somehow user ran this command for a non table view
        if (this.Design.View is not TableView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TableView)} to support {nameof(AddColumnOperation)}");
        }

        this.tableView = (TableView)this.Design.View;
    }

    public override bool Do()
    {
        if (this.column != null)
        {
            throw new Exception("This command has already been performed once.  Use Redo instead of Do");
        }

        if (Modals.GetString("Add Column", "Column Name", "MyCol", out var newColumnName))
        {
            this.column = this.tableView.Table.Columns.Add(newColumnName);
            this.tableView.Update();
        }

        return true;
    }

    public override void Redo()
    {
        // cannot redo (maybe user hit redo twice thats fine)
        if (this.column == null || this.tableView.Table.Columns.Contains(this.column.ColumnName))
        {
            return;
        }

        this.tableView.Table.Columns.Add(this.column);
        this.tableView.Update();
    }

    public override void Undo()
    {
        // cannot undo
        if (this.column == null || !this.tableView.Table.Columns.Contains(this.column.ColumnName))
        {
            return;
        }

        this.tableView.Table.Columns.Remove(this.column);
        this.tableView.Update();
    }
}
