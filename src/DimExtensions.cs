using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class DimExtensions
{

    private static bool TreatNullDimAs0 = true;

    public static bool IsPercent(this Dim d)
    {
        if (d == null)
            return false;

        return d.GetType().Name == "DimFactor";
    }


    public static bool IsPercent(this Dim d, out float percent)
    {
        if (d != null && d.IsPercent())
        {
            var nField = d.GetType().GetField("factor", BindingFlags.NonPublic | BindingFlags.Instance) 
                ?? throw new Exception("Expected private field 'factor' of DimPercent was missing");
            percent = ((float?)nField.GetValue(d) ?? throw new Exception("Expected private field 'factor' to be a float"))*100f;
            return true;
        }

        percent = 0;
        return false;
    }

    public static bool IsFill(this Dim d)
    {
        if (d == null)
            return false;

        return d.GetType().Name == "DimFill";
    }

    public static bool IsFill(this Dim d, out int margin)
    {
        if (d != null && d.IsFill())
        {
            var nField = d.GetType().GetField("margin", BindingFlags.NonPublic | BindingFlags.Instance)
                 ?? throw new Exception("Expected private field 'margin' of DimFill was missing"); ;
            margin = (int?)nField.GetValue(d) ?? throw new Exception("Expected private field 'margin' of DimFill had unexpected Type");
            return true;
        }

        margin = 0;
        return false;
    }


    public static bool IsAbsolute(this Dim d)
    {
        if (d == null)
            return TreatNullDimAs0;

        return d.GetType().Name == "DimAbsolute";
    }
    public static bool IsAbsolute(this Dim d, out int n)
    {
        if (d.IsAbsolute())
        {

            if (d == null)
            {
                n = 0;
                return TreatNullDimAs0;
            }

            var nField = d.GetType().GetField("n", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception("Expected private field was missing from DimAbsolute");
            n = (int?)nField.GetValue(d) 
                ?? throw new Exception("Expected private field 'n' to be in int for DimAbsolute");
            return true;
        }

        n = 0;
        return false;
    }


    public static bool IsCombine(this Dim d)
    {
        if (d == null)
            return false;

        return d.GetType().Name == "DimCombine";
    }
    public static bool IsCombine(this Dim d, out Dim left,out Dim right, out bool add)
    {
        if (d.IsCombine())
        {
            var fLeft = d.GetType().GetField("left", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Expected private field was missing from Dim.Combine");
            left = fLeft.GetValue(d) as Dim ?? throw new Exception("Expected private field in DimCombine to be of Type Dim");

            var fRight = d.GetType().GetField("right", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Expected private field was missing from Dim.Combine");
            right = fRight.GetValue(d) as Dim ?? throw new Exception("Expected private field in DimCombine to be of Type Dim");
           
            var fAdd = d.GetType().GetField("add", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Expected private field was missing from Dim.Combine"); ;
            add = fAdd.GetValue(d) as bool? ?? throw new Exception("Expected private field in DimCombine to be of Type bool"); ;
            
            return true;
        }

        left = 0;
        right = 0;
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

    public static string? ToCode(this Dim d)
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
                    return $"Dim.Percent({val:G5}f) + {offset}";
                if(offset < 0)
                    return $"Dim.Percent({val:G5}f) - {Math.Abs(offset)}";

                return $"Dim.Percent({val:G5}f)";
            
            default: throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}