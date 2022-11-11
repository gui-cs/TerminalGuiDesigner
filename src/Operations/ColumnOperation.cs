using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Abstract base class for <see cref="Operation"/> which affect a <see cref="DataColumn"/> in a <see cref="TableView"/>.
/// </summary>
public abstract class ColumnOperation : Operation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnOperation"/> class.
    /// </summary>
    /// <param name="design"><see cref="Design"/> wrapper for a <see cref="TableView"/> that you want
    /// to operate on.</param>
    /// <param name="column">The column to operate on.  Must belong to <see cref="TableView.Table"/> of <paramref name="design"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TableView"/>.</exception>
    protected ColumnOperation(Design design, DataColumn column)
    {
        this.Design = design;
        this.Column = column;

        // somehow user ran this command for a non table view
        if (this.Design.View is not Terminal.Gui.TableView)
        {
            throw new ArgumentException($"Design must be for a {nameof(Terminal.Gui.TableView)} to support {nameof(ColumnOperation)}");
        }

        this.TableView = (TableView)this.Design.View;
    }

    /// <summary>
    /// Gets the <see cref="TableView"/> which will be operated on (same as <see cref="Design.View"/>).
    /// </summary>
    protected TableView TableView { get; }

    /// <summary>
    /// Gets the wrapper for a <see cref="TableView"/> which will be operated on.
    /// </summary>
    protected Design Design { get; }

    /// <summary>
    /// Gets the column that will be operated on.
    /// </summary>
    protected DataColumn Column { get; }
}