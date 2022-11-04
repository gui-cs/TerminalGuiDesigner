using System.CodeDom;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.ToCode
{
    public class ColorSchemeToCode : ToCodeBase
    {
        private NamedColorScheme scheme;

        public ColorSchemeToCode(NamedColorScheme scheme)
        {
            this.scheme = scheme;
        }

        public void ToCode(CodeDomArgs args)
        {
            this.AddFieldToClass(args, typeof(ColorScheme), this.scheme.Name);

            this.AddConstructorCall(args, $"this.{this.scheme.Name}", typeof(ColorScheme));

            this.AddColorSchemeField(args, this.scheme.Scheme.Normal, nameof(ColorScheme.Normal));
            this.AddColorSchemeField(args, this.scheme.Scheme.HotNormal, nameof(ColorScheme.HotNormal));
            this.AddColorSchemeField(args, this.scheme.Scheme.Focus, nameof(ColorScheme.Focus));
            this.AddColorSchemeField(args, this.scheme.Scheme.HotFocus, nameof(ColorScheme.HotFocus));
            this.AddColorSchemeField(args, this.scheme.Scheme.Disabled, nameof(ColorScheme.Disabled));
        }

        private void AddColorSchemeField(CodeDomArgs args, Attribute color, string colorSchemeSubfield)
        {
            this.AddPropertyAssignment(
                args,
                $"this.{this.scheme.Name}.{colorSchemeSubfield}",
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
}