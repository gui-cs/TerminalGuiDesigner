
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DragOperation : IOperation
{
    public Design BeingDragged { get; }
    public Pos OriginX { get; }
    public Pos OriginY { get; }

    public int DestinationX { get; set; }
    public int DestinationY { get; set; }

    public DragOperation(Design beingDragged, Pos originX, Pos originY,int destX,int destY)
    {
        BeingDragged = beingDragged;
        OriginX = originX;
        OriginY = originY;
        DestinationX = destX;
        DestinationY = destY;
    }
    public void Do()
    {
        if (BeingDragged.View.X.IsAbsolute())
        {
            BeingDragged.View.X = DestinationX;
        }

        if (BeingDragged.View.Y.IsAbsolute())
        {
            BeingDragged.View.Y = DestinationY;
        }
    }

    public void Undo()
    {
        if (BeingDragged.View.X.IsAbsolute())
        {
            // TODO : find and update the property properly
            // not on the view directly
            BeingDragged.View.X = OriginX;
        }

        if (BeingDragged.View.Y.IsAbsolute())
        {
            BeingDragged.View.Y = OriginY;
        }
    }

    public void Redo()
    {
        Do();
    }

    public void ContinueDrag(Point dest)
    {
        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).

        if (BeingDragged.View.X.IsAbsolute())
            BeingDragged.View.X = dest.X;

        DestinationX = dest.X;

        if (BeingDragged.View.Y.IsAbsolute())
            BeingDragged.View.Y = dest.Y;

        DestinationY = dest.Y;

    }
}
