using System.CodeDom;
using System.Reflection;
using System.Runtime.Intrinsics;
using Terminal.Gui;
using TerminalGuiDesigner.FromCode;

namespace TerminalGuiDesigner.ToCode;

public class TreeObjectsProperty<T> : Property where T : class
{
    public List<T> Value { get; private set; } = new List<T>();
    readonly TreeView<T> treeView;

    public TreeObjectsProperty(Design design)
        : base(
        design,
        typeof(TreeView<T>).GetProperty(nameof(TreeView<T>.Objects)) 
              ?? throw new MissingFieldException("Expected property was missing from TreeView"))
    {
        treeView = (TreeView<T>)design.View;
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
    }

    public override string GetLhs()
    {
        throw new NotSupportedException("This property requires ");
    }

}
