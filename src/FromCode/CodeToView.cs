using System.ComponentModel;
using System.Reflection;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using NLog;
using NStack;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.FromCode;

/// <summary>
/// Converts a <see cref="SourceCodeFile"/> into a running instance of a <see cref="View"/>
/// by compiling it into an in memory <see cref="Assembly"/> (see <see cref="CSharpCompilation"/>).
/// </summary>
/// <remarks>
/// Compiling requires having the correct assembly references for dependencies.  This is handled
/// by <see cref="CompileAssembly"/>.  Most references come from <see cref="ReferenceAssemblies.Net60"/>
/// but also <see cref="Terminal.Gui"/>.
/// </remarks>
public class CodeToView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeToView"/> class.  Opens the provided <paramref name="sourceFile"/>
    /// and extracts <see cref="Namespace"/>, <see cref="ClassName"/> etc.
    /// </summary>
    /// <param name="sourceFile">Files on disk that will be read by this class (e.g. MyView.cs and MyView.Designer.cs).</param>
    /// <exception cref="Exception">Thrown if file cannot be parsed, does not exist or has multiple class files in it.</exception>
    public CodeToView(SourceCodeFile sourceFile)
    {
        this.SourceFile = sourceFile;

        // Parse .cs file using Roslyn SyntaxTree
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile.CsFile.FullName));
        var root = syntaxTree.GetRoot();

        var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToArray();

        if (namespaces.Length != 1)
        {
            throw new Exception($"Expected {sourceFile.CsFile.FullName} to contain only a single namespace declaration but it had {namespaces.Length}");
        }

        this.Namespace = namespaces.Single().Name.ToString();

        // classes
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();

        if (classes.Length != 1)
        {
            throw new Exception($"Expected {sourceFile.CsFile.FullName} to contain only a single class declaration but it had {classes.Length}");
        }

        var designedClass = classes.Single();
        this.ClassName = designedClass.Identifier.ToString();
    }

    /// <summary>
    /// Gets the C# namespace found in <see cref="SourceFile"/>.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Gets the C# class found in <see cref="SourceFile"/>.
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// Gets the source files this instance is working with.  This is a pair of C#
    /// files (e.g. MyView.cs and MyView.Designer.cs).
    /// </summary>
    public SourceCodeFile SourceFile { get; }

    /// <summary>
    /// Compiles the source code in <see cref="SourceFile"/> and
    /// creates an instance of the View in it wrapped in a <see cref="Design"/>.
    /// </summary>
    /// <returns>Root <see cref="Design"/> (wrapper for <see cref="View"/>) that is constructed when compiling
    /// the contents of the users .Designer.cs file.</returns>
    /// <exception cref="Exception">Thrown if <see cref="SourceCodeFile"/> could not be compiled or it contains multiple class etc.</exception>
    public Design CreateInstance()
    {
        var logger = LogManager.GetCurrentClassLogger();
        logger.Info($"About to compile {this.SourceFile.DesignerFile}");

        var assembly = this.CompileAssembly();

        var expectedClassName = this.ClassName;

        var instances = assembly.GetTypes().Where(t => t.Name.Equals(expectedClassName)).ToArray();

        if (instances.Length == 0)
        {
            throw new Exception($"Could not find a Type called {expectedClassName} in compiled assembly");
        }

        if (instances.Length > 1)
        {
            throw new Exception($"Found {instances.Length} Types called {expectedClassName} in compiled assembly");
        }

        View view;

        try
        {
            view = Activator.CreateInstance(instances[0]) as View
                ?? throw new Exception($"Activator.CreateInstance returned null or class in {this.SourceFile.DesignerFile} was not a View");
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not create instance of {instances[0].FullName}", ex);
        }

        var toReturn = new Design(this.SourceFile, Design.RootDesignName, view);
        toReturn.CreateSubControlDesigns();

        // Record the design in Data field so it can be found later by controls
        // looking up their hierarchy to find top level control designs.
        toReturn.View.Data = toReturn;

        return toReturn;
    }

    /// <summary>
    /// <para>Compiles the .Designer.cs file in <see cref="SourceCodeFile"/> into an <see cref="Assembly"/>
    /// so that an instance of the <see cref="View"/> it contains can be presented in the designer.
    /// </para>
    /// <para>
    /// Only the <see cref="SourceCodeFile.DesignerFile"/> file is used for compilation, any code in <see cref="SourceCodeFile.CsFile"/>
    /// (e.g. MyView.cs) is ignored (event handlers, constructor arguments etc).
    /// </para>
    /// </summary>
    /// <returns>An 'in memory' <see cref="Assembly"/>.</returns>
    /// <exception cref="Exception">Thrown if code in <see cref="SourceCodeFile.DesignerFile"/> does not compile.</exception>
    public Assembly CompileAssembly()
    {
        // All the changes we really care about that are on disk in the users csproj file
        var designerTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(File.ReadAllText(this.SourceFile.DesignerFile.FullName));

        // the user could have put all kinds of stuff into their MyWindow.cs including references to other Types and
        // other things so lets just get what it would be if we had outputted it fresh out of the oven.
        var csTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(ViewToCode.GetGenerateNewViewCode(this.ClassName, this.Namespace));

        var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
        var coreDir = Directory.GetParent(dd) ?? throw new Exception($"Could not find parent directory of dotnet sdk.  Sdk directory was {dd}");

        var references = new List<MetadataReference>(Net70.References.All);

        references.Add(MetadataReference.CreateFromFile(typeof(View).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(ustring).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(System.Data.DataTable).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(MarshalByValueComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"));
        references.Add(MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll"));

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create(
                Guid.NewGuid().ToString() + ".dll",
                new CSharpSyntaxTree[] { csTree, designerTree },
                references,
                options);

        using var stream = new MemoryStream();
        EmitResult result = compilation.Emit(stream);
        if (result.Success)
        {
            var assembly = Assembly.Load(stream.GetBuffer());
            return assembly;
        }

        throw new Exception($"Could not compile {this.SourceFile.DesignerFile}:" + Environment.NewLine + string.Join(Environment.NewLine, result.Diagnostics));
    }
}
