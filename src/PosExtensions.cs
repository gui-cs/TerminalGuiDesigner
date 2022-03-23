using NStack;
using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public static class PosExtensions
{
    public static bool IsAbsolute(this Pos p)
    {
        return p.GetType().Name == "PosAbsolute";
    }
    public static bool IsAbsolute(this Pos p, out int n)
    {
        if(p.IsAbsolute())
        {
            var nField = p.GetType().GetField("n", BindingFlags.NonPublic | BindingFlags.Instance);
            n = (int)nField.GetValue(p);
            return true;
        }

        n = 0;
        return false;        
    }

    public static bool IsPercent(this Pos p)
    {
        return p.GetType().Name == "PosFactor";
    }

    public static bool IsPercent(this Pos p, out float percent)
    {
        if (p.IsPercent())
        {
            var nField = p.GetType().GetField("factor", BindingFlags.NonPublic | BindingFlags.Instance);
            percent = ((float)nField.GetValue(p))*100f;
            return true;
        }

        percent = 0;
        return false;
    }


    public static bool IsRelative(this Pos p, out Pos posView)
    {
        // Terminal.Gui will often use Pos.Combine with RHS of 0 instead of just PosView alone
        if(p.IsCombine(out var left, out var right, out _))
        {
            if(right.IsAbsolute(out int n) && n == 0)
            {
                return left.IsRelative(out posView);
            }
        }        
        
        if(p.GetType().Name == "PosView")
        {
            posView = p;
            return true;
        }

        posView = null;
        return false;
    }

    public static bool IsRelative(this Pos p,IList<Design> knownDesigns, out Design relativeTo, out Side side)
    {
        relativeTo = null;
        side = default;

        // Terminal.Gui will often use Pos.Combine with RHS of 0 instead of just PosView alone
        if(p.IsCombine(out var left, out var right, out _))
        {
            if(right.IsAbsolute(out var n) && n == 0)
            {
                return IsRelative(left,knownDesigns,out relativeTo,out side);
            }
        }

        if (p.IsRelative(out var posView))
        {
            var fTarget = posView.GetType().GetField("Target");
            var view = (View)fTarget.GetValue(posView);

            relativeTo = knownDesigns.FirstOrDefault(d=>d.View == view);

            // We are relative to a view that we don't have a Design for
            // sad times guess we can't describe it
            if(relativeTo == null)
                return false;

            var fSide = posView.GetType().GetField("side", BindingFlags.NonPublic | BindingFlags.Instance);
            var iSide = (int)fSide.GetValue(posView);

            side = (Side)iSide;
            return true;
        }

        return false;
    }

    public static bool IsCombine(this Pos p)
    {
        return p.GetType().Name == "PosCombine";
    }

    public static bool IsCombine(this Pos p, out Pos left,out Pos right, out bool add)
    {
        if (p.IsCombine())
        {
            var fLeft = p.GetType().GetField("left", BindingFlags.NonPublic | BindingFlags.Instance);
            left = (Pos)fLeft.GetValue(p);

            var fRight = p.GetType().GetField("right", BindingFlags.NonPublic | BindingFlags.Instance);
            right = (Pos)fRight.GetValue(p);
           
            var fAdd = p.GetType().GetField("add", BindingFlags.NonPublic | BindingFlags.Instance);
            add = (bool)fAdd.GetValue(p);
            
            return true;
        }

        left = null;
        right = null;
        add = false;
        return false;
    }


    public static bool GetPosType(this Pos p,IList<Design> knownDesigns, out PosType type,out float value,out Design relativeTo, out Side side, out int offset)
    {

        type = default;
        relativeTo = null;
        side = default;
        value = 0;
        offset = 0;

        if(p.IsAbsolute(out var n))
        {
            type = PosType.Absolute;
            value = n;
            return true;
        }

        if(p.IsRelative(knownDesigns,out relativeTo, out side))
        {
            type = PosType.Relative;
            return true;
        }

        if(p.IsPercent(out var percent))
        {
            type = PosType.Percent;
            value = percent;
            offset = 0;
            return true;
        }

        if(p.IsCombine(out var left, out var right, out var add))
        {
            // we only deal in combines if the right is an absolute
            // e.g. Pos.Percent(25) + 5 is supported but Pos.Percent(5) + Pos.Percent(10) is not
            if(right.IsAbsolute(out int rhsVal))
            {
                offset = add ? rhsVal : -rhsVal;
                GetPosType(left,knownDesigns,out type,out value,out relativeTo, out side, out _);
                return true;
            }
        }

        // we have no idea what this Pos type is
        return false;
    }



    /// <summary>
    /// <para>Returns the primitive value of <paramref name="value"/> or the inputted
    /// value if it is not possible to convert.  Supports:
    /// </para>
    /// <list type="bullet">
    /// <item>ustring to string</item>
    /// <item>Absolute Pos to int</item>
    /// <item>Absolute Dim to int</item>
    /// </list>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static object? ToPrimitive(this object? value)
    {
        if(value == null)
            return null;

        if (value is ustring u)
        {
            return u.ToString();
        }

        if (value is Pos p)
        {
            // Value is a position e.g. X=2
            // Pos can be many different subclasses all of which are public
            // lets deal with only PosAbsolute for now
            if (p.IsAbsolute(out int n))
            {
                return n;
            }
            else
                throw new NotImplementedException("Only absolute positions are supported at the moment");
        }
        else if (value is Dim d)
        {
            // Value is a position e.g. X=2
            // Pos can be many different subclasses all of which are public
            // lets deal with only PosAbsolute for now
            if (d.IsAbsolute(out int n))
            {
                return n;
            }
            else
                throw new NotImplementedException("Only absolute dimensions are supported at the moment");
        }
        else
        {
            // assume it is already a primitive
            return value;
        }
    }

    public static string? ToCode(this Pos d, List<Design> knownDesigns)
    {
        if(!d.GetPosType(knownDesigns,out var type, out var val,out var relativeTo, out var side, out var offset))
        {
            // could not determine the type
            return null;
        }

        switch (type)
        {
            case PosType.Absolute : 
                    return val.ToString();
            case PosType.Relative:
                if(offset > 0)
                    return $"Pos.{GetMethodNameFor(side)}({relativeTo.FieldName}) + {offset}";
                if(offset < 0)
                    return $"Pos.{GetMethodNameFor(side)}({relativeTo.FieldName}) - {Math.Abs(offset)}";
                return $"Pos.{GetMethodNameFor(side)}({relativeTo.FieldName})";

            case PosType.Percent:
                if(offset > 0)
                    return $"Pos.Percent({val}) + {offset}";
                if(offset < 0)
                    return $"Pos.Percent({val}) - {Math.Abs(offset)}";
                return $"Pos.Percent({val})";
            
            default: throw new ArgumentOutOfRangeException("Unknown PosType");
        }
}

    private static string GetMethodNameFor(Side side)
    {
        switch (side)
        {
            case Side.Left : return "Left";
            case Side.Right : return "Right";
            case Side.Above : return "Top";
            case Side.Below : return "Bottom";

            default: throw new ArgumentOutOfRangeException("Unknown side");
        }
    }
}
