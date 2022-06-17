using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI;

class MouseManager
{
    DragOperation? dragOperation = null;
    ResizeOperation? resizeOperation = null;

    /// <summary>
    /// If the user is dragging a selection box then this is the current area
    /// that is being pulled over or null if no multi select is underway.
    /// </summary>

    private Point? SelectionStart = null;
    private Point? SelectionEnd = null;
    public Rect? SelectionBox => RectExtensions.FromBetweenPoints(SelectionStart,SelectionEnd);

    public void HandleMouse(MouseEvent m, Design viewBeingEdited)
    {
        // start dragging
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) 
            && resizeOperation == null && dragOperation == null && SelectionStart == null)
        {
            var drag = viewBeingEdited.View.HitTest(m, out bool isLowerRight);

            // if mousing down in empty space
            // TODO: why this doesn't conditional work?
            // if (drag == null || drag.IsContainerView())
            {
                // start dragging a selection box
                SelectionStart = new Point(m.X,m.Y);
            }

            // if nothing is going on yet
            if (drag != null && drag.Data is Design design
             && resizeOperation == null && dragOperation == null && SelectionStart == null)
            {
                var dest = viewBeingEdited.View.ScreenToClient(m.X, m.Y);
                
                if (isLowerRight)
                {
                    resizeOperation = new ResizeOperation(design, dest.X, dest.Y);
                }
                else
                {
                    dragOperation = new DragOperation(design, dest.X, dest.Y);
                }
                
            }
        }

        // continue dragging a selection box
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && SelectionStart != null)
        {
            // move selection box to new mouse position
            SelectionEnd = new Point(m.X,m.Y);
            viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
            return;
        }

        // continue dragging a view
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragOperation != null)
        {
            var dest = viewBeingEdited.View.ScreenToClient(m.X, m.Y);

            dragOperation.ContinueDrag(dest);

            viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
        }

        // continue resizing
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && resizeOperation != null)
        {
            var dest = viewBeingEdited.View.ScreenToClient(m.X, m.Y);

            resizeOperation.ContinueResize(dest);

            viewBeingEdited.View.SetNeedsDisplay();
            Application.DoEvents();
        }

        // end things (because mouse released)
        if (!m.Flags.HasFlag(MouseFlags.Button1Pressed))
        {
            // end selection box
            if (SelectionStart != null)
            {
                // TODO: record what was multi selected somewhere

                SelectionStart = null;
                SelectionEnd = null;
            }

            //end dragging
            if ( dragOperation != null)
            {
                // see if we are dragging into a new container
                var dropInto = viewBeingEdited.View.HitTest(m, out _, dragOperation.BeingDragged.View);

                // we are dragging into a new container
                dragOperation.DropInto = dropInto;

                // end drag
                OperationManager.Instance.Do(dragOperation);
                dragOperation = null;
            }

            // end resize
            if (resizeOperation != null)
            {
                // end resize
                OperationManager.Instance.Do(resizeOperation);
                resizeOperation = null;
            }

        }
    }
}
