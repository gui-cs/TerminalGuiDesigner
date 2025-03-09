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
                       Editor.Experimental = o.Experimental;
                       Editor.Quiet = o.Quiet;
                       
                       Application.Init(null,o.Driver);
                       var editor = new Editor();
                       editor.Run(o);
                   });
    }
}
