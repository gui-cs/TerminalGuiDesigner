using System.CodeDom;
using System.Reflection;
using System.Text;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for <see cref="object"/>.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Casts <paramref name="o"/> to <paramref name="type"/>.
    /// </summary>
    /// <param name="o">The object to cast.</param>
    /// <param name="type">Type to cast to.</param>
    /// <returns>Hard typed dynamic of <paramref name="type"/>.</returns>
    public static dynamic CastToReflected(this object o, Type type)
    {
        var methodInfo = typeof(ObjectExtensions).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.NonPublic);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o }) ?? throw new Exception("Expected genericMethodInfo CastTo<T> to have a non null return value");
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="CodePrimitiveExpression"/>.  Value
    /// must be a basic Type.  This is an object that can be used to represent <paramref name="value"/>
    /// in code generated in a .Designer.cs file.
    /// </summary>
    /// <param name="value"><see cref="CodePrimitiveExpression"/> CodeDOM wrapper for <paramref name="value"/>.</param>
    /// <returns>CodeDOM object representing <paramref name="value"/>.</returns>
    public static CodePrimitiveExpression ToCodePrimitiveExpression(this object? value)
    {
        var val = ToPrimitive(value);
        return val == null ? new CodePrimitiveExpression() : new CodePrimitiveExpression(val);
    }

    /// <summary>
    /// <para>Returns the primitive value of <paramref name="value"/> or the inputted
    /// value if it is not possible to convert.  Supports:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="Rune"/> to <see cref="char"/></item>
    /// <item>Absolute <see cref="Pos"/> to int</item>
    /// <item>Absolute <see cref="Dim"/> to int</item>
    /// </list>
    /// </summary>
    /// <param name="value">Value type to convert.</param>
    /// <returns>A simpler value type for <paramref name="value"/> e.g. convert PosAbsolute to int
    /// to <see cref="string"/>.  If no conversion exists then <paramref name="value"/> is returned
    /// unchanged.</returns>
    /// <exception cref="ArgumentException">Thrown if Type is <see cref="Pos"/> or <see cref="Dim"/>
    /// but not absolute (i.e. cannot be converted to a value type - int).</exception>
    public static object? ToPrimitive(this object? value)
    {
        if (value == null)
        {
            return null;
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
            {
                throw new ArgumentException("Only absolute positions are supported at the moment");
            }
        }
        else if (value is Dim d)
        {
            // Value is a position e.g. X=2
            // Dim can be many different subclasses all of which are public
            // lets deal with only DimAbsolute for now
            if (d.IsAbsolute(out int n))
            {
                return n;
            }
            else
            {
                throw new ArgumentException("Only absolute dimensions are convertible to primitives");
            }
        }
        else
        {
            // assume it is already a primitive
            return value;
        }
    }

    private static T CastTo<T>(this object o) => (T)o;
}
