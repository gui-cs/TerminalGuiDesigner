using CommandLine;
using Terminal.Gui;
using TerminalGuiDesigner.UI;

namespace TerminalGuiDesigner;

/// <summary>
/// Application entry point.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Application entry method.
    /// </summary>
    /// <param name="args">Arguments user supplied on command line (if any).</param>
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       Application.UseSystemConsole = o.Usc;
                       Editor.Experimental = o.Experimental;

                       Application.Init();
                       var editor = new Editor();
                       editor.Run(o);
                   });
    }
}
