using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace TerminalGuiDesigner;

internal class RoslynCodeToView
{
    public string Namespace { get;}
    public string ClassName { get;}
    public SourceCodeFile SourceFile { get; }

    public RoslynCodeToView(SourceCodeFile sourceFile)
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
