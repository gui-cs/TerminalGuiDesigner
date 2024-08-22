using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Handles generating all <see cref="StatusItem"/> that are stored a the <see cref="StatusBar"/>.
/// </summary>
public class StatusBarItemsToCode : ToCodeBase
{
    private readonly Design design;
    private readonly StatusBar statusBar;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusBarItemsToCode"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="StatusBar"/>.</param>
    public StatusBarItemsToCode(Design design)
    {
        this.design = design;

        if (design.View is not StatusBar sb)
        {
            throw new ArgumentException(nameof(design), $"{nameof(StatusBarItemsToCode)} can only be used with {nameof(TerminalGuiDesigner.Design)} that wrap {nameof(StatusBar)}");
        }

        this.statusBar = sb;
    }

    /// <summary>
    /// Adds code to .Designer.cs to construct and initialize all <see cref="StatusItem"/>
    /// in the <see cref="StatusBar"/>.
    /// </summary>
    /// <param name="args">State object for the .Designer.cs file being generated.</param>
    public void ToCode(CodeDomArgs args)
    {
        // TODO: Let user name these
        List<string> items = new();

        var statusBarFieldExpression = new CodeFieldReferenceExpression(
            new CodeThisReferenceExpression(), design.FieldName);

        foreach (var child in this.statusBar.GetShortcuts())
        {
            var name = args.GetUniqueFieldName(child.Title.ToString(), false);
            items.Add(name);

            this.AddFieldToClass(args, typeof(Shortcut), name);

            var param1 = new CodeCastExpression(
                        new CodeTypeReference(typeof(KeyCode)),
                        new CodePrimitiveExpression((uint)child.Key.KeyCode));
            var param2 = child.Title.ToCodePrimitiveExpression();
            var param3 = new CodePrimitiveExpression(/*null*/);

            this.AddConstructorCall(
                args,
                $"this.{name}",
                typeof(Shortcut),
                param1,
                param2,
                param3);

            var shortcutFieldExpression = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), name);


            // Create this.myBar.Add(sb1);
            AddMethodCall(args, statusBarFieldExpression,
                nameof(StatusBar.Add),new CodeExpression[]
                {
                    shortcutFieldExpression
                });
        }
    }
}
