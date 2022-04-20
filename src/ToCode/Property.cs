using NStack;
using System.CodeDom;
using System.Reflection;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using TerminalGuiDesigner;
using Attribute = Terminal.Gui.Attribute;

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
    public string? SubProperty { get; }
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

    public virtual object? GetValue()
    {
        return PropertyInfo.GetValue(DeclaringObject);
    }

    public virtual void SetValue(object? value)
    {
        // handle type conversions
        if (PropertyInfo.PropertyType == typeof(Rune))
        {
            if (value is char ch)
            {
                value = new Rune(ch);
            }
        }
        if (PropertyInfo.PropertyType == typeof(ustring))
        {
            if (value is string s)
            {
                value = ustring.Make(s);

                // TODO: This seems like something AutoSize should do automatically
                // if renaming a button update its size to match
                if(Design.View is Button b && PropertyInfo.Name.Equals("Text") && b.Width.IsAbsolute())
                {
                    b.Width = s.Length + (b.IsDefault ? 6 : 4);
                }
            }
        }
        if(PropertyInfo.PropertyType == typeof(IListDataSource))
        {
            if(value != null && value is Array a)
            {
                // accept arrays as valid input values 
                // for setting an IListDataSource.  Just
                // convert them to ListWrappers
                value = new ListWrapper(a.ToList());
            }
        }

        // TODO: This hack gets around an ArgumentException that gets thrown when
        // switching from Computed to Absolute values of Dim/Pos
        Design.View.IsInitialized = false;
        PropertyInfo.SetValue(DeclaringObject, value);

        CallRefreshMethodsIfAny();

        Design.View.IsInitialized = true;
    }

    /// <summary>
    /// Calls any methods that update the state of the View
    /// and refresh it against its style e.g. <see cref="TableView.Update"/>
    /// </summary>
    private void CallRefreshMethodsIfAny()
    {
        if(Design.View is TabView tv)
        {
            tv.ApplyStyleChanges();
        }
        if(Design.View is TableView t)
        {
            t.Update();
        }
        
        Design.View.SetNeedsDisplay();
    }

    public virtual void ToCode(CodeDomArgs args)
    {
        try
        {
            AddPropertyAssignment(args, GetLhs(), GetRhs());   
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate ToCode for Property '{PropertyInfo.Name}' of Design '{Design.FieldName}'",ex);
        }
    }

    public virtual CodeExpression GetRhs()
    {
        var val = GetValue();
        if(val == null)
        {
            return new CodeSnippetExpression("null");
        }
        if(val is Attribute attribute)
        {
            return new CodeSnippetExpression(attribute.ToCode());
        }
        if(val is Dim d)
        {
            return new CodeSnippetExpression(d.ToCode());
        }
        if(val is PointF pointf)
        {
            return new CodeObjectCreateExpression(typeof(PointF),
                 new CodePrimitiveExpression(pointf.X),
                 new CodePrimitiveExpression(pointf.Y));
        }
        if(val is TextRegexProvider regv)
        {
            return new CodeObjectCreateExpression(typeof(TextRegexProvider),
                 new CodePrimitiveExpression(regv.Pattern.ToPrimitive()));
        }
        if(val is ListWrapper w)
        {
            // Create an Expression like:
            // new ListWrapper(new string[]{"bob","frank"})

            var a = new CodeArrayCreateExpression();
            Type? listType = null;

            foreach(var v in w.ToList())
            {
                if(v != null && listType == null)
                {
                    listType = v.GetType();
                }
                
                CodeExpression element = v == null ? new CodeDefaultValueExpression() : new CodePrimitiveExpression(v);

                a.Initializers.Add(element);
            }
            a.CreateType = new CodeTypeReference(listType ?? typeof(string)); 
            var ctor = new CodeObjectCreateExpression(typeof(ListWrapper),a);

            return ctor;
        }

        if(val is Pos p)
        {
            // TODO: Get EVERYONE! not just siblings
            return new CodeSnippetExpression(p.ToCode(Design.GetSiblings().ToList()));
        }


        if(val is Enum e)
        {
            return new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(e.GetType()),
                    e.ToString());
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
            return string.IsNullOrWhiteSpace(SubProperty) ? 
                $"this.{PropertyInfo.Name}" :
                $"this.{SubProperty}.{PropertyInfo.Name}";

        // if the property being designed exists on the View directly e.g. MyView.X
        return string.IsNullOrWhiteSpace(SubProperty) ?
            $"this.{Design.FieldName}.{PropertyInfo.Name}" :
            $"this.{Design.FieldName}.{SubProperty}.{PropertyInfo.Name}";
    }

    /// <summary>
    /// Returns the Name of the property including any <see cref="SubProperty"/>
    /// and special formatting e.g. for <see cref="NameProperty"/>
    /// </summary>
    /// <returns></returns>
    public virtual string GetHumanReadableName()
    {
        return SubProperty != null ? $"{SubProperty}.{PropertyInfo.Name}" : PropertyInfo.Name;
    }
    public override string ToString()
    {
        return GetHumanReadableName() + ":" + GetHumanReadableValue();
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

        if(val is Attribute attribute)
        {
            return attribute.ToCode();
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
