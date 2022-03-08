using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.UI;

namespace TerminalGuiDesigner;


public partial class Program
{
    public static void Main(string[] args)
    {
        var editor = new Editor();
        editor.Run();
    }
}
