using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Moves a <see cref="DataColumn"/> left or right within the ordering
/// of columns in a <see cref="TableView"/>.
/// </summary>
public class MoveColumnOperation : MoveOperation<TableView, DataColumn>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoveColumnOperation"/> class.
    /// Creates an operation that will change the ordering of columns within
    /// a <see cref="TableView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TableView"/>.</param>
    /// <param name="column">The <see cref="DataColumn"/> to move.</param>
    /// <param name="adjustment">Negative to move left, positive to move right.</param>
    public MoveColumnOperation(Design design, DataColumn column, int adjustment)
        : base(
            (v) => v.GetDataTable().Columns.Cast<DataColumn>().ToArray(),
            (v, a) => v.ReOrderColumns(a),
            (c) => c.ColumnName,
            design,
            column,
            adjustment)
    {
    }
}
