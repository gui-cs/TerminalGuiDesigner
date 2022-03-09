using NStack;
using System.CodeDom;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.UI.Windows;

public class Property : ToCodeBase
{
    public Design Design { get; }

    /// <summary>
    /// The code that leads to this property from <see cref="Design.View"/> or null
    /// if <see cref="PropertyInfo"/> is a direct property of the <see cref="Design.View"/>.
    /// 
    /// <para>For example "TableStyle" or "Border"</para>
    /// 
    /// </summary>
    public string SubProperty { get; }
    public object DeclaringObject { get; set; }

    public PropertyInfo PropertyInfo { get; }
    public Property(Design design, PropertyInfo property)
    {
        Design = design;
        this.PropertyInfo = property;
        DeclaringObject = Design.View;
    }
    public Property(Design design, PropertyInfo property,string subProperty, object declaringObject) : this(design,property)
    {
        SubProperty = subProperty;
        DeclaringObject = declaringObject;
    }

    public virtual object GetValue()
    {
        return PropertyInfo.GetValue(DeclaringObject);
    }

    public virtual void SetValue(object value)
    {
        // handle type conversions
        if (PropertyInfo.PropertyType == typeof(ustring))
        {
            if (value is string s)
            {
                value = ustring.Make(s);
            }
        }

        PropertyInfo.SetValue(DeclaringObject,value);
    }

    public virtual void ToCode(CodeDomArgs args)
    {
        AddPropertyAssignment(args, GetLhs(), GetRhs());
    }

    protected virtual CodeExpression GetRhs()
    {
        return new CodePrimitiveExpression(GetValue().ToPrimitive());
    }

    protected virtual string GetLhs()
    {
        // if the property being designed exists on the View directly e.g. MyView.X
        return string.IsNullOrWhiteSpace(SubProperty) ?
            $"this.{Design.FieldName}.{PropertyInfo.Name}" :
            $"this.{Design.FieldName}.{SubProperty}.{PropertyInfo.Name}";
    }

    public override string ToString()
    {
        return PropertyInfo.Name +":" + GetValue();
    }
}

public class NameProperty : Property
{
    public NameProperty(Design design) : base(design,typeof(Design).GetProperty("FieldName"))
    {

    }

    public override string ToString()
    {
        return $"(Name):{Design.FieldName}";
    }
    public override void SetValue(object value)
    {
        if (value == null)
        {
            throw new ArgumentException("Not Possible", "You cannot set a View (Name) property to null");
        }
        else
        {
            Design.FieldName = value.ToString();
        }
    }

    public override object GetValue()
    {
        return Design.FieldName;
    }
    protected override string GetLhs()
    {
        // Set View.Data to the name of the field so that we can 
        // determine later on which View instances come from which
        // Fields in the class

        return $"this.{Design.FieldName}.{nameof(View.Data)}";
    }
}

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
}
