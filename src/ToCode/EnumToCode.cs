using System.CodeDom;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Code generation methods for writing out enum values
/// using CodeDom.
/// </summary>
public class EnumToCode : ToCodeBase
{
    private readonly Enum value;
    private readonly Type enumType;

    /// <summary>
    /// Creates a new instance of the class, primed to generate code
    /// for the supplied <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    public EnumToCode(Enum value)
    {
        this.value = value;
        this.enumType = value.GetType();
    }

    /// <summary>
    /// <para>Returns code expression similar to <c>MyEnum.SomeValue</c>.</para>
    /// <para>Supports Flags enums e.g. generating things like <c>MyEnum.ValA | MyEnum.ValB</c></para>
    /// </summary>
    /// <returns></returns>
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