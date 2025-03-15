using CommandLine;
using CommandLine.Text;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Command line options that can be supplied to TerminalGuiDesigner e.g. to immediately open
/// a file on startup instead of just showing splash screen.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets examples for command line that show how to invoke the application.  These
    /// appear when user uses:
    /// <code>--help</code>
    /// </summary>
    [Usage(ApplicationAlias = "TerminalGuiDesigner")]
    public static IEnumerable<Example> Examples
    {
        get
        {
            return new List<Example>()
            {
                new Example("Run the application", new Options { }),
                new Example("Open existing MyExistingView.cs", new Options { Path = "../MyProject/MyExistingView.cs" }),
                new Example("Create a new Dialog called MyNewView.cs", new Options { Path = "../MyProject/MyNewView.cs", ViewType = "Dialog", Namespace = "MyApp" }),
            };
        }
    }

#nullable disable warnings

    /// <summary>
    /// Gets or Sets path to create or open.
    /// </summary>
    [Value(0, MetaName = "path", HelpText = "New to create or existing file to open.")]
    public string Path { get; set; }

    /// <summary>
    /// Gets or Sets name of a Type of <see cref="View"/> to create after opening.
    /// </summary>
    [Option('v', HelpText = "The type of root View to create (e.g. Window, Dialog)")]
    public string ViewType { get; set; }

    /// <summary>
    /// Gets or sets the namespace of new view to be created.
    /// </summary>
    [Option('n', HelpText = "The C# namespace to be used for the View code generated")]
    public string Namespace { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether experimental new features should be accessible.
    /// </summary>
    [Option('e', HelpText = "Enables experimental features")]
    public bool Experimental { get; set; }


    /// <summary>
    /// Gets or sets a which driver to use.
    /// </summary>
    [Option('d', HelpText = "Driver to use. v2, v2net, v2win, WindowsDriver, CursesDriver or NetDriver",
        Default = "v2")]
    public string Driver { get; set; } = "v2";


    /// <summary>
    /// Gets or sets a value indicating whether to enable logging.
    /// </summary>
    [Option('q', HelpText = "Pass to suppress log generation")]
    public bool Quiet { get; set; }
#nullable enable warnings
}
