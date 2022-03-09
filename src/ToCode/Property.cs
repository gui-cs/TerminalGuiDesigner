using NStack;
using System.CodeDom;
using System.Reflection;

namespace TerminalGuiDesigner.ToCode;

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
        PropertyInfo = property;
        DeclaringObject = Design.View;
    }
    public Property(Design design, PropertyInfo property, string subProperty, object declaringObject) : this(design, property)
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

        if(value is SnippetProperty snip)
        {
            Design.SetSnippetProperty(snip, value);
            return;
        }

        // TODO: This hack gets around an ArgumentException that gets thrown when
        // switching from Computed to Absolute values of Dim/Pos
        Design.View.IsInitialized = false;
        PropertyInfo.SetValue(DeclaringObject, value);
        Design.View.IsInitialized = true;
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
        return PropertyInfo.Name + ":" + GetValue();
    }
}
