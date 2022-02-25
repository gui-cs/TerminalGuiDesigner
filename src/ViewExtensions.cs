using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class ViewExtensions
{
    public static IList<View> GetActualSubviews(this View v)
    {
        if(v is Window w)
        {
            return w.Subviews[0].Subviews;
        }
        return v.Subviews;
    }
}
