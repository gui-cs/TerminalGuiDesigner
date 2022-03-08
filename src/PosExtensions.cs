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
}
