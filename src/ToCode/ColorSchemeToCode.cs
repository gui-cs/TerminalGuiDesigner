using System.CodeDom;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.ToCode
{
    public class ColorSchemeToCode : ToCodeBase
    {
        private NamedColorScheme _scheme;

        public ColorSchemeToCode(NamedColorScheme scheme)
        {
            _scheme = scheme;
        }

        public void ToCode(CodeDomArgs args)
        {
            AddFieldToClass(args, typeof(ColorScheme), _scheme.Name);

            AddConstructorCall(args, $"this.{_scheme.Name}", typeof(ColorScheme));

            AddColorSchemeField(args, _scheme.Scheme.Normal, nameof(ColorScheme.Normal));
            AddColorSchemeField(args, _scheme.Scheme.HotNormal, nameof(ColorScheme.HotNormal));
            AddColorSchemeField(args, _scheme.Scheme.Focus, nameof(ColorScheme.Focus));
            AddColorSchemeField(args, _scheme.Scheme.HotFocus, nameof(ColorScheme.HotFocus));
            AddColorSchemeField(args, _scheme.Scheme.Disabled, nameof(ColorScheme.Disabled));
        }

        private void AddColorSchemeField(CodeDomArgs args, Attribute color, string colorSchemeSubfield)
        {
            AddPropertyAssignment(args, $"this.{_scheme.Name}.{colorSchemeSubfield}",
                new CodeObjectCreateExpression(
                    new CodeTypeReference(typeof(Attribute)),
                        GetEnumExpression(color.Foreground),
                        GetEnumExpression(color.Background)));
        }

        private CodeExpression GetEnumExpression(Color color)
        {
            return new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(typeof(Color)),
                color.ToString());
        }
    }
}