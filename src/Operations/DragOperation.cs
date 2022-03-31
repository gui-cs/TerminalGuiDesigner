
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Allows the user to use the mouse to reposition views by
/// dragging them around the place.
/// </summary>
public class DragOperation : Operation
{
    public Design BeingDragged { get; }
    public Pos OriginX { get; }
    public Pos OriginY { get; }

    /// <summary>
    /// When draging from the middle of the control this is the position of the cursor X 
    /// at the start of the drag
    /// </summary>
    public readonly int OriginalClickX;
    /// <summary>
    /// When draging from the middle of the control this is the position of the cursor Y 
    /// at the start of the drag
    /// </summary>
    public readonly int OriginalClickY;

    public int DestinationX { get; set; }
    public int DestinationY { get; set; }

    public DragOperation(Design beingDragged, int destX,int destY)
    {
        BeingDragged = beingDragged;
        OriginX = beingDragged.View.X;
        OriginY = beingDragged.View.Y;
        DestinationX = destX;
        DestinationY = destY;

        OriginalClickX = destX;
        OriginalClickY = destY;
    }
    public override void Do()
    {
        if (BeingDragged.View.X.IsAbsolute() && OriginX.IsAbsolute(out var originX))
        {
            BeingDragged.GetDesignableProperty("X").SetValue(Pos.At(originX + (DestinationX - OriginalClickX)));
        }

        if (BeingDragged.View.Y.IsAbsolute() && OriginY.IsAbsolute(out var originY))
        {
            BeingDragged.GetDesignableProperty("Y").SetValue(Pos.At(originY + (DestinationY - OriginalClickY)));
        }
    }

    public override void Undo()
    {
        if (BeingDragged.View.X.IsAbsolute())
        {
            BeingDragged.GetDesignableProperty("X").SetValue(OriginX);
        }

        if (BeingDragged.View.Y.IsAbsolute())
        {
            BeingDragged.GetDesignableProperty("Y").SetValue(OriginY);
        }
    }

    public override void Redo()
    {
        Do();
    }

    public void ContinueDrag(Point dest)
    {
        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).

        if (BeingDragged.View.X.IsAbsolute() && OriginX.IsAbsolute(out var originX))
            BeingDragged.View.X = originX + (dest.X - OriginalClickX);

        DestinationX = dest.X;

        if (BeingDragged.View.Y.IsAbsolute() && OriginY.IsAbsolute(out var originY))
            BeingDragged.View.Y = originY + (dest.Y - OriginalClickY);

        DestinationY = dest.Y;

    }
}
