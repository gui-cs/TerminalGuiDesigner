using NStack;
using System.CodeDom;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;

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
        var val = GetValue();
        if(val == null)
        {
            return new CodeSnippetExpression("null");
        }

        if(val is Dim d)
        {
            return new CodeSnippetExpression(d.ToCode());
        }

        if(val is Pos p)
        {
            // TODO: Get EVERYONE! not just siblings
            return new CodeSnippetExpression(p.ToCode(Design.GetSiblings().ToList()));
        }


        if(val is Enum e)
        {
            return new CodeSnippetExpression($"{e.GetType().Name}.{e.ToString()}");
        }

        var type = val.GetType();

        if(type.IsArray)
        {
            var elementType = type.GetElementType();

            
            var values = ((Array)val).ToList();
            return new CodeArrayCreateExpression(elementType,
                        values.Select(v=>new CodePrimitiveExpression(v.ToPrimitive())).ToArray()
                   );
        }

        return new CodePrimitiveExpression(val.ToPrimitive());
    }

    public virtual string GetLhs()
    {
        if(Design.IsRoot)
            return $"this.{PropertyInfo.Name}";

        // if the property being designed exists on the View directly e.g. MyView.X
        return string.IsNullOrWhiteSpace(SubProperty) ?
            $"this.{Design.FieldName}.{PropertyInfo.Name}" :
            $"this.{Design.FieldName}.{SubProperty}.{PropertyInfo.Name}";
    }

    public override string ToString()
    {
        return PropertyInfo.Name + ":" + GetHumanReadableValue();
    }

    private string GetHumanReadableValue()
    {
        var val = GetValue();

        if(val == null)
        {
            return "null";
        }
        if(val is bool b)
        {
            return b ? "Yes" : "No";
        }
        if(val is Dim d)
        {
            return d.ToCode() ?? d.ToString() ?? "";
        }
        if(val is Pos p)
        {
            // TODO: Get EVERYONE not just siblings
            return p.ToCode(Design.GetSiblings().ToList()) ?? p.ToString() ?? "";
        }

        if(val is Array a)
        {
            return String.Join(",",a.ToList());
        }

        return val.ToString() ?? "";
    }
}
