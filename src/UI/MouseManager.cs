using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI;

class MouseManager
{
    DragOperation? dragOperation = null;
    ResizeOperation? resizeOperation = null;

    public void HandleMouse(MouseEvent m, Design viewBeingEdited)
    {
        // start dragging
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && dragOperation == null)
        {
            var drag = viewBeingEdited.View.HitTest(m, out bool isLowerRight);


            // if nothing is going on yet
            if (drag != null && drag.Data is Design design && resizeOperation == null && dragOperation == null)
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

        // continue dragging
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

        // end dragging
        if (!m.Flags.HasFlag(MouseFlags.Button1Pressed))
        {
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

            if (resizeOperation != null)
            {
                // end resize
                OperationManager.Instance.Do(resizeOperation);
                resizeOperation = null;
            }

        }
    }
}
