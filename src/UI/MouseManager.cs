using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI;

public class MouseManager
{
    DragOperation? dragOperation = null;
    ResizeOperation? resizeOperation = null;

    /// <summary>
    /// If the user is dragging a selection box then this is the current area
    /// that is being pulled over or null if no multi select is underway.
    /// </summary>

    private Point? SelectionStart = null;
    private Point? SelectionEnd = null;

    public MouseManager()
    {
    }

    public Rect? SelectionBox => RectExtensions.FromBetweenPoints(this.SelectionStart, this.SelectionEnd);
    public View? SelectionContainer { get; private set; }

    public void HandleMouse(MouseEvent m, Design viewBeingEdited)
    {
        // start dragging
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed)
            && this.resizeOperation == null && this.dragOperation == null && this.SelectionStart == null)
        {
            var drag = viewBeingEdited.View.HitTest(m, out bool isBorder, out bool isLowerRight);

            // if mousing down in empty space
            if (drag != null && drag.IsContainerView() && !isLowerRight && !isBorder)
            {
                // start dragging a selection box
                this.SelectionContainer = drag;
                this.SelectionStart = new Point(m.X, m.Y);
            }

            // if nothing is going on yet
            if (drag != null && drag.Data is Design design
             && this.resizeOperation == null && this.dragOperation == null && this.SelectionStart == null)
            {
                var dest = viewBeingEdited.View.ScreenToClient(m.X, m.Y);

                if (isLowerRight)
                {
                    this.resizeOperation = new ResizeOperation(design, dest.X, dest.Y);
                }
                else
                {
                    var multiSelected = SelectionManager.Instance.Selected.ToArray();

                    // if user is click and drag moving a single view
                    // in a multi selection.
                    if (multiSelected.Contains(design))
                    {
                        // drag all the views at once                    
                        this.dragOperation = new DragOperation(design, dest.X, dest.Y,
                            multiSelected.Except(new[] { design }).ToArray());
                    }
                    else
                    {
                        // else drag only the non selected one
                        this.dragOperation = new DragOperation(design, dest.X, dest.Y, new Design[0]);
                    }

                    // don't begin an impossible drag!
                    if (this.dragOperation.IsImpossible)
                    {
                        this.dragOperation = null;
                    }
                }
            }
        }

        // continue dragging a selection box
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && this.SelectionStart != null)
        {
            // move selection box to new mouse position
            this.SelectionEnd = new Point(m.X, m.Y);
            viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
            return;
        }

        // continue dragging a view
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && this.dragOperation != null)
        {
            var dest = viewBeingEdited.View.ScreenToClient(m.X, m.Y);

            this.dragOperation.ContinueDrag(dest);

            viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
        }

        // continue resizing
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && this.resizeOperation != null)
        {
            var dest = viewBeingEdited.View.ScreenToClient(m.X, m.Y);

            this.resizeOperation.ContinueResize(dest);

            viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
        }

        // end things (because mouse released)
        if (!m.Flags.HasFlag(MouseFlags.Button1Pressed))
        {
            // end selection box
            if (this.SelectionStart != null && this.SelectionBox != null && this.SelectionContainer != null)
            {
                SelectionManager.Instance.SetSelection(
                    this.SelectionContainer.GetActualSubviews()
                    .Where(v => v.IntersectsScreenRect(this.SelectionBox.Value))
                    .Select(v => v.GetNearestDesign())
                    .Where(d => d != null && !d.IsRoot)
                    .Cast<Design>()
                    .ToArray()
                    );

                this.SelectionStart = null;
                this.SelectionEnd = null;
                this.SelectionContainer = null;
                viewBeingEdited.View.SetNeedsDisplay();
                Application.DoEvents();
            }

            // end dragging
            if (this.dragOperation != null)
            {
                // see if we are dragging into a new container
                var dropInto = viewBeingEdited.View.HitTest(m, out _, out _, this.dragOperation.BeingDragged.View);

                // we are dragging into a new container
                this.dragOperation.DropInto = dropInto;

                // end drag
                OperationManager.Instance.Do(this.dragOperation);
                this.dragOperation = null;
            }

            // end resize
            if (this.resizeOperation != null)
            {
                // end resize
                OperationManager.Instance.Do(this.resizeOperation);
                this.resizeOperation = null;
            }
        }
    }
}
