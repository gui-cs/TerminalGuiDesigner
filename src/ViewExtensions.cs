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
        if(v is Window w)
        {
            return w.Subviews[0].Subviews;
        }

        if(v is TabView t)
        {
            return t.Tabs.Select(tab=>tab.View).Where(v=>v!=null).ToList();
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
        {
            view.Text = text;
        }
    }
    public static string GetActualText(this View view)
    {
        if (view is TextField f)
        {
            return f.Text?.ToString() ?? "";
        }
        else
        if (view is Button b)
        {
            return b.Text?.ToString() ?? "";
        }
        else
        {
            return view.Text?.ToString() ?? "";
        }
    }

    /// <summary>
    /// Some Views have hidden subviews e.g. TabView, ComboBox etc
    /// Sometimes the most focused view is a sub element.  This method
    /// goes up the hierarchy and finds the first that is designable
    /// </summary>
    public static Design? GetNearestDesign(this View view)
    {
        if(view is null)
            return null;

        if(view.Data is Design d)
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
            return null;

        if (d.IsContainerView)
            return d;

        return GetNearestContainerDesign(d.View.SuperView);
    }


    /// <summary>
    /// Converts a view-relative (col,row) position to a screen-relative position (col,row). The values are optionally clamped to the screen dimensions.
    /// </summary>
    /// <param name="col">View-relative column.</param>
    /// <param name="row">View-relative row.</param>
    /// <param name="rcol">Absolute column; screen-relative.</param>
    /// <param name="rrow">Absolute row; screen-relative.</param>
    /// <param name="clipped">Whether to clip the result of the ViewToScreen method, if set to <c>true</c>, the rcol, rrow values are clamped to the screen (terminal) dimensions (0..TerminalDim-1).</param>
    public static void ViewToScreen(this View v, int col, int row, out int rcol, out int rrow, bool clipped = true)
    {
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
}
