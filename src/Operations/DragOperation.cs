
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

    /// <summary>
    /// The View that the <see cref="BeingDragged"/> was in at the start of the drag operation.
    /// Important for undo when <see cref="DropInto"/> is set.
    /// </summary>
    public readonly View OriginalSuperView;

    public int DestinationX { get; set; }
    public int DestinationY { get; set; }

    /// <summary>
    /// A <see cref="Design.IsContainerView"/> into which to move the control in addition
    /// to moving it.  Allows users to drag views into other container views (e.g. tabs etc)
    /// </summary>
    public View? DropInto {
        get
        {
            return dropInto;
        } 
        set
        {
            // Don't let them attempt to drop a view into itself!
            if (value == BeingDragged.View)
                return;

            // don't let user drop stuff into labels or tables etc
            if (value != null && !value.IsContainerView())
                return;
            
            dropInto = value;
        }
    }

    private View? dropInto;
    public DragOperation(Design beingDragged, int destX, int destY)
    {
        BeingDragged = beingDragged;
        OriginX = beingDragged.View.X;
        OriginY = beingDragged.View.Y;
        DestinationX = destX;
        DestinationY = destY;

        OriginalClickX = destX;
        OriginalClickY = destY;
        OriginalSuperView = BeingDragged.View.SuperView;
    }
    public override bool Do()
    {

        if (DropInto != null)
        {
            // if changing to a new container
            if (DropInto != OriginalSuperView && OriginalSuperView != null)
            {
                OriginalSuperView.Remove(BeingDragged.View);
                DropInto.Add(BeingDragged.View);
            }
        }

        var offsetP = OffsetByDropInto(new Point(OriginalClickX,OriginalClickY));
        var dx = offsetP.X;
        var dy = offsetP.Y;

        if (BeingDragged.View.X.IsAbsolute() && OriginX.IsAbsolute(out var originX))
        {
            BeingDragged.GetDesignableProperty("X").SetValue(Pos.At(originX + (DestinationX - dx)));
        }

        if (BeingDragged.View.Y.IsAbsolute() && OriginY.IsAbsolute(out var originY))
        {
            BeingDragged.GetDesignableProperty("Y").SetValue(Pos.At(originY + (DestinationY - dy)));
        }

        return true;
    }


    /// <summary>
    /// When dropping into a new container the X/Y of the control change from being relative to 
    /// the one parents client area to another so we need to compensate otherwise when the user
    /// lets go of the mouse the control jumps (often out of the visible area of the control)
    /// </summary>
    /// <returns>The original point adjusted to the client area of the control you dropped into (if any).</returns>
    private Point OffsetByDropInto(Point p)
    {
        if(OriginalSuperView == DropInto || DropInto  == null)
            return p;

        OriginalSuperView.ViewToScreen(0, 0, out var originalSuperX, out var originalSuperY, false);
        DropInto.ViewToScreen(0, 0, out var newSuperX, out var newSuperY, false);

        p.Offset(newSuperX - originalSuperX, newSuperY - originalSuperY);
        return p;
    }

    public override void Undo()
    {
        // if we changed the parent of the object (e.g. by dragging it into another view)
        if (OriginalSuperView != null && BeingDragged.View.SuperView != OriginalSuperView)
        {
            // change back to the original container
            BeingDragged.View.SuperView?.Remove(BeingDragged.View);
            OriginalSuperView.Add(BeingDragged.View);
        }

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
