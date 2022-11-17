using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Removes a <see cref="DataColumn"/> from a <see cref="TableView"/>.
/// Cannot be used to remove the last column.
/// </summary>
public class RemoveColumnOperation : ColumnOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveColumnOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TableView"/>.</param>
    /// <param name="column">Column to remove.</param>
    public RemoveColumnOperation(Design design, DataColumn column)
        : base(design, column)
    {
        // TODO: currently this crashes TableView in its ReDraw (calculate view port) method
        // don't let them remove the last column
        if (TableView.Table.Columns.Count == 1)
        {
            IsImpossible = true;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Remove Column '{Column}'";
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (!TableView.Table.Columns.Contains(Column.ColumnName))
        {
            TableView.Table.Columns.Add(Column.ColumnName);
            TableView.Update();
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (TableView.Table.Columns.Contains(Column.ColumnName))
        {
            TableView.Table.Columns.Remove(Column.ColumnName);
            TableView.Update();

            return true;
        }

        return false;
    }
}