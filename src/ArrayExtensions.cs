namespace TerminalGuiDesigner;



public static class ArrayExtensions
{
    public static List<object> ToList(this Array a)
    {
        if(a == null)
        {
            return null;
        }

        var toReturn = new List<object>();

        for(int i=0;i<a.Length;i++)
        {
            toReturn.Add(a.GetValue(i));
        }

        return toReturn;
    }
}
