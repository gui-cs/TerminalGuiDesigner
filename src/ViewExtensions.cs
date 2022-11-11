using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class ViewExtensions
{
    /// <summary>
    /// Returns the subviews of <paramref name="v"/> skipping out any
    /// public views used by the Terminal.Gui API e.g. the 'ContentView'
    /// invisible sub view of the 'Window' class
    /// <para>
    /// Also unpacks Tabs in a <see cref="TabView"/>> to return all of those too
    /// </para>
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
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
    /// having two Text properties (use of `new` keyword).
    ///
    /// <para>
    /// See https://github.com/migueldeicaza/gui.cs/issues/1619
    /// </para>
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    public static PropertyInfo GetActualTextProperty(this View view)
    {
        return view.GetType().GetProperty("Text") ?? typeof(View).GetProperty("Text") ?? throw new Exception("Expected property 'Text' on Type 'View' was missing");
    }

    /// <summary>
    /// Sets the Text property.  This deals with some Views (e.g. Button, TextField)
    /// having two Text properties (use of `new` keyword).
    ///
    /// <para>
    /// See https://github.com/migueldeicaza/gui.cs/issues/1619
    /// </para>
    /// </summary>
    /// <param name="view"></param>
    /// <param name="text"></param>
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
    /// Some Views have hidden subviews e.g. TabView, ComboBox etc
    /// Sometimes the most focused view is a sub element.  This method
    /// goes up the hierarchy and finds the first that is designable
    /// </summary>
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
    /// none are
    /// </summary>
    /// <returns></returns>
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

        // get nearest Design to the clicked thing (avoids issues with ContentView and other wierd subviews etc)
        hit = hit?.GetNearestDesign()?.View;

        if (hit != null)
        {
            hit.ViewToScreenActual(hit.Bounds.Right, hit.Bounds.Bottom, out int lowerRightX, out int lowerRightY, true);
            hit.ViewToScreenActual(0, 0, out int upperLeftX, out int upperLeftY, true);

            isLowerRight = Math.Abs(lowerRightX - point.X) <= resizeBoxArea && Math.Abs(lowerRightY - point.Y) <= resizeBoxArea;

            isBorder =
                m.X == lowerRightX - 1 ||
                m.X == upperLeftX ||
                m.Y == lowerRightY - 1 ||
                m.Y == upperLeftY;
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
    /// <param name="v"></param>
    /// <param name="screenRect"></param>
    /// <returns></returns>
    public static bool IntersectsScreenRect(this View v, Rect screenRect)
    {
        v.ViewToScreenActual(0, 0, out var x0, out var y0);
        v.ViewToScreenActual(v.Bounds.Width, v.Bounds.Height, out var x1, out var y1);

        return Rect.FromLTRB(x0, y0, x1, y1).IntersectsWith(screenRect);
    }

    /// <summary>
    /// Returns the explicitly defined private ColorScheme on the view
    /// Or null if it inherits it from its parent or a globa scheme
    /// </summary>
    /// <returns></returns>
    public static ColorScheme? GetExplicitColorScheme(this View v)
    {
        var explicitColorSchemeField = typeof(View).GetField("colorScheme", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new Exception("ColorScheme private backing field no longer exists");

        return (ColorScheme?)explicitColorSchemeField.GetValue(v);
    }

    /// <summary>
    /// Order the passed views from top left to bottom right
    /// (helps ensure a sensible Tab order)
    /// </summary>
    public static IEnumerable<View> OrderViewsByScreenPosition(IEnumerable<View> views)
    {
        return views.OrderBy(v => v.Frame.Y).ThenBy(v => v.Frame.X);
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
