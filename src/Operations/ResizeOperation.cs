
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
        BeingResized = beingResized;
        OriginalWidth = beingResized.View.Width;
        OriginalHeight = beingResized.View.Height;
        DestinationX = destX;
        DestinationY = destY;
    }
    public override bool Do()
    {
        if (BeingResized.View.X.IsAbsolute(out var x))
        {
            BeingResized.GetDesignableProperty("Width")?.SetValue(Dim.Sized(Math.Max(1, DestinationX - x)));
        }

        if (BeingResized.View.Y.IsAbsolute(out var y))
        {
            BeingResized.GetDesignableProperty("Height")?.SetValue(Dim.Sized(Math.Max(1, DestinationY - y)));
        }
        
        return true;
    }

    public override void Undo()
    {
        BeingResized.GetDesignableProperty("Width")?.SetValue(OriginalWidth);
        BeingResized.GetDesignableProperty("Height")?.SetValue(OriginalHeight);
    }

    public override void Redo()
    {
        Do();
    }

    public void ContinueResize(Point dest)
    {
        // Only support dragging for properties that are exact absolute
        // positions (i.e. not relative positioning - Bottom of other control etc).

        if (BeingResized.View.X.IsAbsolute(out var x))
            BeingResized.GetDesignableProperty("Width")?.SetValue(Math.Max(1,dest.X - x));

        DestinationX = dest.X;

        if (BeingResized.View.Y.IsAbsolute(out var y))
            BeingResized.GetDesignableProperty("Height")?.SetValue(Math.Max(1,dest.Y - y));

        DestinationY = dest.Y;
    }
}
