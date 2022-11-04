using System.CodeDom;
using System.Reflection;
using NStack;
using Terminal.Gui;
using Terminal.Gui.Graphs;
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
        this.Design = design;
        this.PropertyInfo = property;
        this.DeclaringObject = this.Design.View;
    }

    public Property(Design design, PropertyInfo property, string subProperty, object declaringObject)
        : this(design, property)
    {
        this.SubProperty = subProperty;
        this.DeclaringObject = declaringObject;
    }

    public virtual object? GetValue()
    {
        return this.PropertyInfo.GetValue(this.DeclaringObject);
    }

    public virtual void SetValue(object? value)
    {
        // handle type conversions
        if (this.PropertyInfo.PropertyType == typeof(Rune))
        {
            if (value is char ch)
            {
                value = new Rune(ch);
            }
        }

        if (this.PropertyInfo.PropertyType == typeof(Dim))
        {
            if (value is int i)
            {
                value = Dim.Sized(i);
            }
        }

        if (this.PropertyInfo.PropertyType == typeof(ustring))
        {
            if (value is string s)
            {
                value = ustring.Make(s);

                // TODO: This seems like something AutoSize should do automatically
                // if renaming a button update its size to match
                if (this.Design.View is Button b && this.PropertyInfo.Name.Equals("Text") && b.Width.IsAbsolute())
                {
                    b.Width = s.Length + (b.IsDefault ? 6 : 4);
                }
            }

            // some views don't like null and only work with "" e.g. TextView
            // see https://github.com/gui-cs/TerminalGuiDesigner/issues/91
            if (value == null)
            {
                value = ustring.Make(string.Empty);
            }
        }

        if (this.PropertyInfo.PropertyType == typeof(IListDataSource))
        {
            if (value != null && value is Array a)
            {
                // accept arrays as valid input values
                // for setting an IListDataSource.  Just
                // convert them to ListWrappers
                value = new ListWrapper(a.ToList());
            }
        }

        // TODO: This hack gets around an ArgumentException that gets thrown when
        // switching from Computed to Absolute values of Dim/Pos
        this.Design.View.IsInitialized = false;

        // if a LineView and changing Orientation then also flip
        // the Height/Width and set appropriate new rune
        if (this.PropertyInfo.Name == nameof(LineView.Orientation)
            && this.Design.View is LineView v && value is Orientation newOrientation)
        {
            switch (newOrientation)
            {
                case Orientation.Horizontal:
                    v.Width = v.Height;
                    v.Height = 1;
                    v.LineRune = Application.Driver.HLine;

                    break;
                case Orientation.Vertical:
                    v.Height = v.Width;
                    v.Width = 1;
                    v.LineRune = Application.Driver.VLine;
                    break;
                default:
                    throw new ArgumentException($"Unknown Orientation {newOrientation}");
            }
        }

        this.PropertyInfo.SetValue(this.DeclaringObject, value);

        this.CallRefreshMethodsIfAny();

        this.Design.View.IsInitialized = true;
    }

    /// <summary>
    /// Calls any methods that update the state of the View
    /// and refresh it against its style e.g. <see cref="TableView.Update"/>
    /// </summary>
    private void CallRefreshMethodsIfAny()
    {
        if (this.Design.View is TabView tv)
        {
            tv.ApplyStyleChanges();
        }

        if (this.Design.View is TableView t)
        {
            t.Update();
        }

        this.Design.View.SetNeedsDisplay();
    }

    public virtual void ToCode(CodeDomArgs args)
    {
        try
        {
            this.AddPropertyAssignment(args, this.GetLhs(), this.GetRhs());
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate ToCode for Property '{this.PropertyInfo.Name}' of Design '{this.Design.FieldName}'", ex);
        }
    }

    public virtual CodeExpression GetRhs()
    {
        var val = this.GetValue();
        if (val == null)
        {
            return new CodeSnippetExpression("null");
        }

        if (val is Attribute attribute)
        {
            return new CodeSnippetExpression(attribute.ToCode());
        }

        if (val is Dim d)
        {
            return new CodeSnippetExpression(d.ToCode());
        }

        if (val is Size s)
        {
            return new CodeSnippetExpression(s.ToCode());
        }

        if (val is PointF pointf)
        {
            return new CodeObjectCreateExpression(
                typeof(PointF),
                new CodePrimitiveExpression(pointf.X),
                new CodePrimitiveExpression(pointf.Y));
        }

        if (val is TextRegexProvider regv)
        {
            return new CodeObjectCreateExpression(
                typeof(TextRegexProvider),
                new CodePrimitiveExpression(regv.Pattern.ToPrimitive()));
        }

        if (val is ListWrapper w)
        {
            // Create an Expression like:
            // new ListWrapper(new string[]{"bob","frank"})

            var a = new CodeArrayCreateExpression();
            Type? listType = null;

            foreach (var v in w.ToList())
            {
                if (v != null && listType == null)
                {
                    listType = v.GetType();
                }

                CodeExpression element = v == null ? new CodeDefaultValueExpression() : new CodePrimitiveExpression(v);

                a.Initializers.Add(element);
            }

            a.CreateType = new CodeTypeReference(listType ?? typeof(string));
            var ctor = new CodeObjectCreateExpression(typeof(ListWrapper), a);

            return ctor;
        }

        if (val is Pos p)
        {
            // TODO: Get EVERYONE! not just siblings
            return new CodeSnippetExpression(p.ToCode(this.Design.GetSiblings().ToList()));
        }

        if (val is Enum e)
        {
            return new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(e.GetType()),
                    e.ToString());
        }

        var type = val.GetType();

        if (type.IsArray)
        {
            var elementType = type.GetElementType();

            var values = ((Array)val).ToList();
            return new CodeArrayCreateExpression(
                elementType,
                values.Select(v => new CodePrimitiveExpression(v.ToPrimitive())).ToArray());
        }

        return new CodePrimitiveExpression(val.ToPrimitive());
    }

    public virtual string GetLhs()
    {
        if (this.Design.IsRoot)
        {
            return string.IsNullOrWhiteSpace(this.SubProperty) ?
                $"this.{this.PropertyInfo.Name}" :
                $"this.{this.SubProperty}.{this.PropertyInfo.Name}";
        }

        // if the property being designed exists on the View directly e.g. MyView.X
        return string.IsNullOrWhiteSpace(this.SubProperty) ?
            $"this.{this.Design.FieldName}.{this.PropertyInfo.Name}" :
            $"this.{this.Design.FieldName}.{this.SubProperty}.{this.PropertyInfo.Name}";
    }

    /// <summary>
    /// Returns the Name of the property including any <see cref="SubProperty"/>
    /// and special formatting e.g. for <see cref="NameProperty"/>
    /// </summary>
    /// <returns></returns>
    public virtual string GetHumanReadableName()
    {
        return this.SubProperty != null ? $"{this.SubProperty}.{this.PropertyInfo.Name}" : this.PropertyInfo.Name;
    }

    public override string ToString()
    {
        return this.GetHumanReadableName() + ":" + this.GetHumanReadableValue();
    }

    protected virtual string GetHumanReadableValue()
    {
        var val = this.GetValue();

        if (val == null)
        {
            return "null";
        }

        if (val is bool b)
        {
            return b ? "Yes" : "No";
        }

        if (val is Attribute attribute)
        {
            return attribute.ToCode();
        }

        if (val is Dim d)
        {
            return d.ToCode() ?? d.ToString() ?? string.Empty;
        }

        if (val is Pos p)
        {
            // TODO: Get EVERYONE not just siblings
            return p.ToCode(this.Design.GetSiblings().ToList()) ?? p.ToString() ?? string.Empty;
        }

        if (val is Array a)
        {
            return string.Join(",", a.ToList());
        }

        return val.ToString() ?? string.Empty;
    }
}
