using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Allows the user to use the mouse to reposition views by
/// dragging them around the place.
/// </summary>
public partial class DragOperation : Operation
{
    private readonly List<DragMemento> mementos = new();

    private View? dropInto;

    /// <summary>
    /// Initializes a new instance of the <see cref="DragOperation"/> class.
    /// Begins a drag operation in which <paramref name="beingDragged"/> is moved.
    /// </summary>
    /// <param name="beingDragged">The primary design being moved.  Cannot be the root design (see <see cref="Design.IsRoot"/>).</param>
    /// <param name="originalX">The client X coordinate position of the mouse when dragging began.  Final location is calculated as an offset from this point.
    /// <remarks>This is not necessarily the X/Y of <paramref name="beingDragged"/> (e.g. if mouse click drag starts in from middle of View area)</remarks></param>
    /// <param name="originalY">The client Y coordinate position of the mouse when dragging began.  Final location is calculated as an offset from this point.
    /// <remarks>This is not necessarily the X/Y of <paramref name="beingDragged"/> (e.g. if mouse click drag starts in from middle of View area)</remarks></param>
    /// <param name="alsoDrag">Other Designs that are also multi selected and should be dragged at the same time.</param>
    public DragOperation(Design beingDragged, int originalX, int originalY, Design[]? alsoDrag)
    {
        // TODO: how does this respond when alsoDrag has some that are not in
        // same view as beingDragged - write unit test

        // don't let the user drag the root view anywhere
        if (beingDragged.IsRoot || (alsoDrag?.Any(d => d.IsRoot) ?? false))
        {
            this.IsImpossible = true;
            return;
        }

        this.mementos.Add(new DragMemento(beingDragged));

        this.DestinationX = originalX;
        this.DestinationY = originalY;

        // we also have to drag all these but don't drag
        // a thing that is in a container which is also being dragged
        foreach (var d in this.PruneChildViews(alsoDrag ?? new Design[0]) ?? new Design[0])
        {
            if (!this.mementos.Any(m => m.Design == d))
            {
                this.mementos.Add(new DragMemento(d));
            }
        }

        this.OriginalClickX = originalX;
        this.OriginalClickY = originalY;
    }

    /// <summary>
    /// Gets the Design (wrapper) for the View that is being dragged around.
    /// </summary>
    public Design BeingDragged => this.mementos.First().Design;

    /// <summary>
    /// Gets the position of the cursor within the area of <see cref="BeingDragged"/> when mouse was
    /// pressed down.  This is expressed in Client Coordinates (not screen coordinates) of the
    /// <see cref="BeingDragged"/> parent.
    /// <para>
    /// For example if a Label "Hey" is at X=1 and Y=1 of it's parent container and user clicks midway
    /// along the button then this could be X=3.
    /// </para>
    /// </summary>
    public int OriginalClickX { get; }

    /// <summary>
    /// Gets the position of the cursor within the area of <see cref="BeingDragged"/> when mouse was
    /// pressed down.  This is expressed in Client Coordinates (not screen coordinates) of the
    /// <see cref="BeingDragged"/> parent.
    /// </summary>
    public int OriginalClickY { get; }

    // TODO: Think this might be client coordinates not screen coordinates

    /// <summary>
    /// <para>
    /// Gets the client coordinates that the View will be moved to if the operation
    /// were to complete.  These are regularly updated during dragging.  They are expressed
    /// in client coordinates of the original View that contains <see cref="BeingDragged"/>.
    /// </para>
    /// <remarks>
    /// To change this use <see cref="ContinueDrag(Point)"/>.
    /// </remarks>
    /// </summary>
    public int DestinationX { get; private set; }

    /// <inheritdoc cref="DestinationX"/>
    public int DestinationY { get; private set; }

    /// <summary>
    /// <para>
    /// Gets or Sets a <see cref="Design.IsContainerView"/> into which to move the view in addition
    /// to moving it.  Allows users to drag views into other container views (e.g. tabs etc).
    /// </para>
    /// <para>To calculate this, map <see cref="DestinationX"/>/<see cref="DestinationY"/> into screen
    /// coordinates (based on current parent container) and then performing a hit test at that
    /// point.</para>
    /// <para>
    /// Attempts to set this to a non-container view will be ignored.
    /// </para>
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
            if (this.mementos.Any(m => m.Design.View == value))
            {
                return;
            }

            // don't let user drop stuff into labels or tables etc
            if (value != null && !value.IsContainerView())
            {
                return;
            }

            // When dropping onto a TabView
            if (value is TabView tv && tv.SelectedTab != null && tv.SelectedTab.View != null)
            {
                // Add to the content area of the selected tab instead
                value = tv.SelectedTab.View;
            }

            this.dropInto = value;
        }
    }

    /// <summary>
    /// Moves all dragged views back to original positions.
    /// </summary>
    public override void Undo()
    {
        foreach (var mem in this.mementos)
        {
            mem.Undo();
        }
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <summary>
    /// Updates the drag and the current positions of all Views being dragged
    /// to the new client coordinates.
    /// </summary>
    /// <param name="dest">Client coordinates to update to.  All dragged
    /// views must have same parent and this coordinate must be in that parents
    /// coordinate system (client area) although it can exceed the bounds (e.g.
    /// to drag out of current container and drop into another).
    /// </param>
    public void ContinueDrag(Point dest)
    {
        foreach (var mem in this.mementos)
        {
            this.ContinueDrag(mem, dest);
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        bool didAny = false;
        foreach (var mem in this.mementos)
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
            if (this.DropInto != mem.OriginalSuperView)
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
    /// When dropping into a new container the X/Y of the view change from being relative to
    /// the one parents client area to another so we need to compensate otherwise when the user
    /// lets go of the mouse the view jumps (often out of the visible area of the view).
    /// </summary>
    /// <returns>The original point adjusted to the client area of the view you dropped into (if any).</returns>
    private Point OffsetByDropInto(DragMemento mem, Point p)
    {
        // if not changing container then point is unchanged.
        if (mem.OriginalSuperView == this.DropInto || this.DropInto == null)
        {
            return p;
        }

        // Calculate screen coordinates of 0,0 in each of the views (from and to)
        mem.OriginalSuperView.ViewToScreen(0, 0, out var originalSuperX, out var originalSuperY, false);
        this.DropInto.ViewToScreen(0, 0, out var newSuperX, out var newSuperY, false);

        // Offset the point by the difference in screen space between 0,0 on each view
        p.Offset(newSuperX - originalSuperX, newSuperY - originalSuperY);
        return p;
    }

    private void ContinueDrag(DragMemento mem, Point dest)
    {
        /*
         * Only support dragging for properties that are exact absolute
         * positions (i.e. not relative positioning - Bottom of other view etc).
         */

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
