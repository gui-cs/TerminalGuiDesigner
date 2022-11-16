using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Documents the initial state of a <see cref="Design"/> that is
/// involved in a <see cref="DragOperation"/>.
/// </summary>
internal class DragMemento
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DragMemento"/> class.
    /// Records current state of <paramref name="design"/> (position and
    /// parent).  This allows state reversion during a <see cref="DragOperation"/>.
    /// </summary>
    /// <param name="design">A design participating in a <see cref="DragOperation"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <see cref="Design.IsRoot"/> or the parent of
    /// the dragged view could not be determined.</exception>
    public DragMemento(Design design)
    {
        if (design.IsRoot)
        {
            throw new ArgumentException($"Root views cannot be dragged and so cannot spawn {nameof(DragMemento)}");
        }

        this.Design = design;
        this.OriginalX = design.View.X;
        this.OriginalY = design.View.Y;

        this.OriginalSuperView = design.View.SuperView
            ?? throw new ArgumentException($"Design {design} (of View {design.View}) had no SuperView, what does it exist in?!");
    }

    /// <summary>
    /// Gets the Design that is being dragged (may be one of many).
    /// </summary>
    public Design Design { get; }

    /// <summary>
    /// Gets the <see cref="View.X"/> of the <see cref="View"/> wrapped by <see cref="Design"/>
    /// at the point the <see cref="DragMemento"/> was created (i.e. when <see cref="DragOperation"/>
    /// started).
    /// </summary>
    public Pos OriginalX { get; }

    /// <summary>
    /// Gets the <see cref="View.Y"/> of the <see cref="View"/> wrapped by <see cref="Design"/>
    /// at the point the <see cref="DragMemento"/> was created (i.e. when <see cref="DragOperation"/>
    /// started).
    /// </summary>
    public Pos OriginalY { get; }

    /// <summary>
    /// Gets the <see cref="View"/> that <see cref="Design"/> was in at the point the <see cref="DragMemento"/>
    /// was created (i.e. when <see cref="DragOperation"/> started).
    /// </summary>
    public View OriginalSuperView { get; }

    /// <summary>
    /// Rewinds the state of <see cref="Design"/> to its original parent and position.
    /// </summary>
    public void Undo()
    {
        // if we changed the parent of the object (e.g. by dragging it into another view)
        if (this.Design.View.SuperView != this.OriginalSuperView)
        {
            // change back to the original container
            this.Design.View.SuperView?.Remove(this.Design.View);
            this.OriginalSuperView.Add(this.Design.View);
        }

        // Revert X to OriginalX
        if (this.Design.View.X.IsAbsolute())
        {
            this.Design.GetDesignableProperty("X")
                ?.SetValue(this.OriginalX);
        }

        // Revert Y to OriginalY
        if (this.Design.View.Y.IsAbsolute())
        {
            this.Design.GetDesignableProperty("Y")
                ?.SetValue(this.OriginalY);
        }
    }
}