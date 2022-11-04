namespace TerminalGuiDesigner;

public static class DictionaryExtensions
{
    public static void AddOrUpdate<TK, TV>(this Dictionary<TK, TV> dict, TK key, TV value) where TK : class
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            dict.Add(key, value);
        }
    }
}