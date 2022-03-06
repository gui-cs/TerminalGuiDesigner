using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class DimExtensions
{
    public static bool IsAbsolute(this Dim d)
    {
        return d.GetType().Name == "DimAbsolute";
    }
    public static bool IsAbsolute(this Dim d, out int n)
    {
        if (d.IsAbsolute())
        {
            var nField = d.GetType().GetField("n", BindingFlags.NonPublic | BindingFlags.Instance);
            n = (int)nField.GetValue(d);
            return true;
        }

        n = 0;
        return false;
    }
}