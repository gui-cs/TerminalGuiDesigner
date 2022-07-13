namespace TerminalGuiDesigner;
public static class ArrayExtensions
{
    /// <summary>
    /// Converts an <see cref="Array"/> to list of objects.
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static List<object?> ToList(this Array a)
    {
        if(a == null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        var toReturn = new List<object?>();

        for(int i=0;i<a.Length;i++)
        {
            toReturn.Add(a.GetValue(i));
        }

        return toReturn;
    }
}
