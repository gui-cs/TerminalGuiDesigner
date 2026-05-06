using System.Data;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;
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
    /// <param name="app">The application instance.</param>
    /// <param name="design">The <see cref="Design"/> wrapper for a <see cref="TableView"/>.</param>
    /// <param name="column">The column to rename.</param>
    /// <param name="newName">New name to use or null to prompt user.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TableView"/>.</exception>
    public RenameColumnOperation(IApplication app, Design design, DataColumn column, string? newName)
        : base(
            app,
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
