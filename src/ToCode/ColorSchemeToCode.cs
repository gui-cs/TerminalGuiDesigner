using System.CodeDom;
using Terminal.Gui;
using Terminal.Gui.Drawing;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Handles generating code for a <see cref="NamedScheme"/> into .Designer.cs
/// file (See <see cref="CodeDomArgs"/>).  This will be a private field within the
/// class generated e.g.:
/// <code>private Terminal.Gui.Drawing.Scheme dialogBackground;</code>
/// </summary>
public class SchemeToCode : ToCodeBase
{
    private NamedScheme scheme;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemeToCode"/> class.
    /// </summary>
    /// <param name="scheme">The <see cref="Scheme"/> tracked by
    /// <see cref="SchemeManager"/> that is to be added to .Designer.cs.</param>
    public SchemeToCode(NamedScheme scheme)
    {
        this.scheme = scheme;
    }

    /// <summary>
    /// Generates CodeDOM statements to declare private <see cref="Scheme"/> field,
    /// constructor call and property initializations for the <see cref="NamedScheme"/>.
    /// </summary>
    /// <param name="args">State object for the .Designer.cs file being generated.</param>
    public void ToCode(CodeDomArgs args)
    {
        this.AddFieldToClass(args, typeof(Scheme), this.scheme.Name);

        this.AddConstructorCall(args, $"this.{this.scheme.Name}", typeof(Scheme),
            GetColorCode(this.scheme.Scheme.Normal),
            GetColorCode(this.scheme.Scheme.Focus),
            GetColorCode(this.scheme.Scheme.HotNormal),
            GetColorCode(this.scheme.Scheme.Disabled),
            GetColorCode(this.scheme.Scheme.HotFocus));
    }

    private CodeObjectCreateExpression GetColorCode(Attribute attr)
    {
        return new CodeObjectCreateExpression(
            typeof(Attribute),
            new CodePrimitiveExpression(attr.Foreground.Argb),
            new CodePrimitiveExpression(attr.Background.Argb));
    }

    private void AddSchemeField(CodeDomArgs args, Attribute color, string SchemeSubfield)
    {
        this.AddPropertyAssignment(
            args,
            $"this.{this.scheme.Name}.{SchemeSubfield}",
            new CodeObjectCreateExpression(
                new CodeTypeReference(typeof(Attribute)),
                this.GetEnumExpression(color.Foreground),
                this.GetEnumExpression(color.Background)));
    }

    private CodeExpression GetEnumExpression(Color color)
    {
        return new CodeFieldReferenceExpression(
            new CodeTypeReferenceExpression(typeof(Color)),
            color.ToString());
    }
}