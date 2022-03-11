using System.CodeDom;
using System.Reflection;

namespace TerminalGuiDesigner.ToCode;

public class SnippetProperty : Property
{
    public string? Code { get; private set;}
    public object Value { get; private set; }
    public Func<string>[] CodeParameters { get; private set;}

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
        if(Code != null)
        {
            return $"{PropertyInfo.Name}:{GetCodeWithParameters()}";
        }

        return base.ToString();
    }

    protected override CodeExpression GetRhs()
    {
        if(Code != null)
        {
            return new CodeSnippetExpression(GetCodeWithParameters());
        }

        return base.GetRhs();
    }

    public override void SetValue(object value)
    {
        if(value is SnippetProperty newSnip)
        {
            Code = newSnip.Code;
            Value = newSnip.Value;
            CodeParameters = newSnip.CodeParameters;
            
            base.SetValue(Value);

        }
        else
        {
            Code = null;
            Value = value;
            base.SetValue(value);
        }
    }
}
