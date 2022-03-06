using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using NLog;
using NStack;
using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

internal class CodeToView
{
    public string Namespace { get;}
    public string ClassName { get;}
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

        if(classes.Length != 1)
        {
            throw new Exception($"Expected {sourceFile.CsFile.FullName} to contain only a single class declaration but it had {classes.Length}");
        }
        
        var designedClass = classes.Single();
        ClassName = designedClass.Identifier.ToString();
    }

    internal Design CreateInstance()
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

        var toReturn = new Design(SourceFile, "root", view);
        toReturn.CreateSubControlDesigns();

        return toReturn;
    }

    public Assembly CompileAssembly()
    {
        // the user could have put all kinds of stuff into their MyWindow.cs including references to other Types and
        // other things so lets just get what it would be if we had outputted it fresh out of the oven.
        var csTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(ViewToCode.GetGenerateNewWindowCode(ClassName, Namespace));

        // All the changes we really care about that are on disk in the users csproj file
        var designerTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(File.ReadAllText(SourceFile.DesignerFile.FullName));

        var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
        var coreDir = Directory.GetParent(dd);

        var netCoreLib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var terminalGuilib = MetadataReference.CreateFromFile(typeof(View).Assembly.Location);
        var nstackLib = MetadataReference.CreateFromFile(typeof(ustring).Assembly.Location);
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

            throw new Exception($"Could not compile {SourceFile.DesignerFile}:" + Environment.NewLine + string.Join(Environment.NewLine,result.Diagnostics));
        }
    }
    public string GetRhsCodeFor(Design design,string fieldName, PropertyInfo p)
    {
        // read the .Designer.cs file
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(SourceFile.DesignerFile.FullName));

        // look for an assignment for the given property
        var filedName = design.FieldName;

        // get the InitializeComponent method
        var root = syntaxTree.GetRoot();
        var initMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m=>m.Identifier.ToString().Equals(SourceCodeFile.InitializeComponentMethodName)).ToArray();

        if (initMethods.Length != 1)
        {
            throw new Exception($"Expected {SourceFile.DesignerFile.FullName} to contain only a single '{SourceCodeFile.InitializeComponentMethodName}' method but it contained {initMethods.Length}");
        }

        var initMethod = initMethods.Single();

        var lookingFor = $"{fieldName}.{p.Name}";

        // find assignments where the lhs ends with the fieldName
        var assignments = initMethod.DescendantNodes().OfType<AssignmentExpressionSyntax>()
            .Where(a=>a.Left.ToString().EndsWith(lookingFor)).ToArray();

        if (assignments.Length > 1)
        {
            throw new Exception($"Found multiple assignments to the field '{lookingFor}' in the '{SourceCodeFile.InitializeComponentMethodName}' method in {SourceFile.DesignerFile.FullName}");
        }

        // there are no assignments to this property
        if(assignments.Length == 0)
            return null;

        // return the Rhs code in the Designer.cs for this field
        return assignments[0].Right.ToString();
    }
}
