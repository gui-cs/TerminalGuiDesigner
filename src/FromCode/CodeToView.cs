using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using NLog;
using NStack;
using System.ComponentModel;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.FromCode;

public class CodeToView
{
    public string Namespace { get; }
    public string ClassName { get; }
    public SourceCodeFile SourceFile { get; }

    ILogger logger = LogManager.GetCurrentClassLogger();

    public CodeToView(SourceCodeFile sourceFile)
    {
        SourceFile = sourceFile;

        // Parse .cs file using Roslyn SyntaxTree
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile.CsFile.FullName));
        var root = syntaxTree.GetRoot();

        var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToArray();

        if (namespaces.Length != 1)
        {
            throw new Exception($"Expected {sourceFile.CsFile.FullName} to contain only a single namespace declaration but it had {namespaces.Length}");
        }

        Namespace = namespaces.Single().Name.ToString();

        // classes
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();

        if (classes.Length != 1)
        {
            throw new Exception($"Expected {sourceFile.CsFile.FullName} to contain only a single class declaration but it had {classes.Length}");
        }

        var designedClass = classes.Single();
        ClassName = designedClass.Identifier.ToString();
    }

    /// <summary>
    /// Compiles the source code in <see cref="SourceFile"/> and 
    /// creates an instance of the View in it wrapped in a <see cref="Design"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Design CreateInstance()
    {
        logger.Info($"About to compile {SourceFile.DesignerFile}");

        var assembly = CompileAssembly();

        var expectedClassName = ClassName;

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
                ?? throw new Exception($"Activator.CreateInstance returned null or class in {SourceFile.DesignerFile} was not a View");
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not create instance of {instances[0].FullName}", ex);
        }

        var toReturn = new Design(SourceFile, Design.RootDesignName, view);
        toReturn.CreateSubControlDesigns();

        return toReturn;
    }

    public Assembly CompileAssembly()
    {
        // All the changes we really care about that are on disk in the users csproj file
        var designerTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(File.ReadAllText(SourceFile.DesignerFile.FullName));

        var viewType = GetViewType(designerTree);
        
        // the user could have put all kinds of stuff into their MyWindow.cs including references to other Types and
        // other things so lets just get what it would be if we had outputted it fresh out of the oven.
        var csTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(ViewToCode.GetGenerateNewViewCode(ClassName, Namespace));

        var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
        var coreDir = Directory.GetParent(dd) ?? throw new Exception($"Could not find parent directory of dotnet sdk.  Sdk directory was {dd}");

        var netCoreLib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var terminalGuilib = MetadataReference.CreateFromFile(typeof(View).Assembly.Location);
        var nstackLib = MetadataReference.CreateFromFile(typeof(ustring).Assembly.Location);
        var marshalLib = MetadataReference.CreateFromFile(typeof(MarshalByValueComponent).Assembly.Location);
        var systemData = MetadataReference.CreateFromFile(typeof(System.Data.DataTable).Assembly.Location);
        var mscorLib = MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll");
        var runtimeLib = MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll");

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation
            = CSharpCompilation.Create(Guid.NewGuid().ToString() + ".dll",
            new CSharpSyntaxTree[] { csTree, designerTree }, references: new[]
            {
                netCoreLib,
                terminalGuilib,
                nstackLib,
                systemData,
                marshalLib,
                mscorLib,
                runtimeLib}, options: options);

        using (var stream = new MemoryStream())
        {
            EmitResult result = compilation.Emit(stream);
            if (result.Success)
            {
                var assembly = Assembly.Load(stream.GetBuffer());
                return assembly;
            }

            throw new Exception($"Could not compile {SourceFile.DesignerFile}:" + Environment.NewLine + string.Join(Environment.NewLine, result.Diagnostics));
        }
    }

    /// <summary>
    /// Returns the Type for the 
    /// </summary>
    private Type GetViewType(CSharpSyntaxTree designerTree)
    {
        
        // get the InitializeComponent method
        var root = designerTree.GetRoot();
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .ToArray();

        if(classDeclarations.Length != 1)
        {
            throw new Exception($"Found {classDeclarations.Length} class declarations");
        }
        
        var baseClass = classDeclarations[0].BaseList?.Types.Single().Type ?? throw new Exception($"Expected .Designer.cs class to have a base class derived from View");
        var baseTypeName = baseClass.ToString();

        return typeof(View).Assembly.GetTypes().Single(t=>
            !t.IsInterface && !t.IsAbstract && typeof(View).IsAssignableFrom(t)
            & ( t.Name.Equals(baseTypeName) || baseTypeName.Equals(t.FullName))) ?? throw new Exception($"Could not find Type '{baseTypeName}'");
    }
}
