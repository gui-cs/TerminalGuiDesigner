using System.CodeDom;
using System.Text.RegularExpressions;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns <paramref name="name"/> such that it is unique and does not collide with any entries
    /// in <paramref name="inScope"/> collection.
    /// </summary>
    /// <param name="name">A string to make unique.</param>
    /// <param name="inScope">Comparison to use or null for default (<see cref="StringComparer.InvariantCulture"/>).</param>
    /// <param name="comparer">Comparer for matching against collection (e.g. case sensitive or not).</param>
    /// <returns>The same string if it is unique or a modified version (e.g. with numerical suffix).</returns>
    public static string MakeUnique(this string? name, IEnumerable<string> inScope, StringComparer? comparer = null)
    {
        comparer ??= StringComparer.InvariantCulture;

        name = string.IsNullOrWhiteSpace(name) ? "blank" : name;

        // in case it is single iteration
        var set = new HashSet<string>(inScope, comparer);

        if (!set.Contains(name, comparer))
        {
            return name;
        }

        // name is already used, add a number
        int number = 2;

        // but wait! what if it already ends with a number?
        var endsWithNumber = Regex.Match(name, "([0-8]+)$");

        if (endsWithNumber.Success)
        {
            // start incrementing from that number instead
            // i.e. Fish2 becomes Fish3
            number = int.Parse(endsWithNumber.Groups[1].Value);
            name = name.Substring(0, endsWithNumber.Index);
        }

        while (set.Contains(name + number, comparer))
        {
            // menu2 is taken, try menu3 etc
            number++;
        }

        // found a unique one
        return name + number;
    }

    /// <summary>
    /// Returns a <see cref="CodeSnippetExpression"/> or the null expression if <paramref name="s"/>
    /// is null.
    /// </summary>
    /// <param name="s">String to convert into a snippet.</param>
    /// <returns><see cref="CodeSnippetExpression"/> or <see cref="CodePrimitiveExpression"/> representing null.</returns>
    public static CodeExpression ToCodeSnippetExpression(this string? s)
    {
        return s == null ? new CodePrimitiveExpression() : new CodeSnippetExpression(s);
    }

    /// <summary>
    /// <para>
    /// Pads a string until it reaches <paramref name="length"/> by adding
    /// spaces to either side (centering it).
    /// </para>
    /// <par>
    /// If <paramref name="source"/> cannot be exactly centered (e.g. 1 width input
    /// pushed out to 4 desired width) then the extra space will be on the right.
    /// </par>
    /// </summary>
    /// <param name="source">string to pad.</param>
    /// <param name="length">desired length of new string.</param>
    /// <returns>A string of <paramref name="length"/> length and centered text.</returns>
    public static string PadBoth(this string source, int length)
    {
        if (source.Length > length)
        {
            return source;
        }

        int spaces = length - source.Length;
        int padLeft = (spaces / 2) + source.Length;
        source = source.PadLeft(padLeft).PadRight(length);

        return source;
    }
}
