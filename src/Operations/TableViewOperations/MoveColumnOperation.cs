using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Moves a <see cref="DataColumn"/> left or right within the ordering
/// of columns in a <see cref="TableView"/>.
/// </summary>
public class MoveColumnOperation : ColumnOperation
{
    /// <summary>
    /// Gets the number of index positions the <see cref="DataColumn"/> will
    /// be moved. Negative for left, positive for right.
    /// </summary>
    private readonly int adjustment;
    private readonly int originalIdx;
    private readonly int newIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveColumnOperation"/> class.
    /// Creates an operation that will change the ordering of columns within
    /// a <see cref="TableView"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="TableView"/>.</param>
    /// <param name="column">The <see cref="DataColumn"/> to move.</param>
    /// <param name="adjustment">Negative to move left, positive to move right.</param>
    public MoveColumnOperation(Design design, DataColumn column, int adjustment)
        : base(design, column)
    {
        if (this.TableView.Table.Columns.Count == 1)
        {
            this.IsImpossible = true;
        }

        this.originalIdx = column.Ordinal;

        if (this.originalIdx == -1)
        {
            this.IsImpossible = true;
            return;
        }

        // calculate new index without falling off array
        this.newIndex = Math.Max(0, Math.Min(
                    this.TableView.Table.Columns.Count - 1,
                    this.originalIdx + adjustment));

        // they are moving it nowhere?!
        if (this.newIndex == this.originalIdx)
        {
            this.IsImpossible = true;
        }

        this.adjustment = adjustment;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this.adjustment == 0)
        {
            return $"Bad Command '{this.GetType().Name}'";
        }

        if (this.adjustment < 0)
        {
            return $"Move '{this.Column}' Left";
        }

        if (this.adjustment > 0)
        {
            return $"Move '{this.Column}' Right";
        }

        return base.ToString();
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        this.Column.SetOrdinal(this.originalIdx);
        this.TableView.Update();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        this.Column.SetOrdinal(this.newIndex);
        this.TableView.Update();
        return true;
    }
}
