
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DragOperation : Operation
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
    public override void Do()
    {
        if (BeingDragged.View.X.IsAbsolute())
        {
            BeingDragged.GetDesignableProperty("X").SetValue(Pos.At(DestinationX));
        }

        if (BeingDragged.View.Y.IsAbsolute())
        {
            BeingDragged.GetDesignableProperty("Y").SetValue(Pos.At(DestinationY));
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

        if (BeingDragged.View.X.IsAbsolute())
            BeingDragged.View.X = dest.X;

        DestinationX = dest.X;

        if (BeingDragged.View.Y.IsAbsolute())
            BeingDragged.View.Y = dest.Y;

        DestinationY = dest.Y;

    }
}
