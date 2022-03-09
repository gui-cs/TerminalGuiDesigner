using System.CodeDom;
using System.Reflection;

namespace TerminalGuiDesigner.ToCode;

public class SnippetProperty : Property
{
    public string Code { get; }
    public object Value { get; private set; }
    public Func<string>[] CodeParameters { get; }

    public SnippetProperty(Property toWrap, string code, object value, params Func<string>[] codeParameters)
        : base(toWrap.Design, toWrap.PropertyInfo, toWrap.SubProperty, toWrap.DeclaringObject)
    {
        Code = code;
        Value = value;

        // Using delegates means that rename operations will
        // not break us
        CodeParameters = codeParameters;
    }

    public string GetCodeWithParameters()
    {
        return string.Format(Code, CodeParameters.Select(f => f()).ToArray());
    }
    public override string ToString()
    {
        return $"{PropertyInfo.Name}:{GetCodeWithParameters()}";
    }

    protected override CodeExpression GetRhs()
    {
        return new CodeSnippetExpression(GetCodeWithParameters());
    }

    public override void SetValue(object value)
    {
        // if we are changing a value to a complex designed value type (e.g. Pos or Dim)
        if (value is SnippetProperty snipNew)
        {
            // changing to a snip
            Design.SetSnippetProperty(snipNew, value);
            return;
        }

        base.SetValue(value);
    }
}
