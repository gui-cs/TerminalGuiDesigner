using CommandLine;
using CommandLine.Text;

namespace TerminalGuiDesigner
{
    public class Options
    {
        #nullable disable warnings
        [Value(0, MetaName = "path", HelpText = "New to create or existing file to open.")]
        public string Path { get; set; }

        [Option('v', HelpText = "The type of root View to create (Window or Dialog)")]
        public string ViewType { get; set; }

        [Option('n', HelpText = "The C# namespace to be used for the View code generated")]
        public string Namespace { get; set; }
        
        #nullable enable warnings

        [Usage(ApplicationAlias = "TerminalGuiDesigner")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Run the application", new Options { }),
                    new Example("Open existing MyExistingView.cs", new Options { Path = "../MyProject/MyExistingView.cs"}),
                    new Example("Create a new Dialog called MyNewView.cs", new Options { Path = "../MyProject/MyNewView.cs",ViewType = "Dialog", Namespace = "MyApp"})
                };
            }
        }
    }
}
