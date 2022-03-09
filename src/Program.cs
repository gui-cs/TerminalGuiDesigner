using TerminalGuiDesigner.UI;

namespace TerminalGuiDesigner;


public partial class Program
{
    public static void Main(string[] args)
    {
        var editor = new Editor();
        editor.Run(args.Length > 0 ? args[0] : null);
    }
}
