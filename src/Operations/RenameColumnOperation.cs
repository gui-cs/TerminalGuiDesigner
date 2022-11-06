using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner;

internal class RenameColumnOperation : Operation
{
    private TableView tableView;
    private DataColumn? column;
    private string? originalName;
    private string? newColumnName;

    public Design Design { get; }

    public RenameColumnOperation(Design design, DataColumn? column)
    {
        this.Design = design;

        // somehow user ran this command for a non table view
        if (this.Design.View is not TableView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TableView)} to support {nameof(AddColumnOperation)}");
        }

        this.tableView = (TableView)this.Design.View;

        // user specified column, or the currently selected cell in the table
        this.column = column ??
            (this.tableView.SelectedColumn < 0 ? null : this.tableView.Table.Columns[this.tableView.SelectedColumn]);

        // user has no column selected
        if (this.column == null)
        {
            this.IsImpossible = true;
        }

        this.originalName = this.column?.ColumnName;
    }

    public override string ToString()
    {
        return $"Rename Column '{this.originalName}'";
    }

    public override bool Do()
    {
        if (this.column == null)
        {
            throw new Exception("No column was selected so command cannot be run");
        }

        if (Modals.GetString("Rename Column", "Column Name", this.originalName, out var newColumnName))
        {
            this.newColumnName = newColumnName;
            this.column.ColumnName = newColumnName;
            this.tableView.Update();
            return true;
        }

        return false;
    }

    public override void Redo()
    {
        if (this.column != null)
        {
            this.column.ColumnName = this.newColumnName;
            this.tableView.Update();
        }
    }

    public override void Undo()
    {
        if (this.column != null)
        {
            this.column.ColumnName = this.originalName;
            this.tableView.Update();
        }
    }
}
