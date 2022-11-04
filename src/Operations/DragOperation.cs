
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
        public Design Design { get; set; }
        public Pos OriginalX { get; }
        public Pos OriginalY { get; }

        public View? OriginalSuperView { get; }

        public DragMemento(Design design)
        {
            this.Design = design;
            this.OriginalX = design.View.X;
            this.OriginalY = design.View.Y;
            this.OriginalSuperView = design.View.SuperView?.GetNearestDesign()?.View;
        }
    }

    private List<DragMemento> Mementos = new();

    public Design BeingDragged => this.Mementos.First().Design;

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

    /// <summary>
    /// A <see cref="Design.IsContainerView"/> into which to move the control in addition
    /// to moving it.  Allows users to drag views into other container views (e.g. tabs etc)
    /// </summary>
    public View? DropInto
    {
        get
        {
            return this.dropInto;
        }
        set
        {
            // Don't let them attempt to drop a view into itself!
            if (this.Mementos.Any(m => m.Design.View == value))
            {
                return;
            }

            // don't let user drop stuff into labels or tables etc
            if (value != null && !value.IsContainerView())
            {
                return;
            }

            this.dropInto = value;
        }
    }

    private View? dropInto;
    public DragOperation(Design beingDragged, int destX, int destY, Design[]? alsoDrag)
    {
        // don't let the user drag the root view anywhere
        if (beingDragged.IsRoot || (alsoDrag?.Any(d => d.IsRoot) ?? false))
        {
            this.IsImpossible = true;
            return;
        }

        this.Mementos.Add(new DragMemento(beingDragged));

        this.DestinationX = destX;
        this.DestinationY = destY;

        foreach (var d in alsoDrag ?? new Design[0])
        {
            if (!this.Mementos.Any(m => m.Design == d))
            {
                this.Mementos.Add(new DragMemento(d));
            }
        }

        this.OriginalClickX = destX;
        this.OriginalClickY = destY;
    }

    public override bool Do()
    {
        bool didAny = false;
        foreach (var mem in this.Mementos)
        {
            didAny = this.Do(mem) || didAny;
        }

        return didAny;
    }

    private bool Do(DragMemento mem)
    {
        if (this.DropInto != null)
        {
            // if changing to a new container
            if (this.DropInto != mem.OriginalSuperView && mem.OriginalSuperView != null)
            {
                mem.Design.View.SuperView.Remove(mem.Design.View);
                this.DropInto.Add(mem.Design.View);
            }
        }

        var offsetP = this.OffsetByDropInto(mem, new Point(this.OriginalClickX, this.OriginalClickY));
        var dx = offsetP.X;
        var dy = offsetP.Y;

        if (mem.Design.View.X.IsAbsolute() &&
            mem.OriginalX.IsAbsolute(out var originX))
        {
            mem.Design.GetDesignableProperty("X")?.SetValue(Pos.At(originX + (this.DestinationX - dx)));
        }

        if (mem.Design.View.Y.IsAbsolute() &&
            mem.OriginalY.IsAbsolute(out var originY))
        {
            mem.Design.GetDesignableProperty("Y")?.SetValue(Pos.At(originY + (this.DestinationY - dy)));
        }

        return true;
    }

    /// <summary>
    /// When dropping into a new container the X/Y of the control change from being relative to
    /// the one parents client area to another so we need to compensate otherwise when the user
    /// lets go of the mouse the control jumps (often out of the visible area of the control)
    /// </summary>
    /// <returns>The original point adjusted to the client area of the control you dropped into (if any).</returns>
    private Point OffsetByDropInto(DragMemento mem, Point p)
    {
        if (mem.OriginalSuperView == this.DropInto || this.DropInto == null)
        {
            return p;
        }

        mem.OriginalSuperView.ViewToScreen(0, 0, out var originalSuperX, out var originalSuperY, false);
        this.DropInto.ViewToScreen(0, 0, out var newSuperX, out var newSuperY, false);

        p.Offset(newSuperX - originalSuperX, newSuperY - originalSuperY);
        return p;
    }

    public override void Undo()
    {
        foreach (var mem in this.Mementos)
        {
            this.Undo(mem);
        }
    }

    private void Undo(DragMemento mem)
    {
        // if we changed the parent of the object (e.g. by dragging it into another view)
        if (mem.OriginalSuperView != null && mem.Design.View.SuperView != mem.OriginalSuperView)
        {
            // change back to the original container
            mem.Design.View.SuperView?.Remove(mem.Design.View);
            mem.OriginalSuperView.Add(mem.Design.View);
        }

        if (mem.Design.View.X.IsAbsolute())
        {
            mem.Design.GetDesignableProperty("X")
                ?.SetValue(mem.OriginalX);
        }

        if (mem.Design.View.Y.IsAbsolute())
        {
            mem.Design.GetDesignableProperty("Y")
                ?.SetValue(mem.OriginalY);
        }
    }

    public override void Redo()
    {
        this.Do();
    }

    public void ContinueDrag(Point dest)
    {
        foreach (var mem in this.Mementos)
        {
            this.ContinueDrag(mem, dest);
        }
    }

    private void ContinueDrag(DragMemento mem, Point dest)
    {
        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).

        if (mem.Design.View.X.IsAbsolute() && mem.OriginalX.IsAbsolute(out var originX))
        {
            mem.Design.View.X = originX + (dest.X - this.OriginalClickX);
        }

        this.DestinationX = dest.X;

        if (mem.Design.View.Y.IsAbsolute() && mem.OriginalY.IsAbsolute(out var originY))
        {
            mem.Design.View.Y = originY + (dest.Y - this.OriginalClickY);
        }

        this.DestinationY = dest.Y;
    }
}
