using System.CodeDom;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.ToCode
{
    public class ColorSchemeToCode : ToCodeBase
    {
        private string fieldName;
        private ColorScheme colorScheme;

        public ColorSchemeToCode(string fieldName, ColorScheme colorScheme)
        {
            this.fieldName = fieldName;
            this.colorScheme = colorScheme;
        }

        public void ToCode(CodeDomArgs args)
        {
            AddFieldToClass(args,typeof(ColorScheme),fieldName);

            AddConstructorCall(args,$"this.{fieldName}",typeof(ColorScheme));

            AddColorSchemeField(args,colorScheme.Normal,nameof(ColorScheme.Normal));
            AddColorSchemeField(args,colorScheme.HotNormal,nameof(ColorScheme.HotNormal));
            AddColorSchemeField(args,colorScheme.Focus,nameof(ColorScheme.Focus));
            AddColorSchemeField(args,colorScheme.HotFocus,nameof(ColorScheme.HotFocus));
            AddColorSchemeField(args,colorScheme.Disabled,nameof(ColorScheme.Disabled));
        }

        private void AddColorSchemeField(CodeDomArgs args, Attribute color, string colorSchemeSubfield)
        {
            AddPropertyAssignment(args,$"this.{fieldName}.{colorSchemeSubfield}",
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