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

            // TODO: add other color member
            AddPropertyAssignment(args,$"this.{fieldName}.{nameof(ColorScheme.Normal)}",
                new CodeObjectCreateExpression(
                    new CodeTypeReference(typeof(Attribute)), 
                        GetEnumExpression(colorScheme.Normal.Foreground),
                        GetEnumExpression(colorScheme.Normal.Background)));
        }

        private CodeExpression GetEnumExpression(Color foreground)
        {
            return new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(typeof(Color)),
                foreground.ToString());
        }
    }
}