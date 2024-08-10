using System.CodeDom;

namespace TerminalGuiDesigner.ToCode;

public class EnumToCode : ToCodeBase
{
    private readonly Enum value;
    private readonly Type enumType;


    public EnumToCode(Enum value)
    {
        this.value = value;
        this.enumType = value.GetType();
    }

    public CodeExpression ToCode()
    {
        var isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);

        if (isFlags)
        {
            return FlagsToExpression();
        }

        return new CodeFieldReferenceExpression(
            new CodeTypeReferenceExpression(enumType),
            value.ToString());
    }

    private CodeExpression FlagsToExpression()
    {
        // Split the string representation of the Flags enum into individual values
        var flagValues = value.ToString().Split(new[] { ", " }, StringSplitOptions.None);

        // Start by creating the first flag expression
        CodeExpression expression = new CodeFieldReferenceExpression(
            new CodeTypeReferenceExpression(enumType),
            flagValues[0]);

        // Iterate through the remaining flags and combine them using bitwise OR
        for (int i = 1; i < flagValues.Length; i++)
        {
            var nextFlag = new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(enumType),
                flagValues[i]);

            expression = new CodeBinaryOperatorExpression(
                expression,
                CodeBinaryOperatorType.BitwiseOr,
                nextFlag);
        }

        return expression;
    }
}