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
        BeingMoved = toMove;
        OriginX = toMove.View.X;
        OriginY = toMove.View.Y;

        // start out assuming X and Y are PosRelative so cannot be moved
        IsImpossible = true;
        var super = BeingMoved.View.SuperView;
        int maxWidth = (super?.Bounds.Width ?? int.MaxValue) - 1;
        int maxHeight = (super?.Bounds.Height ?? int.MaxValue) - 1;

        if (BeingMoved.View.X.IsAbsolute(out var x))
        {
            // x is absolute so record where this operation
            // moves to
            DestinationX = Math.Min(Math.Max(x + deltaX, 0), maxWidth);

            // if it moves it somewhere then command isn't impossible
            if (DestinationX != x)
            {
                IsImpossible = false;
            }
        }

        if (BeingMoved.View.Y.IsAbsolute(out var y))
        {
            // y is absolute so record where this operation
            // moves to
            DestinationY = Math.Min(Math.Max(y + deltaY, 0), maxHeight);

            // if it moves it somewhere then command isn't impossible
            if (DestinationY != y)
            {
                IsImpossible = false;
            }
        }
    }

    public override bool Do()
    {
        if (BeingMoved.View.X.IsAbsolute())
        {
            BeingMoved.View.X = DestinationX;
        }

        if (BeingMoved.View.Y.IsAbsolute())
        {
            BeingMoved.View.Y = DestinationY;
        }

        return true;
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        if (BeingMoved.View.X.IsAbsolute())
        {
            BeingMoved.View.X = OriginX;
        }

        if (BeingMoved.View.Y.IsAbsolute())
        {
            BeingMoved.View.Y = OriginY;
        }
    }
}
