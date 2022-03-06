using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TerminalGuiDesigner;

internal class RoslynCodeToView
{
    public string Namespace { get;}
    public string ClassName { get;}
    public SourceCodeFile SourceFile { get; }

    public RoslynCodeToView(SourceCodeFile sourceFile)
    {
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
        SourceFile = sourceFile;
    }
}
