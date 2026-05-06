using System.Data;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Removes a <see cref="DataColumn"/> from a <see cref="TableView"/>.
/// Cannot be used to remove the last column.
/// </summary>
public class RemoveColumnOperation : RemoveOperation<TableView, DataColumn>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveColumnOperation"/> class.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="design">Wrapper for a <see cref="TableView"/>.</param>
    /// <param name="column">Column to remove.</param>
    public RemoveColumnOperation(IApplication app, Design design, DataColumn column)
         : base(
            app,
            (v) => v.GetDataTable().Columns.Cast<DataColumn>().ToArray(),
            (v, a) => v.ReOrderColumns(a),
            (c) => c.ColumnName,
            design,
            column)
    {
        // TODO: currently this crashes TableView in its ReDraw (calculate view port) method
        // don't let them remove the last column
        if (this.View.GetDataTable().Columns.Count == 1)
        {
            this.IsImpossible = true;
        }
    }
}