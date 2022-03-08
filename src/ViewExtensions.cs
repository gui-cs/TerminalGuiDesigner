using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class ViewExtensions
{
    /// <summary>
    /// Returns the subviews of <paramref name="v"/> skipping out any
    /// public views used by the Terminal.Gui API e.g. the 'ContentView'
    /// invisible sub view of the 'Window' class
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static IList<View> GetActualSubviews(this View v)
    {
        if(v is Window w)
        {
            return w.Subviews[0].Subviews;
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
}
