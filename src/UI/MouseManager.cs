using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Manages responding to root mouse e.g. by
/// dragging <see cref="Design"/> around and/or resizing.
/// </summary>
public class MouseManager
{
    private DragOperation? dragOperation = null;
    private ResizeOperation? resizeOperation = null;

    /// <summary>
    /// If the user is dragging a selection box then this is the current area
    /// that is being pulled over or null if no multi select is underway.
    /// </summary>
    private Point? selectionStart = null;
    private Point? selectionEnd = null;

    /// <summary>
    /// Gets the container that 'drag a box' selection is occurring in (if any).
    /// See also <see cref="SelectionBox"/>.
    /// </summary>
    private View? selectionContainer;

    /// <summary>
    /// Gets the current 'drag a box' selection area that is ongoing (if any).
    /// </summary>
    public Rectangle? SelectionBox => RectExtensions.FromBetweenPoints(this.selectionStart, this.selectionEnd);

    /// <summary>
    /// Responds to <see cref="Application.MouseEvent"/>(by changing a 'drag a box' selection area
    /// or starting a resize etc).
    /// </summary>
    /// <param name="m">The <see cref="MouseEventArgs"/> reported by <see cref="Application.RootMouseEvent"/>.</param>
    /// <param name="viewBeingEdited">The root <see cref="Design"/> that is open in the <see cref="Editor"/>.</param>
    public void HandleMouse(MouseEventArgs m, Design viewBeingEdited)
    {
        // start dragging
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed)
            && this.resizeOperation == null && this.dragOperation == null && this.selectionStart == null)
        {
            View? drag = viewBeingEdited.View.HitTest(m, out bool isBorder, out bool isLowerRight);

            // if user is ctrl+click
            if (m.Flags.HasFlag(MouseFlags.ButtonCtrl) && drag != null)
            {
                // then add or remove the clicked item from the group selection
                var addOrRemove = drag.GetNearestDesign();
                var selection = SelectionManager.Instance.Selected.ToList();

                if (addOrRemove != null && selection.Any())
                {
                    if (selection.Contains(addOrRemove))
                    {
                        selection.Remove(addOrRemove);
                    }
                    else
                    {
                        selection.Add(addOrRemove);
                    }

                    SelectionManager.Instance.ForceSetSelection(selection.ToArray());
                    return;
                }
            }

            // if mousing down in empty space
            if (drag != null && drag.IsContainerView() && !isLowerRight && !isBorder)
            {
                // start dragging a selection box
                this.selectionContainer = drag;
                this.selectionStart = m.Position;
            }

            // if nothing is going on yet
            if (drag != null && drag.Data is Design design && drag.SuperView != null
             && this.resizeOperation == null && this.dragOperation == null && this.selectionStart == null)
            {
                var parent = drag.SuperView;

                var dest = parent.ScreenToContent(m.Position);

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
                        this.dragOperation = new DragOperation(
                            design,
                            dest.X,
                            dest.Y,
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
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && this.selectionStart != null)
        {
            // move selection box to new mouse position
            this.selectionEnd = m.Position;
            viewBeingEdited.View.SetNeedsDraw();
            return;
        }

        // continue dragging a view
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed) && this.dragOperation?.BeingDragged.View?.SuperView != null)
        {
            var dest = this.dragOperation?.BeingDragged.View.SuperView.ScreenToContent(m.Position);

            if (dest != null && this.dragOperation != null)
            {
                this.dragOperation.ContinueDrag(dest.Value);
                viewBeingEdited.View.SetNeedsDraw();
                // BUG: Method is gone, will this functionality work still without it?
                // Application.DoEvents();
            }
        }

        // continue resizing
        if (m.Flags.HasFlag(MouseFlags.Button1Pressed)
            && this.resizeOperation != null
            && this.resizeOperation.BeingResized.View.SuperView != null)
        {
            var dest = this.resizeOperation.BeingResized.View.SuperView.ScreenToContent(m.Position);

            this.resizeOperation.ContinueResize(dest);

            viewBeingEdited.View.SetNeedsDraw();
            // BUG: Method is gone, will this functionality work still without it?
            // Application.DoEvents();
        }

        // end things (because mouse released)
        if (!m.Flags.HasFlag(MouseFlags.Button1Pressed))
        {
            // end selection box
            if (this.selectionStart != null && this.SelectionBox != null && this.selectionContainer != null)
            {
                SelectionManager.Instance.SetSelection(
                    this.selectionContainer.GetActualSubviews()
                    .Where(v => v.IntersectsScreenRect(this.SelectionBox.Value))
                    .Select(v => v.GetNearestDesign())
                    .Where(d => d != null && !d.IsRoot)
                    .Cast<Design>()
                    .ToArray());

                this.selectionStart = null;
                this.selectionEnd = null;
                this.selectionContainer = null;
                viewBeingEdited.View.SetNeedsDraw();
                // BUG: Method is gone, will this functionality work still without it?
                // Application.DoEvents();
            }

            // end dragging
            if (this.dragOperation != null)
            {
                // see if we are dragging into a new container
                var dropInto = viewBeingEdited.View.HitTest(m, out _, out _, this.dragOperation.BeingDragged.View);

                // TODO: this is quite hacky workaround for dropping on things like TabView top row.  Need
                // a better solution to this.

                // HitTest might return a sub-control (e.g. TabView top row tabs)
                // So grab the nearest user designable view.  That's probably what
                // they want to move into anyway.
                var into = dropInto?.GetNearestDesign();

                if (into != null && dropInto != this.dragOperation.DropInto)
                {
                    // we are dragging into a new container
                    this.dragOperation.DropInto = into.View;

                    // end drag
                    OperationManager.Instance.Do(this.dragOperation);
                    this.dragOperation = null;
                }
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
