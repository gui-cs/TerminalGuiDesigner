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
        return view.GetType().GetProperty("Text") ?? typeof(View).GetProperty("Text");
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
            f.Text = "Heya";
        }
        else
        if (view is Button b)
        {
            b.Text = "Heya";
        }
        else
        {
            view.Text = text;
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
}
