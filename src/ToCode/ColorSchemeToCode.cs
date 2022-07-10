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
                        new CodePrimitiveExpression(colorScheme.Normal.Foreground),
                        new CodePrimitiveExpression(colorScheme.Normal.Background)));
        }
    }
}