using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for <see cref="Pos"/>.
/// </summary>
public static class PosExtensions
{
    /// <summary>
    /// When <see cref="Pos"/> is null should it be treated as 0 instead.
    /// </summary>
    private const bool TreatNullPosAs0 = true;

    /// <summary>
    /// Returns true if <paramref name="p"/> is an absolute fixed position.
    /// </summary>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <returns>True if <paramref name="p"/> is absolute.</returns>
    public static bool IsAbsolute(this Pos? p)
    {
        if (p == null)
        {
            return TreatNullPosAs0;
        }

        return p.GetType().Name == "PosAbsolute";
    }

    /// <inheritdoc cref="IsAbsolute(Pos)"/>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <param name="n">The absolute size or 0.</param>
    /// <returns>True if <paramref name="p"/> is absolute.</returns>
    public static bool IsAbsolute(this Pos? p, out int n)
    {
        if (p.IsAbsolute())
        {
            if (p == null)
            {
                n = 0;
                return TreatNullPosAs0;
            }

            var nField = p.GetType().GetField("_n", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception("Expected private field '_n' of PosAbsolute was missing");
            n = (int?)nField.GetValue(p)
                ?? throw new Exception("Expected private field '_n' of PosAbsolute to be int");
            return true;
        }

        n = 0;
        return false;
    }

    /// <summary>
    /// Returns true if <paramref name="p"/> is a percentage position (See <see cref="Pos.Percent(float)"/>).
    /// </summary>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <returns>True if <paramref name="p"/> is <see cref="Pos.Percent(float)"/>.</returns>
    public static bool IsPercent(this Pos? p)
    {
        if (p == null)
        {
            return false;
        }

        return p.GetType().Name == "PosFactor";
    }

    /// <inheritdoc cref="IsPercent(Pos)"/>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <param name="percent">The percentage number (typically out of 100) that could be passed
    /// to <see cref="Pos.Percent(float)"/> to produce <paramref name="p"/> or 0 if <paramref name="p"/>
    /// is not a percent <see cref="Pos"/>.</param>
    /// <returns>True if <paramref name="p"/> is <see cref="Pos.Percent(float)"/>.</returns>
    public static bool IsPercent(this Pos? p, out float percent)
    {
        if (p != null && p.IsPercent())
        {
            var nField = p.GetType().GetField("factor", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception("Expected private field 'factor' was missing from PosFactor");
            percent = ((float?)nField.GetValue(p)
                ?? throw new Exception("Expected private field 'factor' of PosFactor to be float"))
                * 100f;
            return true;
        }

        percent = 0;
        return false;
    }

    /// <summary>
    /// Returns true if <paramref name="p"/> is <see cref="Pos.Center"/>.
    /// </summary>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <returns>True if <see cref="Pos.Center"/>.</returns>
    public static bool IsCenter(this Pos? p)
    {
        if (p == null)
        {
            return false;
        }

        return p.GetType().Name == "PosCenter";
    }

    /// <summary>
    /// Returns true if <paramref name="p"/> is the result of a <see cref="Pos.AnchorEnd(int)"/>.
    /// </summary>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <returns>True if <paramref name="p"/> is <see cref="Pos.AnchorEnd(int)"/>.</returns>
    // BUG: This should not return true on null, because 0 is an absolute Pos
    public static bool IsAnchorEnd(this Pos? p)
    {
        if (p == null)
        {
            return TreatNullPosAs0;
        }

        return p.GetType().Name == "PosAnchorEnd";
    }

    /// <inheritdoc cref="IsAnchorEnd(Pos)"/>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <param name="margin">The margin passed to <see cref="Pos.AnchorEnd(int)"/>. Should typically
    /// be 1 or more otherwise things tend to drift off-screen.</param>
    /// <returns></returns>
    public static bool IsAnchorEnd(this Pos? p, out int margin)
    {
        if (p.IsAnchorEnd())
        {
            if (p == null)
            {
                margin = 0;
                return TreatNullPosAs0;
            }

            var nField = p.GetType().GetField("n", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception("Expected private field 'n' of PosAbsolute was missing");
            margin = (int?)nField.GetValue(p)
                ?? throw new Exception("Expected private field 'n' of PosAbsolute to be int");
            return true;
        }

        margin = 0;
        return false;
    }

    /// <summary>
    /// True if <paramref name="p"/> is <see cref="PosType.Relative"/> i.e. the result of
    /// <see cref="Pos.Right(View)"/>, <see cref="Pos.Bottom(View)"/> etc.
    /// </summary>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <returns>True if <paramref name="p"/> is the result of a call to one of the relative methods
    /// (e.g. <see cref="Pos.Right(View)"/>).</returns>
    public static bool IsRelative(this Pos? p)
    {
        return p.IsRelative(out _);
    }

    /// <inheritdoc cref="IsRelative(Pos)"/>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <param name="knownDesigns">All <see cref="Design"/> that we might be expressed as relative to (e.g. see <see cref="Design.GetAllDesigns"/>).</param>
    /// <param name="relativeTo">The wrapper for the <see cref="View"/> that <paramref name="p"/> is relative to (i.e. what was passed to <see cref="Pos.Left(View)"/> etc).
    /// or null if <paramref name="p"/> is not <see cref="PosType.Relative"/>.</param>
    /// <param name="side"><see cref="Enum"/> representing the method that was used e.g. <see cref="Pos.Right(View)"/>, <see cref="Pos.Left(View)"/> etc.</param>
    /// <returns></returns>
    // BUG: Side should be nullable, because it gets an explicit value in all cases but isn't meaningful if return value was false
    public static bool IsRelative(this Pos? p, IList<Design> knownDesigns, [NotNullWhen(true)]out Design? relativeTo, out Side side)
    {
        relativeTo = null;
        side = default;

        // Terminal.Gui will often use Pos.Combine with RHS of 0 instead of just PosView alone
        if (p.IsCombine(out var left, out var right, out _))
        {
            if (right.IsAbsolute(out var n) && n == 0)
            {
                return IsRelative(left, knownDesigns, out relativeTo, out side);
            }
        }

        if (p.IsRelative(out var posView))
        {
            var fTarget = posView.GetType().GetField("Target") ?? throw new Exception("PosView was missing expected field 'Target'");
            View view = (View?)fTarget.GetValue(posView) ?? throw new Exception("PosView had a null 'Target' view");

            relativeTo = knownDesigns.FirstOrDefault(d => d.View == view);

            // We are relative to a view that we don't have a Design for
            // sad times guess we can't describe it
            if (relativeTo == null)
            {
                return false;
            }

            var fSide = posView.GetType().GetField("side", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("PosView was missing expected field 'side'");
            var iSide = (int?)fSide.GetValue(posView)
                ?? throw new Exception("Expected PosView property 'side' to be of Type int");

            side = (Side)iSide;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if <paramref name="p"/> is the summation or subtraction of two
    /// other <see cref="Pos"/>.
    /// </summary>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <returns>True if <paramref name="p"/> is a PosCombine.</returns>
    public static bool IsCombine( [NotNullWhen( true )] Pos? p )
    {
        if ( p == null )
        {
            return false;
        }

        return p.GetType( ).Name == "PosCombine";
    }

    /// <inheritdoc cref="IsCombine(Pos)"/>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <param name="left">The left hand operand of the summation/subtraction.</param>
    /// <param name="right">The right hand operand of the summation/subtraction.</param>
    /// <param name="add"><see langword="true"/> if addition or <see langword="false"/> if subtraction.</param>
    /// <returns>True if <paramref name="p"/> is PosCombine.</returns>
    public static bool IsCombine(this Pos? p, out Pos left, out Pos right, out bool add)
    {
        if (IsCombine(p))
        {
            var fLeft = p.GetType().GetField("left", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Expected private field missing from PosCombine");
            left = fLeft.GetValue(p) as Pos ?? throw new Exception("Expected field 'left' of PosCombine to be a Pos");

            var fRight = p.GetType().GetField("right", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Expected private field missing from PosCombine");
            right = fRight.GetValue(p) as Pos ?? throw new Exception("Expected field 'right' of PosCombine to be a Pos");

            var fAdd = p.GetType().GetField("add", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Expected private field missing from PosCombine");
            add = fAdd.GetValue(p) as bool? ?? throw new Exception("Expected field 'add' of PosCombine to be a bool");

            return true;
        }

        left = 0;
        right = 0;
        add = false;
        return false;
    }

    /// <summary>
    /// Determines the <see cref="PosType"/> of an unknown <see cref="Pos"/> instance.
    /// </summary>
    /// <param name="p">The <see cref="Pos"/> for which you want to determine type.</param>
    /// <param name="knownDesigns">Designs in the current scope, for populating <paramref name="relativeTo"/> in the case of <see cref="PosType.Relative"/>.</param>
    /// <param name="type">The type we determined <paramref name="p"/> to be.</param>
    /// <param name="value">Measure for the Type e.g. for <see cref="PosType.Percent"/> this is the percent value, for <see cref="PosType.Absolute"/> it is the absolute positional value.</param>
    /// <param name="relativeTo">Only populated for <see cref="PosType.Relative"/>, this is the <see cref="Design"/> that <paramref name="p"/> positions itself relative to.</param>
    /// <param name="side">Only populated for <see cref="PosType.Relative"/>, this is the direction of offset from <paramref name="relativeTo"/>.</param>
    /// <param name="offset">The offset from the listed position.  Is provided if the input has addition/subtraction e.g.<code>Pos.Center() + 2</code></param>
    /// <returns>True if it was possible to determine what <see cref="PosType"/> <paramref name="p"/> is.</returns>
    public static bool GetPosType(this Pos? p, IList<Design> knownDesigns, out PosType type, out float value, out Design? relativeTo, out Side side, out int offset)
    {
        type = default;
        relativeTo = null;
        side = default;
        value = 0;
        offset = 0;

        if (p.IsAbsolute(out var n))
        {
            type = PosType.Absolute;
            value = n;
            return true;
        }

        if (p.IsRelative(knownDesigns, out relativeTo, out side))
        {
            type = PosType.Relative;
            return true;
        }

        if (p.IsPercent(out var percent))
        {
            type = PosType.Percent;
            value = percent;
            offset = 0;
            return true;
        }

        if (p.IsCenter())
        {
            type = PosType.Center;
            value = 0;
            offset = 0;
            return true;
        }

        if (p.IsAnchorEnd(out var margin))
        {
            type = PosType.AnchorEnd;
            value = margin;
            return true;
        }

        if (p.IsCombine(out var left, out var right, out var add))
        {
            // we only deal in combines if the right is an absolute
            // e.g. Pos.Percent(25) + 5 is supported but Pos.Percent(5) + Pos.Percent(10) is not
            if (right.IsAbsolute(out int rhsVal))
            {
                offset = add ? rhsVal : -rhsVal;
                GetPosType(left, knownDesigns, out type, out value, out relativeTo, out side, out _);
                return true;
            }
        }

        // we have no idea what this Pos type is
        return false;
    }

    /// <summary>
    /// Returns code to generate <paramref name="p"/> as a snippet for writing out to .Designer.cs files.
    /// </summary>
    /// <param name="p"><see cref="Pos"/> to classify.</param>
    /// <param name="knownDesigns">All <see cref="Design"/> that we might be expressed as relative to (e.g. see <see cref="Design.GetAllDesigns"/>).</param>
    /// <returns>Code to generate <paramref name="p"/>.</returns>
    public static string? ToCode(this Pos? p, List<Design> knownDesigns)
    {
        if (!p.GetPosType(knownDesigns, out var type, out var val, out var relativeTo, out var side, out var offset))
        {
            // could not determine the type
            return null;
        }

        return type switch
        {
            PosType.Absolute => val.ToString( "N0" ),
            PosType.Relative when relativeTo is null => throw new InvalidOperationException( "Pos was Relative but 'relativeTo' was null.  What is the Pos relative to?!" ),
            PosType.Relative when offset > 0 => $"Pos.{GetMethodNameFor( side )}({relativeTo.FieldName}) + {offset}",
            PosType.Relative when offset < 0 => $"Pos.{GetMethodNameFor( side )}({relativeTo.FieldName}) - {Math.Abs( offset )}",
            PosType.Relative => $"Pos.{GetMethodNameFor( side )}({relativeTo.FieldName})",
            PosType.Percent when offset > 0 => $"Pos.Percent({val:G5}f) + {offset}",
            PosType.Percent when offset < 0 => $"Pos.Percent({val:G5}f) - {Math.Abs( offset )}",
            PosType.Percent => $"Pos.Percent({val:G5}f)",
            PosType.Center when offset > 0 => $"Pos.Center() + {offset}",
            PosType.Center when offset < 0 => $"Pos.Center() - {Math.Abs( offset )}",
            PosType.Center => $"Pos.Center()",
            PosType.AnchorEnd when offset > 0 => $"Pos.AnchorEnd({(int)val}) + {offset}",
            PosType.AnchorEnd when offset < 0 => $"Pos.AnchorEnd({(int)val}) - {Math.Abs( offset )}",
            PosType.AnchorEnd => $"Pos.AnchorEnd({(int)val})",
            _ => throw new ArgumentOutOfRangeException( nameof( type ) )
        };
    }

    /// <summary>
    /// Creates a <see cref="PosType.Relative"/> by calling appropriate method
    /// e.g. <see cref="Pos.Top(View)"/>.
    /// </summary>
    /// <param name="relativeTo">Wrapper for the <see cref="View"/> that result should be positioned relative to.</param>
    /// <param name="side"><see cref="Enum"/> indicating which relative method should be used
    /// (e.g. <see cref="Pos.Left(View)"/>, <see cref="Pos.Bottom(View)"/>).</param>
    /// <param name="offset">The offset if any to use e.g. if you want:
    /// <code>Pos.Left(myView) + 5</code></param>
    /// <returns>The resulting <see cref="Pos"/> of the invoked method (e.g. <see cref="Pos.Right(View)"/>.</returns>
    // BUG: This returns absolute positions when offsets are non-zero
    // It's a Terminal.Gui issue, but we can probably work around it.
    public static Pos CreatePosRelative(this Design relativeTo, Side side, int offset = 0)
    {
        return side switch
        {
            Side.Top => Pos.Top( relativeTo.View ) + offset,
            Side.Bottom => Pos.Bottom( relativeTo.View ) + offset,
            Side.Left => Pos.Left( relativeTo.View ) + offset,
            Side.Right => Pos.Right( relativeTo.View ) + offset,
            _ => throw new ArgumentOutOfRangeException( nameof( side ) )
        };
    }

    private static bool IsRelative(this Pos? p, out Pos posView)
    {
        // Terminal.Gui will often use Pos.Combine with RHS of 0 instead of just PosView alone
        if (p != null && p.IsCombine(out var left, out var right, out _))
        {
            if (right.IsAbsolute(out int n) && n == 0)
            {
                return left.IsRelative(out posView);
            }
        }

        if (p != null && p.GetType().Name == "PosView")
        {
            posView = p;
            return true;
        }

        posView = 0;
        return false;
    }

    private static string GetMethodNameFor(Side side)
    {
        return side.ToString();
    }
}
