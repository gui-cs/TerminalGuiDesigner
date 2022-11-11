using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Reposition views by an absolute number of units.  Cannot
/// move them into sub-views or off screen.  Ignores invalid
/// changes (e.g. if X/Y are PosRelative ).
/// </summary>
public class MoveViewOperation : Operation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoveViewOperation"/> class.
    /// Moves a <see cref="View"/> within it's current container by a fixed amount
    /// (e.g. nudging with Shift+Cursor).
    /// </summary>
    /// <param name="toMove">Wrapper of the <see cref="View"/> to move.</param>
    /// <param name="deltaX">The amount to move in the X plane.  Positive for Right and Negative for Left.
    /// Ignored if <see cref="View.X"/> is relative (e.g. <see cref="Pos.Center"/>).</param>
    /// <param name="deltaY">The amount to move in the Y plane.  Positive for Down and Negative for Up.
    /// Ignored if <see cref="View.Y"/> is relative (e.g. <see cref="Pos.Center"/>).</param>
    public MoveViewOperation(Design toMove, int deltaX, int deltaY)
    {
        this.BeingMoved = toMove;
        this.OriginX = toMove.View.X;
        this.OriginY = toMove.View.Y;

        // start out assuming X and Y are PosRelative so cannot be moved
        this.IsImpossible = true;
        var super = this.BeingMoved.View.SuperView;
        int maxWidth = (super?.Bounds.Width ?? int.MaxValue) - 1;
        int maxHeight = (super?.Bounds.Height ?? int.MaxValue) - 1;

        if (this.BeingMoved.View.X.IsAbsolute(out var x))
        {
            // x is absolute so record where this operation
            // moves to
            this.DestinationX = Math.Min(Math.Max(x + deltaX, 0), maxWidth);

            // if it moves it somewhere then command isn't impossible
            if (this.DestinationX != x)
            {
                this.IsImpossible = false;
            }
        }

        if (this.BeingMoved.View.Y.IsAbsolute(out var y))
        {
            // y is absolute so record where this operation
            // moves to
            this.DestinationY = Math.Min(Math.Max(y + deltaY, 0), maxHeight);

            // if it moves it somewhere then command isn't impossible
            if (this.DestinationY != y)
            {
                this.IsImpossible = false;
            }
        }
    }

    /// <summary>
    /// Gets the <see cref="Design"/> wrapper for the <see cref="View"/> that is
    /// being moved.
    /// </summary>
    public Design BeingMoved { get; }

    /// <summary>
    /// Gets the original value <see cref="View.X"/> of <see cref="BeingMoved"/> before
    /// operation was performed.
    /// </summary>
    public Pos OriginX { get; }

    /// <summary>
    /// Gets the original value <see cref="View.Y"/> of <see cref="BeingMoved"/> before
    /// operation was performed.
    /// </summary>
    public Pos OriginY { get; }

    /// <summary>
    /// Gets the <see cref="View.Y"/> that will be set on <see cref="BeingMoved"/> if the
    /// operation is run (see <see cref="Operation.Do"/>).
    /// </summary>
    public int DestinationY { get; }

    /// <summary>
    /// Gets the <see cref="View.X"/> that will be set on <see cref="BeingMoved"/> if the
    /// operation is run (see <see cref="Operation.Do"/>).
    /// </summary>
    public int DestinationX { get; }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (this.BeingMoved.View.X.IsAbsolute())
        {
            this.BeingMoved.View.X = this.OriginX;
        }

        if (this.BeingMoved.View.Y.IsAbsolute())
        {
            this.BeingMoved.View.Y = this.OriginY;
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.BeingMoved.View.X.IsAbsolute())
        {
            this.BeingMoved.View.X = this.DestinationX;
        }

        if (this.BeingMoved.View.Y.IsAbsolute())
        {
            this.BeingMoved.View.Y = this.DestinationY;
        }

        return true;
    }
}
