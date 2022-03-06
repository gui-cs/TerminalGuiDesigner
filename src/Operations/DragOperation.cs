
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DragOperation : IOperation
{
    public View BeingDragged { get; }
    public Pos OriginX { get; }
    public Pos OriginY { get; }

    public int DestinationX { get; set; }
    public int DestinationY { get; set; }

    public DragOperation(View beingDragged, Pos originX, Pos originY,int destX,int destY)
    {
        BeingDragged = beingDragged;
        OriginX = originX;
        OriginY = originY;
        DestinationX = destX;
        DestinationY = destY;
    }

    public void Do()
    {
        if (BeingDragged.X.IsAbsolute())
            BeingDragged.X = DestinationX;

        if (BeingDragged.Y.IsAbsolute())
            BeingDragged.Y = DestinationY;
    }

    public void Undo()
    {
        if (BeingDragged.X.IsAbsolute())
            BeingDragged.X = OriginX;

        if (BeingDragged.Y.IsAbsolute())
            BeingDragged.Y = OriginY;
    }

    public void Redo()
    {
        Do();
    }

    internal void ContinueDrag(Point dest)
    {
        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).

        if (BeingDragged.X.IsAbsolute())
            BeingDragged.X = dest.X;

        DestinationX = dest.X;

        if (BeingDragged.Y.IsAbsolute())
            BeingDragged.Y = dest.Y;

        DestinationY = dest.Y;

    }
}
