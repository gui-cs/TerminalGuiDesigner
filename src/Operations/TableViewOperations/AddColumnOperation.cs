using System.Data;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Adds a new <see cref="DataColumn"/> to a <see cref="DataTable"/> hosted in a <see cref="TableView"/>.
/// </summary>
public class AddColumnOperation : AddOperation<TableView, DataColumn>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddColumnOperation"/> class.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="design">Wrapper for a <see cref="TableView"/>.</param>
    /// <param name="newColumnName">The name for the new column or null to prompt at runtime with a <see cref="Modals"/> dialog.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> is not wrapping a <see cref="TableView"/>.</exception>
    public AddColumnOperation(IApplication app, Design design, string? newColumnName)
        : base(
            app,
            (v) => v.GetDataTable().Columns.Cast<DataColumn>().ToArray(),
            (v, a) => v.ReOrderColumns(a),
            (c) => c.ColumnName,
            (v, n) => { return v.GetDataTable()?.Columns.Add(n) ?? throw new Exception("TableView Table had not been initialized at the time we were asked to construct a new DataColumn for it."); },
            design,
            newColumnName)
    {
    }

    /// <inheritdoc/>
    protected override void SetNeedsDraw()
    {
        this.View.Update();
        base.SetNeedsDraw();
    }
}
