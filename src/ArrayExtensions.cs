namespace TerminalGuiDesigner;

/// <summary>
/// Extensions methods for the <see cref="Array"/> class.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Converts an <see cref="Array"/> to list of objects.
    /// </summary>
    /// <param name="a">An array to convert.</param>
    /// <returns>The array as a list.</returns>
    public static List<object?> ToList(this Array a)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        var toReturn = new List<object?>();

        for (int i = 0; i < a.Length; i++)
        {
            toReturn.Add(a.GetValue(i));
        }

        return toReturn;
    }
}
