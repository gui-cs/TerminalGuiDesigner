using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class DimExtensions
{

    public static bool IsPercent(this Dim d)
    {
        return d.GetType().Name == "DimFactor";
    }


    public static bool IsPercent(this Dim d, out float factor)
    {
        if (d.IsPercent())
        {
            var nField = d.GetType().GetField("factor", BindingFlags.NonPublic | BindingFlags.Instance);
            factor = (float)nField.GetValue(d);
            return true;
        }

        factor = 0;
        return false;
    }

    public static bool IsFill(this Dim d)
    {
        return d.GetType().Name == "DimFill";
    }

    public static bool IsFill(this Dim d, out int margin)
    {
        if (d.IsFill())
        {
            var nField = d.GetType().GetField("margin", BindingFlags.NonPublic | BindingFlags.Instance);
            margin = (int)nField.GetValue(d);
            return true;
        }

        margin = 0;
        return false;
    }


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