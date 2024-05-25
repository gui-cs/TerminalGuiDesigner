using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// <para>Extension methods for the <see cref="Dim"/> class.  Adds discovery of what the underlying type
/// is (e.g. DimCombine, DimFactor) as well as 'to code' methods.
/// </para>
/// <para>
/// Methods in this class make heavy use of <see cref="System.Reflection"/> and so may be brittle if changes
/// are made to the main Terminal.Gui private API.
/// </para>
/// </summary>
public static class DimExtensions
{
    private const bool TreatNullDimAs0 = true;

    /// <summary>
    /// Returns true if the <paramref name="d"/> is a DimFactor (i.e. created by <see cref="Dim.Percent(float, bool)"/>).
    /// </summary>
    /// <param name="d">Dimension to determine Type.</param>
    /// <returns>true if <paramref name="d"/> is DimFactor.</returns>
    public static bool IsPercent(this Dim d)
    {
        if (d == null)
        {
            return false;
        }

        return d is DimPercent;
    }

    /// <inheritdoc cref="IsPercent(Dim)"/>
    /// <param name="d">The <see cref="Dim"/> to determine whether it represents a percent.</param>
    /// <param name="percent">The 'percentage' value of <paramref name="d"/>.  This is the value that would/could be
    /// passed to <see cref="Dim.Percent(float, bool)"/> to produce the <paramref name="d"/> or 0 if <paramref name="d"/> is
    /// not DimFactor.</param>
    public static bool IsPercent(this Dim d, out float percent)
    {
        if (d != null && d.IsPercent())
        {
            var dp = (DimPercent)d;
            percent = dp.Percent;
            return true;
        }

        percent = 0;
        return false;
    }

    /// <summary>
    /// Determines whether an input <see cref="Dim"/> is a <see cref="Dim.Fill(int)"/>.
    /// </summary>
    /// <param name="d">Input <see cref="Dim"/> to classify.</param>
    /// <returns><see langword="true"/> if <paramref name="d"/> is the result of a <see cref="Dim.Fill(int)"/> call.</returns>
    public static bool IsFill(this Dim d)
    {
        if (d == null)
        {
            return false;
        }

        return d is DimFill;
    }

    /// <inheritdoc cref="IsFill(Dim)"/>
    /// <param name="d">Input <see cref="Dim"/> to classify.</param>
    /// <param name="margin">The margin of the <see cref="Dim.Fill(int)"/> used to create <paramref name="d"/> or 0.</param>
    public static bool IsFill(this Dim d, out int margin)
    {
        if (d != null && d.IsFill())
        {
            var df = (DimFill)d;
            margin = df.Margin;
            return true;
        }

        margin = 0;
        return false;
    }

    /// <summary>
    /// True if <paramref name="d"/> is an absolute width/height.
    /// </summary>
    /// <param name="d">The <see cref="Dim"/> to determine whether it is absolute.</param>
    /// <returns><see langword="true"/> if <paramref name="d"/> is a fixed absolute number.</returns>
    public static bool IsAbsolute(this Dim d)
    {
        if (d == null)
        {
            return TreatNullDimAs0;
        }

        return d is DimAbsolute;
    }

    /// <inheritdoc cref="IsAbsolute(Dim)"/>
    /// <param name="d">The <see cref="Dim"/> to determine whether it is absolute.</param>
    /// <param name="n">The value of the fixed number absolute <see cref="Dim"/> value or 0.</param>
    public static bool IsAbsolute(this Dim d, out int n)
    {
        if (d.IsAbsolute())
        {
            if (d == null)
            {
                n = 0;
                return TreatNullDimAs0;
            }

            var da = (DimAbsolute)d;
            n = da.Size;
            return true;
        }

        n = 0;
        return false;
    }

    /// <summary>
    /// True if the <paramref name="d"/> is the combination of 2 other <see cref="Dim"/>
    /// objects e.g.:<code>Dim.Percent(10)+Dim.Percent(5)</code>
    /// </summary>
    /// <param name="d">Input <see cref="Dim"/> you want classified.</param>
    /// <returns><see langword="true"/> if <paramref name="d"/> is a combination of 2 other <see cref="Dim"/>.</returns>
    public static bool IsCombine(this Dim d)
    {
        if (d == null)
        {
            return false;
        }
        return d is DimCombine;
    }

