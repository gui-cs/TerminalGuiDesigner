using CommandLine;
using Terminal.Gui;
using TerminalGuiDesigner.UI;

namespace TerminalGuiDesigner;


public partial class Program
{
    public static void Main(string[] args)
    {
        Application.Init();

        Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       var editor = new Editor();
                       editor.Run(o);

                   });
    }
}
