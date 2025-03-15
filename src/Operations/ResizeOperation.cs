using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Allows the user to resize a control by dragging from the bottom left corner
/// to a new location.
/// </summary>
public class ResizeOperation : Operation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeOperation"/> class.
    /// </summary>
    /// <param name="beingResized">Wrapper for the <see cref="View"/> that is to be resized.</param>
    /// <param name="destX">Client coordinate X point within <paramref name="beingResized"/> <see cref="View.SuperView"/>
    /// where the mouse cursor is positioned for resizing.</param>
    /// <param name="destY">Client coordinate Y point within <paramref name="beingResized"/> <see cref="View.SuperView"/>
    /// where the mouse cursor is positioned for resizing.</param>
    public ResizeOperation(Design beingResized, int destX, int destY)
    {
        this.BeingResized = beingResized;
        this.OriginalWidth = beingResized.View.Width;
        this.OriginalHeight = beingResized.View.Height;
        this.DestinationX = destX;
        this.DestinationY = destY;
    }

    /// <summary>
    /// Gets the <see cref="Design"/> that is being resized.
    /// </summary>
    public Design BeingResized { get; }

    /// <summary>
    /// Gets the original <see cref="View.Width"/> of the <see cref="Design"/>.
    /// </summary>
    public Dim OriginalWidth { get; }

    /// <summary>
    /// Gets the original <see cref="View.Height"/> of the <see cref="Design"/>.
    /// </summary>
    public Dim OriginalHeight { get; }

    /// <summary>
    /// Gets the X client coordinates of the mouse within the client area of
    /// <see cref="BeingResized"/> <see cref="View.SuperView"/>.  This is NOT
    /// screen coordinates.  This will be used to determine the new Width.
    /// </summary>
    public int DestinationX { get; private set; }

    /// <summary>
    /// Gets the Y client coordinates of the mouse within the client area of
    /// <see cref="BeingResized"/> <see cref="View.SuperView"/>.  This is NOT
    /// screen coordinates.  This will be used to determine the new Height.
    /// </summary>
    public int DestinationY { get; private set; }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        this.BeingResized.GetDesignableProperty("Width")?.SetValue(this.OriginalWidth);
        this.BeingResized.GetDesignableProperty("Height")?.SetValue(this.OriginalHeight);
        this.BeingResized.View.Layout();
    }

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        this.Do();
    }

    /// <summary>
    /// Update <see cref="DestinationX"/> and <see cref="DestinationY"/> to use the new point in
    /// <see cref="BeingResized"/> <see cref="View.SuperView"/> client area.
    /// </summary>
    /// <param name="dest">Client coordinates of <see cref="BeingResized"/> <see cref="View.SuperView"/> to resize to.</param>
    public void ContinueResize(Point dest)
    {
        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).
        this.DestinationX = dest.X;
        this.SetWidth();

        this.DestinationY = dest.Y;
        this.SetHeight();

        this.BeingResized.View.Layout();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        this.SetWidth();
        this.SetHeight();

        return true;
    }

    private void SetHeight()
    {
        // update width, the +1 comes because we want to include the cursor location in the Width.
        // e.g. resize bounds 0,0 to 1,1 means we want a width/height of 2
        if (this.BeingResized.View.Y.IsAbsolute(out var y) && this.BeingResized.View.Height.IsAbsolute())
        {
            this.BeingResized.GetDesignableProperty("Height")?.SetValue(Math.Max(1, this.DestinationY + 1 - y));
        }
    }

    private void SetWidth()
    {
        if (this.BeingResized.View.X.IsAbsolute(out var x) && this.BeingResized.View.Width.IsAbsolute())
        {
            this.BeingResized.GetDesignableProperty("Width")?.SetValue(Math.Max(1, this.DestinationX + 1 - x));
        }
    }
}
