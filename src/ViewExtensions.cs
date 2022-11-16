using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for <see cref="View"/> <see langword="class"/>.
/// </summary>
public static class ViewExtensions
{
    /// <summary>
    /// Returns the sub-views of <paramref name="v"/> skipping out any
    /// public views used by the Terminal.Gui API e.g. the 'ContentView'
    /// invisible sub view of the 'Window' class.
    /// <para>
    /// Also unpacks Tabs in a <see cref="TabView"/>> to return all of those too.
    /// </para>
    /// </summary>
    /// <param name="v"><see cref="View"/> whose children you want to find.</param>
    /// <returns>All <see cref="View"/> that user perceives as within <paramref name="v"/> skipping over
    /// any Terminal.Gui artifacts (e.g. ContentView).</returns>
    public static IList<View> GetActualSubviews(this View v)
    {
        if (v is Window w)
        {
            return w.Subviews[0].Subviews;
        }

        if (v is ScrollView scroll)
        {
            return scroll.Subviews[0].Subviews;
        }

        if (v is TabView t)
        {
            return t.Tabs.Select(tab => tab.View).Where(v => v != null).ToList();
        }

        return v.Subviews;
    }

    /// <summary>
    /// Returns the Text property.  This deals with some Views (e.g. Button, TextField)
    /// having two Text properties (use of <see langword="new"/> keyword).
    /// <code>
    /// See https://github.com/migueldeicaza/gui.cs/issues/1619
    /// </code>
    /// </summary>
    /// <param name="view"><see cref="View"/> you want to retrieve instance member Text for
    /// (effectively treating Terminal.Gui use of <see langword="new"/> as <see langword="virtual"/>
    /// on properties).</param>
    /// <returns>Text property declared on the underlying type of <paramref name="view"/> (E.g.
    /// Button.Text not View.Text).</returns>
    public static PropertyInfo GetActualTextProperty(this View view)
    {
        return view.GetType().GetProperty("Text") ?? typeof(View).GetProperty("Text") ?? throw new Exception("Expected property 'Text' on Type 'View' was missing");
    }

    /// <summary>
    /// Sets the Text property.  This deals with some Views (e.g. Button, TextField)
    /// having two Text properties (use of <see langword="new"/> keyword).
    /// <code>
    /// See https://github.com/migueldeicaza/gui.cs/issues/1619
    /// </code>
    /// </summary>
    /// <param name="view"><see cref="View"/> to set Text property on.</param>
    /// <param name="text">Value to set.</param>
    public static void SetActualText(this View view, string text)
    {
        if (view is TextField f)
        {
            f.Text = text;
        }
        else
        if (view is Button b)
        {
            b.Text = text;
        }
        else
        if (view is CheckBox cb)
        {
            cb.Text = text;
        }
        else
        if (view is FrameView fr)
        {
            fr.Title = text;
        }
        else
        if (view is Window w)
        {
            w.Title = text;
        }
        else
        {
            view.Text = text;
        }
    }

