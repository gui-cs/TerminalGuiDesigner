
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Documents the initial state of a <see cref="Design"/> that is
/// involved in a <see cref="DragOperation"/>.
/// </summary>
internal class DragMemento
{
    public DragMemento(Design design)
    {
        this.Design = design;
        this.OriginalX = design.View.X;
        this.OriginalY = design.View.Y;

        this.OriginalSuperView = design.View.SuperView;
        this.OriginalSuperViewNearestDesign = this.OriginalSuperView?.GetNearestDesign();
    }

    /// <summary>
    /// Gets the Design that is being dragged (may be one of many).
    /// </summary>
    public Design Design { get; }

    public Pos OriginalX { get; }

    public Pos OriginalY { get; }

    public View? OriginalSuperView { get; }
    public Design? OriginalSuperViewNearestDesign { get; }


    /// <summary>
    /// Rewinds the state of <see cref="Design"/> to its original parent and position.
    /// </summary>
    public void Undo()
    {
        // if we changed the parent of the object (e.g. by dragging it into another view)
        if (this.OriginalSuperView != null && this.Design.View.SuperView != this.OriginalSuperView)
        {
            // change back to the original container
            this.Design.View.SuperView?.Remove(this.Design.View);
            this.OriginalSuperView.Add(this.Design.View);
        }

        if (this.Design.View.X.IsAbsolute())
        {
            this.Design.GetDesignableProperty("X")
                ?.SetValue(this.OriginalX);
        }

        if (this.Design.View.Y.IsAbsolute())
        {
            this.Design.GetDesignableProperty("Y")
                ?.SetValue(this.OriginalY);
        }
    }
}