    /// <summary>
    /// Returns true if <paramref name="d"/> is the product of an addition or subtraction of
    /// two other dimensions (e.g. Dim.Fill() - 1).
    /// </summary>
    /// <param name="d">The <see cref="Dim"/> to examine.</param>
    /// <param name="left">The left hand side of the addition/subtraction calculation.  May itself be another DimCombine.  Or 0 if <paramref name="d"/> is not a DimCombine.</param>
    /// <param name="right">The right hand side of the addition/subtraction calculation.  May itself be another DimCombine.  Or 0 if <paramref name="d"/> is not a DimCombine.</param>
    /// <param name="add">True if <paramref name="d"/> is a DimCombine and the calculation is left+right.  False if calculation is subtraction or <paramref name="d"/> is not a DimCombine.</param>
    /// <returns>True if <paramref name="d"/> is a summation or subtraction of two other <see cref="Dim"/>.</returns>
    /// <exception cref="Exception">Thrown if private Terminal.Gui API changes have taken place and this implementation is therefore broken.</exception>
    public static bool IsCombine(this Dim d, out Dim left, out Dim right, out bool add)
    {
        if (d.IsCombine())
        {
            var dc = (DimCombine)d;

            left = dc.Left;
            right = dc.Right;
            add = dc.Add == AddOrSubtract.Add;

            return true;
        }

        left = 0;
        right = 0;
        add = false;
        return false;
    }

    /// <summary>
    /// Evaluates a <see cref="Dim"/> and returns what <see cref="DimType"/> it is and whether it was possible
    /// to come to a definitive answer.
    /// </summary>
    /// <param name="d">The <see cref="Dim"/> to determine type of.</param>
    /// <param name="type">The determined type.</param>
    /// <param name="value">The numerical element of the type e.g. for <see cref="Dim.Percent(float, bool)"/>
    /// the <paramref name="value"/> is the percentage but for <see cref="Dim.Fill(int)"/> the <paramref name="value"/>
    /// is the margin.</param>
    /// <param name="offset">The numerical offset if any, for example -5 in the following:
    /// <code>Dim.Fill(1)-5</code></param>
    /// <returns>True if it was possible to determine <paramref name="type"/>.</returns>
    public static bool GetDimType(this Dim d, out DimType type, out float value, out int offset)
    {
        if (d.IsAbsolute(out var n))
        {
            type = DimType.Absolute;
            value = n;
            offset = 0;
            return true;
        }

        if (d.IsFill(out var margin))
        {
            type = DimType.Fill;
            value = margin;
            offset = 0;
            return true;
        }

        if (d.IsPercent(out var percent))
        {
            type = DimType.Percent;
            value = percent;
            offset = 0;
            return true;
        }

        if (d.IsCombine(out var left, out var right, out var add))
        {
            // we only deal in combines if the right is an absolute
            // e.g. Dim.Percent(25) + 5 is supported but Dim.Percent(5) + Dim.Percent(10) is not
            if (right.IsAbsolute(out int rhsVal))
            {
                offset = add ? rhsVal : -rhsVal;
                GetDimType(left, out type, out value, out _);
                return true;
            }
        }

        // we have no idea what this Dim type is
        type = default;
        value = 0;
        offset = 0;
        return false;
    }

    /// <summary>
    /// Generates code to create <paramref name="d"/>.
    /// </summary>
    /// <param name="d"><see cref="Dim"/> to generate code snippet for.</param>
    /// <returns>The code required to create <paramref name="d"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="d"/> is
    /// determined to be a <see cref="DimType"/> that is not yet supported.</exception>
    public static string? ToCode(this Dim d)
    {
        if (!d.GetDimType(out var type, out var val, out var offset))
        {
            // TODO: This is currently unreachable (though a TG change could make it not so)
            // Would it maybe be a better idea to throw an exception, so generated code isn't silently incorrect?
            // could not determine the type
            return null;
        }

        switch (type)
        {
            case DimType.Absolute:
                return val.ToString();
            case DimType.Fill:
                if (offset > 0)
                {
                    return $"Dim.Fill({val}) + {offset}";
                }

                if (offset < 0)
                {
                    return $"Dim.Fill({val}) - {Math.Abs(offset)}";
                }

                return $"Dim.Fill({val})";

            case DimType.Percent:
                if (offset > 0)
                {
                    return $"Dim.Percent({val:G5}) + {offset}";
                }

                if (offset < 0)
                {
                    return $"Dim.Percent({val:G5}) - {Math.Abs(offset)}";
                }

                return $"Dim.Percent({val:G5})";

            default: throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}