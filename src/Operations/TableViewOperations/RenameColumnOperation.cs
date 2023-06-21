using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Renames a <see cref="DataColumn"/> in a <see cref="TableView"/>.
/// </summary>
public class RenameColumnOperation : RenameOperation<TableView, DataColumn>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameColumnOperation"/> class.
    /// </summary>
    /// <param name="design">The <see cref="Design"/> wrapper for a <see cref="TableView"/>.</param>
    /// <param name="column">The column to rename.</param>
    /// <param name="newName">New name to use or null to prompt user.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TableView"/>.</exception>
    public RenameColumnOperation(Design design, DataColumn column, string? newName)
        : base(
            (v) => v.GetDataTable().Columns.Cast<DataColumn>().ToArray(),
            (v, a) => v.ReOrderColumns(a),
            (c) => c.ColumnName,
            (c, name) => c.ColumnName = name,
            design,
            column,
            newName)
    {
    }
}
