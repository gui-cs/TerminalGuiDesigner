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
}