    /// <summary>
    /// <para>
    /// Returns the <see cref="View.Text"/> of <paramref name="view"/> in a way
    /// that avoids the Terminal.Gui casting issues around use of the <see langword="new"/>
    /// keyword on this property.
    /// </para>
    /// <para>
    /// See https://github.com/gui-cs/Terminal.Gui/issues/1619 for more info.
    /// </para>
    /// </summary>
    /// <param name="view">The <see cref="View"/> whose text you want.</param>
    /// <returns>Text of <paramref name="view"/>.</returns>
    public static string GetActualText(this View view)
    {
        if (view is TextField f)
        {
            return f.Text?.ToString() ?? string.Empty;
        }
        else
        if (view is Button b)
        {
            return b.Text?.ToString() ?? string.Empty;
        }
        else
        if (view is CheckBox cb)
        {
            return cb.Text?.ToString() ?? string.Empty;
        }
        else
        {
            return view.Text?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// <para>Some Views have hidden sub-views e.g. TabView, ComboBox etc
    /// Sometimes the most focused view is a sub element.  This method
    /// returns the <see cref="Design"/> for <paramref name="view"/> or goes up
    /// the hierarchy and finds the first parent that is designable.
    /// </para>
    /// <para>This method differs from <see cref="GetNearestContainerDesign(View)"/>
    /// because it will first check if <paramref name="view"/> itself has a
    /// <see cref="Design"/> and returns that if so.</para>
    /// </summary>
    /// <param name="view">The <see cref="View"/> you want the nearest <see cref="Design"/> to.</param>
    /// <returns><see cref="Design"/> associated with <paramref name="view"/> or its
    /// closest designed parent.</returns>
    public static Design? GetNearestDesign(this View view)
    {
        if (view is null)
        {
            return null;
        }

        if (view.Data is Design d)
        {
            return d;
        }

        return GetNearestDesign(view.SuperView);
    }

    /// <summary>
    /// Travels up the nested views until it finds one that is
    /// <see cref="Design.IsContainerView"/> or returns null if
    /// none are.
    /// </summary>
    /// <param name="v">The <see cref="View"/> you want to find parent container of.</param>
    /// <returns>Wrapper for the nearest parental <see cref="View"/> that user added
    /// (i.e. not a Terminal.Gui internal artifact) view.</returns>
    public static Design? GetNearestContainerDesign(this View v)
    {
        var d = GetNearestDesign(v);

        if (d == null)
        {
            return null;
        }

        if (d.IsContainerView)
        {
            return d;
        }

        return GetNearestContainerDesign(d.View.SuperView);
    }

    /// <summary>
    /// <para>
    /// Converts a view-relative (col,row) position to a screen-relative position (col,row). The values are optionally clamped to the screen dimensions.
    /// </para>
    /// <para>This method differs from the private method in Terminal.Gui because it will unwrap private views e.g. <see cref="Window"/> such that the real
    /// client coordinates of children are returned (e.g. see <see cref="GetActualSubviews(View)"/>).</para>
    /// </summary>
    /// <param name="v">The view that you want to translate client coordinates for.</param>
    /// <param name="col">View-relative column.</param>
    /// <param name="row">View-relative row.</param>
    /// <param name="rcol">Absolute column; screen-relative.</param>
    /// <param name="rrow">Absolute row; screen-relative.</param>
    /// <param name="clipped">Whether to clip the result of the ViewToScreen method, if set to <c>true</c>, the rcol, rrow values are clamped to the screen (terminal) dimensions (0..TerminalDim-1).</param>
    public static void ViewToScreenActual(this View v, int col, int row, out int rcol, out int rrow, bool clipped = true)
    {
        if (v is Window || v is FrameView)
        {
            v.Subviews[0].ViewToScreenActual(col, row, out rcol, out rrow, clipped);
            return;
        }

        // Computes the real row, col relative to the screen.
        rrow = row + v.Frame.Y;
        rcol = col + v.Frame.X;
        var ccontainer = v.SuperView;
        while (ccontainer != null)
        {
            rrow += ccontainer.Frame.Y;
            rcol += ccontainer.Frame.X;
            ccontainer = ccontainer.SuperView;
        }

        // The following ensures that the cursor is always in the screen boundaries.
        if (clipped)
        {
            rrow = Math.Min(rrow, Application.Driver.Rows - 1);
            rcol = Math.Min(rcol, Application.Driver.Cols - 1);
        }
    }

    /// <summary>
    /// Returns true if <paramref name="v"/> is a Type that is designed to store other
    /// sub-views in it (<see cref="Window"/>, <see cref="FrameView"/> etc).
    /// </summary>
    /// <param name="v">The <see cref="View"/> to classify.</param>
    /// <returns>True if <paramref name="v"/> is designed to host sub-controls.</returns>
    public static bool IsContainerView(this View v)
    {
        var type = v.GetType();

        if (v.Data is Design d)
        {
            // The root class user is designing (e.g. MyView) could be inheriting from
            // TopLevel or View in which case we must allow dropping into it
            if (d.IsRoot)
            {
                return true;
            }
        }

        // TODO: are there any others?
        return
            v is ScrollView ||
            v is TabView ||
            v is FrameView ||
            v is Window ||
            type == typeof(View) || type.Name.Equals("ContentView");
    }

    /// <summary>
    /// True if <paramref name="v"/> has no visible border and is designed
    /// to contain other views.
    /// </summary>
    /// <param name="v"><see cref="View"/> to classify.</param>
    /// <returns>True if no visible border and <see cref="ViewExtensions.IsContainerView(View)"/>.</returns>
    public static bool IsBorderlessContainerView(this View v)
    {
        if (v is TabView tabView)
        {
            return !tabView.Style.ShowBorder || tabView.Style.TabsOnBottom;
        }

        if (v.IsContainerView() && v.HasNoBorderProperty())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds the deepest view at <paramref name="m"/> screen coordinates.
    /// </summary>
    /// <param name="w">Any <see cref="View"/> which is visible and mounted below
    /// <see cref="Application.Top"/>.</param>
    /// <param name="m">Screen coordinates.</param>
    /// <param name="isBorder">True if the click lands on the border of the returned <see cref="View"/>.</param>
    /// <param name="isLowerRight">True if the click lands in the lower right of the returned <see cref="View"/>.</param>
    /// <param name="ignoring">One or more <see cref="View"/> to ignore (click through) when performing the hit test.</param>
    /// <returns>The <see cref="View"/> at the given screen location or null if none found.</returns>
    public static View? HitTest(this View w, MouseEvent m, out bool isBorder, out bool isLowerRight, params View[] ignoring)
    {
        // hide the views while we perform the hit test
        foreach (View v in ignoring)
        {
            v.Visible = false;
        }

        var point = w.ScreenToView(m.X, m.Y);
        var hit = ApplicationExtensions.FindDeepestView(w, m.X, m.Y);

        int resizeBoxArea = 2;

        if (hit != null)
        {
            var screenFrame = hit.FrameToScreen();

            isLowerRight = Math.Abs(screenFrame.X + screenFrame.Width - point.X) <= resizeBoxArea
                && Math.Abs(screenFrame.Y + screenFrame.Height - point.Y) <= resizeBoxArea;

            isBorder =
                m.X == screenFrame.X + screenFrame.Width - 1 ||
                m.X == screenFrame.X ||
                m.Y == screenFrame.Y + screenFrame.Height - 1 ||
                m.Y == screenFrame.Y;
        }
        else
        {
            isLowerRight = false;
            isBorder = false;
        }

        // hide the views while we perform the hit test
        foreach (View v in ignoring)
        {
            v.Visible = true;
        }

        return hit;
    }

    /// <summary>
    /// Returns true if the current screen bounds of <paramref name="v"/> intersect
    /// with the <paramref name="screenRect"/> rectangle.
    /// </summary>
    /// <param name="v"><see cref="View"/> whose bounds will be intersected with <paramref name="screenRect"/>.</param>
    /// <param name="screenRect"><see cref="Rect"/> to intersect with <paramref name="v"/>.</param>
    /// <returns>True if the client area intersects.</returns>
    public static bool IntersectsScreenRect(this View v, Rect screenRect)
    {
        // TODO: maybe this should use Frame instead? Currently this will not let you drag box
        // selection over the border of a container to select it (e.g. FrameView).
        v.ViewToScreenActual(0, 0, out var x0, out var y0);
        v.ViewToScreenActual(v.Bounds.Width, v.Bounds.Height, out var x1, out var y1);

        return Rect.FromLTRB(x0, y0, x1, y1).IntersectsWith(screenRect);
    }

    /// <summary>
    /// <para>Returns the explicitly defined private ColorScheme on the view
    /// Or null if it inherits it from its parent or a global scheme.</para>
    /// <para>
    /// The private backing field value for <see cref="View.ColorScheme"/> is
    /// queried with reflection.  This is necessary because <see cref="View.ColorScheme"/> getter
    /// returns from parent (inherited) if setter has not been called but we want to know
    /// if the <see cref="View"/> really has a 'user intended' scheme assigned.
    /// </para>
    /// </summary>
    /// <param name="v">The <see cref="View"/> you want to get 'user intended' explicitly set
    /// <see cref="ColorScheme"/> for.</param>
    /// <returns>The value that was used to call <see cref="View.ColorScheme"/> setter or null
    /// if never called (i.e. <see cref="View.ColorScheme"/> getter is returning inherited parent value).</returns>
    public static ColorScheme? GetExplicitColorScheme(this View v)
    {
        var explicitColorSchemeField = typeof(View).GetField("colorScheme", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new Exception("ColorScheme private backing field no longer exists");

        return (ColorScheme?)explicitColorSchemeField.GetValue(v);
    }

    /// <summary>
    /// Order the passed views from top left to bottom right
    /// (helps ensure a sensible Tab order).
    /// </summary>
    /// <param name="views">Collection of <see cref="View"/> to order.</param>
    /// <returns>Collection ordered by screen position.</returns>
    public static IEnumerable<View> OrderViewsByScreenPosition(IEnumerable<View> views)
    {
        // always put MenuBar last so that it appears top in z order
        // but order everything else top left to bottom right
        return views
            .OrderBy(v => v is MenuBar ? int.MaxValue : 0)
            .ThenBy(v => v.Frame.Y)
            .ThenBy(v => v.Frame.X);
    }

    /// <summary>
    /// Returns <see cref="View.Frame"/> in screen coordinates.  For tests ensure you
    /// have run <see cref="View.LayoutSubviews"/> and that <paramref name="view"/> has
    /// a route to <see cref="Application.Top"/> (e.g. is showing or <see cref="Application.Begin(Toplevel)"/>).
    /// </summary>
    /// <param name="view">The view you want to translate coordinates for.</param>
    /// <returns>Screen coordinates of <paramref name="view"/>'s <see cref="View.Frame"/>.</returns>
    public static Rect FrameToScreen(this View view)
    {
        int x = 0;
        int y = 0;

        var current = view;

        while (current != null)
        {
            x += current.Frame.X;
            y += current.Frame.Y;
            current = current.SuperView;
        }

        return new Rect(x, y, view.Frame.Width, view.Frame.Height);
    }

    private static bool HasNoBorderProperty(this View v)
    {
        if (v.Border == null)
        {
            return true;
        }

        if (v.Border.BorderStyle == BorderStyle.None)
        {
            return true;
        }

        return false;
    }
}
