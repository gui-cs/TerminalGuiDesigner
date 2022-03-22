using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class DimExtensions
{

    public static bool IsPercent(this Dim d)
    {
        return d.GetType().Name == "DimFactor";
    }


    public static bool IsPercent(this Dim d, out float percent)
    {
        if (d.IsPercent())
        {
            var nField = d.GetType().GetField("factor", BindingFlags.NonPublic | BindingFlags.Instance);
            percent = ((float)nField.GetValue(d))*100f;
            return true;
        }

        percent = 0;
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


    public static bool IsCombine(this Dim d)
    {
        return d.GetType().Name == "DimCombine";
    }
    public static bool IsCombine(this Dim d, out Dim left,out Dim right, out bool add)
    {
        if (d.IsCombine())
        {
            var fLeft = d.GetType().GetField("left", BindingFlags.NonPublic | BindingFlags.Instance);
            left = (Dim)fLeft.GetValue(d);

            var fRight = d.GetType().GetField("right", BindingFlags.NonPublic | BindingFlags.Instance);
            right = (Dim)fRight.GetValue(d);
           
            var fAdd = d.GetType().GetField("add", BindingFlags.NonPublic | BindingFlags.Instance);
            add = (bool)fAdd.GetValue(d);
            
            return true;
        }

        left = null;
        right = null;
        add = false;
        return false;
    }

    public static bool GetDimType(this Dim d, out DimType type,out float value,out int offset)
    {
        if(d.IsAbsolute(out var n))
        {
            type = DimType.Absolute;
            value = n;
            offset = 0;
            return true;
        }

        if(d.IsFill(out var margin))
        {
            type = DimType.Fill;
            value = margin;
            offset = 0;
            return true;
        }

        if(d.IsPercent(out var percent))
        {
            type = DimType.Percent;
            value = percent;
            offset = 0;
            return true;
        }

        if(d.IsCombine(out var left, out var right, out var add))
        {
            // we only deal in combines if the right is an absolute
            // e.g. Dim.Percent(25) + 5 is supported but Dim.Percent(5) + Dim.Percent(10) is not
            if(right.IsAbsolute(out int rhsVal))
            {
                offset = add ? rhsVal : -rhsVal;
                GetDimType(left,out type,out value,out _);
                return true;
            }
        }

        // we have no idea what this Dim type is
        type = default;
        value = 0;
        offset = 0;
        return false;
    }

    public static string? GetCode(this Dim d)
    {
        if(!d.GetDimType(out var type, out var val, out var offset))
        {
            // could not determine the type
            return null;
        }

        switch (type)
        {
            case DimType.Absolute : 
                    return val.ToString();
            case DimType.Fill:
                if(offset > 0)
                    return $"Dim.Fill({val}) + {offset}";
                if(offset < 0)
                    return $"Dim.Fill({val}) - {Math.Abs(offset)}";
                return $"Dim.Fill({val})";

            case DimType.Percent:
                if(offset > 0)
                    return $"Dim.Percent({val}) + {offset}";
                if(offset < 0)
                    return $"Dim.Percent({val}) - {Math.Abs(offset)}";
                return $"Dim.Percent({val})";
            
            default: throw new ArgumentOutOfRangeException("Unknown DimType");
        }
    }
}