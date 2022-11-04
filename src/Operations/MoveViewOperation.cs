using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Reposition views by an absolute number of units.  Cannot
/// move them into subviews or off screen.  Ignores invalid
/// changes (e.g. if X/Y are PosRelative )
/// </summary>
public class MoveViewOperation : Operation
{
    public Design BeingMoved { get; }
    public Pos OriginX { get; }
    public Pos OriginY { get; }
    public int DestinationY { get; }
    public int DestinationX { get; }

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

    public override bool Do()
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

    public override void Redo()
    {
        this.Do();
    }

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
}
