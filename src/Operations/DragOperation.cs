
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Allows the user to use the mouse to reposition views by
/// dragging them around the place.
/// </summary>
public class DragOperation : Operation
{

    private class DragMemento
    {
        public Pos OriginalX {get;}
        public Pos OriginalY {get;}

        public View OriginalSuperView {get;}

        public DragMemento(Design beingDragged)
        {
            OriginalX = beingDragged.View.X;
            OriginalY = beingDragged.View.Y;
            OriginalSuperView = beingDragged.View.SuperView;
        }
    }

    public Design BeingDragged { get; }

    private Dictionary<Design,DragMemento> Mementos = new();

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
    public Design[] AlsoDrag { get; }
    
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
    public DragOperation(Design beingDragged, int destX, int destY, Design[]? alsoDrag)
    {
        BeingDragged = beingDragged;
        
        Mementos.Add(beingDragged,new DragMemento(beingDragged));

        DestinationX = destX;
        DestinationY = destY;
        AlsoDrag = alsoDrag ?? new Design[0];

        foreach(var d in AlsoDrag)
        {
            if(!Mementos.ContainsKey(d))
                Mementos.Add(d,new DragMemento(d));
        }

        OriginalClickX = destX;
        OriginalClickY = destY;
    }
    public override bool Do()
    {
        var mem = Mementos[BeingDragged];

        if (DropInto != null)
        {
            // if changing to a new container
            if (DropInto != mem.OriginalSuperView && mem.OriginalSuperView != null)
            {
                mem.OriginalSuperView.Remove(BeingDragged.View);
                DropInto.Add(BeingDragged.View);
            }
        }

        var offsetP = OffsetByDropInto(mem,new Point(OriginalClickX,OriginalClickY));
        var dx = offsetP.X;
        var dy = offsetP.Y;

        if (BeingDragged.View.X.IsAbsolute() && 
            mem.OriginalX.IsAbsolute(out var originX))
        {
            BeingDragged.GetDesignableProperty("X")?.SetValue(Pos.At(originX + (DestinationX - dx)));
        }

        if (BeingDragged.View.Y.IsAbsolute() && 
            mem.OriginalY.IsAbsolute(out var originY))
        {
            BeingDragged.GetDesignableProperty("Y")?.SetValue(Pos.At(originY + (DestinationY - dy)));
        }

        return true;
    }


    /// <summary>
    /// When dropping into a new container the X/Y of the control change from being relative to 
    /// the one parents client area to another so we need to compensate otherwise when the user
    /// lets go of the mouse the control jumps (often out of the visible area of the control)
    /// </summary>
    /// <returns>The original point adjusted to the client area of the control you dropped into (if any).</returns>
    private Point OffsetByDropInto(DragMemento mem,Point p)
    {
        if(mem.OriginalSuperView == DropInto || DropInto  == null)
            return p;

        mem.OriginalSuperView.ViewToScreen(0, 0, out var originalSuperX, out var originalSuperY, false);
        DropInto.ViewToScreen(0, 0, out var newSuperX, out var newSuperY, false);

        p.Offset(newSuperX - originalSuperX, newSuperY - originalSuperY);
        return p;
    }

    public override void Undo()
    {
        var mem = Mementos[BeingDragged];

        // if we changed the parent of the object (e.g. by dragging it into another view)
        if (mem.OriginalSuperView != null && BeingDragged.View.SuperView != mem.OriginalSuperView)
        {
            // change back to the original container
            BeingDragged.View.SuperView?.Remove(BeingDragged.View);
            mem.OriginalSuperView.Add(BeingDragged.View);
        }

        if (BeingDragged.View.X.IsAbsolute())
        {
            BeingDragged.GetDesignableProperty("X")
                ?.SetValue(mem.OriginalX);
        }

        if (BeingDragged.View.Y.IsAbsolute())
        {
            BeingDragged.GetDesignableProperty("Y")
                ?.SetValue(mem.OriginalY);
        }
    }

    public override void Redo()
    {
        Do();
    }

    public void ContinueDrag(Point dest)
    {

        var mem = Mementos[BeingDragged];

        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).

        if (BeingDragged.View.X.IsAbsolute() && mem.OriginalX.IsAbsolute(out var originX))
            BeingDragged.View.X = originX + (dest.X - OriginalClickX);

        DestinationX = dest.X;

        if (BeingDragged.View.Y.IsAbsolute() && mem.OriginalY.IsAbsolute(out var originY))
            BeingDragged.View.Y = originY + (dest.Y - OriginalClickY);

        DestinationY = dest.Y;

    }
}
