using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// <see cref="Property"/> for storing, editing and code gen for TreeView&lt;T&gt;.Options.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TreeObjectsProperty<T> : Property, ITreeObjectsProperty where T : class
{
    public List<T> Value { get; private set; }
    readonly TreeView<T> treeView;

    Dictionary<Type,Func<CodeExpression>> treeBuilders = new Dictionary<Type, Func<CodeExpression>>()
    {
        { typeof(FileSystemInfo),TreeBuilderForFileSystemInfo} 
    };


    public TreeObjectsProperty(Design design)
        : base(
        design,
        typeof(TreeView<T>).GetProperty(nameof(TreeView<T>.Objects)) 
              ?? throw new MissingFieldException("Expected property was missing from TreeView"))
    {
        treeView = (TreeView<T>)design.View;
        Value = new List<T>(treeView.Objects);
    }

    public override string GetHumanReadableName()
    {
        return "Objects";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{this.GetHumanReadableName()}:{this.Value.Count} objects";
    }

    public override void SetValue(object? value)
    {
        this.Value = (List<T>)(value ?? new List<T>());

        treeView.ClearObjects();
        treeView.AddObjects(this.Value);

    }

    public override object GetValue()
    {
        return this.Value;
    }

    public override void ToCode(CodeDomArgs args)
    {
        // Create statement like this
        //tree1.AddObjects(new List<FileSystemInfo>() { new DirectoryInfo("c:\\") });

        var call = new CodeMethodInvokeExpression();
        call.Method.TargetObject = new CodeFieldReferenceExpression(
            new CodeThisReferenceExpression(),
            this.Design.FieldName);

        call.Method.MethodName = nameof(TreeView.AddObjects);

        var newListStatement = 
             new CodeArrayCreateExpression(
            new CodeTypeReference(typeof(T[])),
            Value.Select(v => TTypes.ToCode(args, Design, v)).ToArray());

        call.Parameters.Add(newListStatement);

        args.InitMethod.Statements.Add(call);

        // Now also create TreeBuilder if its a known Type we can handle
        if(treeBuilders.ContainsKey(typeof(T)))
        {

            var setBuilderLhs = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), $"{this.Design.FieldName}.{nameof(TreeView<T>.TreeBuilder)}");
            var setBuilderRhs = treeBuilders[typeof(T)]();

            var assignStatement = new CodeAssignStatement
            {
                Left = setBuilderLhs,
                Right = setBuilderRhs
            };
            args.InitMethod.Statements.Add(assignStatement);
        }
    }

    private static CodeExpression TreeBuilderForFileSystemInfo()
    {
        return new CodeSnippetExpression("""
                                            new Terminal.Gui.DelegateTreeBuilder<System.IO.FileSystemInfo>(
                                                (p) => p is System.IO.DirectoryInfo d ? d.GetFileSystemInfos() : System.Linq.Enumerable.Empty<System.IO.FileSystemInfo>())
                                            """);
                    }

    public override string GetLhs()
    {
        throw new NotSupportedException("This property requires ");
    }

    public bool IsEmpty()
    {
        return !Value.Any();
    }
}
