using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Removes a <see cref="DataColumn"/> from a <see cref="TableView"/>.
/// Cannot be used to remove the last column.
/// </summary>
public class RemoveColumnOperation : Operation
{
    private TableView tableView;
    private DataColumn? column;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveColumnOperation"/> class.
    /// </summary>
    /// <param name="design"><see cref="Design"/> wrapper for a <see cref="TableView"/> that you want
    /// to remove a <see cref="DataColumn"/> from.</param>
    /// <param name="column">The column to remove.  Must belong to <see cref="TableView.Table"/> of <paramref name="design"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TableView"/>.</exception>
    public RemoveColumnOperation(Design design, DataColumn? column)
    {
        this.Design = design;

        // somehow user ran this command for a non table view
        if (this.Design.View is not TableView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TableView)} to support {nameof(RemoveColumnOperation)}");
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

        // TODO: currently this crashes TableView in its ReDraw (calculate viewport) metho
        // don't let them remove the last column
        if (this.tableView.Table.Columns.Count == 1)
        {
            this.IsImpossible = true;
        }
    }

    /// <summary>
    /// Gets the wrapper for a <see cref="TableView"/> which will be operated on.
    /// </summary>
    public Design Design { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Remove Column '{this.column}'";
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (this.column == null)
        {
            throw new Exception("No column selected");
        }

        if (!this.tableView.Table.Columns.Contains(this.column.ColumnName))
        {
            this.tableView.Table.Columns.Add(this.column.ColumnName);
            this.tableView.Update();
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.column == null)
        {
            throw new Exception("No column selected");
        }

        if (this.tableView.Table.Columns.Contains(this.column.ColumnName))
        {
            this.tableView.Table.Columns.Remove(this.column.ColumnName);
            this.tableView.Update();

            return true;
        }

        return false;
    }

}
