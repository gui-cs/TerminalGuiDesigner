
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Allows the user to resize a control by dragging from the bottom left corner
/// to a new location
/// </summary>
public class ResizeOperation : Operation
{
    public Design BeingResized { get; }

    public Dim OriginalWidth { get; }

    public Dim OriginalHeight { get; }

    public int DestinationX { get; set; }

    public int DestinationY { get; set; }

    public ResizeOperation(Design beingResized, int destX, int destY)
    {
        this.BeingResized = beingResized;
        this.OriginalWidth = beingResized.View.Width;
        this.OriginalHeight = beingResized.View.Height;
        this.DestinationX = destX;
        this.DestinationY = destY;
    }

    protected override bool DoImpl()
    {
        this.SetWidth();
        this.SetHeight();

        return true;
    }

    public override void Undo()
    {
        this.BeingResized.GetDesignableProperty("Width")?.SetValue(this.OriginalWidth);
        this.BeingResized.GetDesignableProperty("Height")?.SetValue(this.OriginalHeight);
    }

    public override void Redo()
    {
        this.Do();
    }

    public void ContinueResize(Point dest)
    {
        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).

        this.DestinationX = dest.X;
        this.SetWidth();

        this.DestinationY = dest.Y;
        this.SetHeight();
    }

    private void SetHeight()
    {
        // update width, the +1 comes because we want to include the cursor location in the Width.
        // e.g. resize bounds 0,0 to 1,1 means we want a width/height of 2

        if (this.BeingResized.View.Y.IsAbsolute(out var y))
        {
            this.BeingResized.GetDesignableProperty("Height")?.SetValue(Math.Max(1, this.DestinationY + 1 - y));
        }
    }

    private void SetWidth()
    {
        if (this.BeingResized.View.X.IsAbsolute(out var x))
        {
            this.BeingResized.GetDesignableProperty("Width")?.SetValue(Math.Max(1, this.DestinationX + 1 - x));
        }
    }
}
