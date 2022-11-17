using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Adds a new <see cref="DataColumn"/> to a <see cref="DataTable"/> hosted in a <see cref="TableView"/>.
/// </summary>
public class AddColumnOperation : Operation
{
    private readonly string? newColumnName;
    private DataColumn? column;
    private TableView tableView;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddColumnOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TableView"/>.</param>
    /// <param name="newColumnName">The name for the new column or null to prompt at runtime with a <see cref="Modals"/> dialog.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> is not wrapping a <see cref="TableView"/>.</exception>
    public AddColumnOperation(Design design, string? newColumnName)
    {
        Design = design;
        this.newColumnName = newColumnName;

        // somehow user ran this command for a non table view
        if (Design.View is not TableView)
        {
            throw new ArgumentException($"Design must be for a {nameof(TableView)} to support {nameof(AddColumnOperation)}");
        }

        tableView = (TableView)Design.View;
    }

    /// <summary>
    /// Gets the <see cref="TableView"/> wrapper <see cref="Design"/> that will be operated on.
    /// </summary>
    public Design Design { get; }

    /// <inheritdoc/>
    public override void Redo()
    {
        // cannot redo (maybe user hit redo twice thats fine)
        if (column == null || tableView.Table.Columns.Contains(column.ColumnName))
        {
            return;
        }

        tableView.Table.Columns.Add(column);
        tableView.Update();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        // cannot undo
        if (column == null || !tableView.Table.Columns.Contains(column.ColumnName))
        {
            return;
        }

        tableView.Table.Columns.Remove(column);
        tableView.Update();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (column != null)
        {
            throw new Exception("This command has already been performed once.  Use Redo instead of Do");
        }

        string? name = newColumnName;

        if (name == null)
        {
            if (!Modals.GetString("Add Column", "Column Name", "MyCol", out name))
            {
                // user canceled adding the column
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            // user canceled adding the column
            return false;
        }

        name = name.MakeUnique(tableView.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

        // Add the new column
        column = tableView.Table.Columns.Add(name);
        tableView.Update();

        return true;
    }
}
