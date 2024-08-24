using System.CodeDom;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using TerminalGuiDesigner;
using YamlDotNet.Core.Tokens;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// <para>
/// Describes a designable setting on a <see cref="View"/>.  Usually this is a
/// <see cref="PropertyInfo"/> but can be a meta setting (e.g. <see cref="NameProperty"/>).
/// </para>
/// <para>
/// Can be a <see cref="SubProperty"/> of a <see cref="View"/> e.g.
/// <see cref="TableView.Style"/> sub-properties.
/// </para>
/// </summary>
public class Property : ToCodeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="View"/> you want to change
    /// the <paramref name="property"/> on.</param>
    /// <param name="property">The <see cref="PropertyInfo"/> that can be
    /// changed. Must be a public property of <see cref="Design.View"/>.</param>
    public Property(Design design, PropertyInfo property)
    {
        this.Design = design;
        this.PropertyInfo = property;
        this.DeclaringObject = this.Design.View;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> class.
    /// <para>This overload handles sub-properties (e.g. properties of <see cref="TableView.Style"/>).</para>
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="View"/> you want to change
    /// the <paramref name="property"/> on.</param>
    /// <param name="property">The <see cref="PropertyInfo"/> that can be changed.  Should NOT exist on
    /// <see cref="Design.View"/> but instead a <paramref name="subProperty"/> (e.g. <see cref="TableStyle.AlwaysShowHeaders"/>).</param>
    /// <param name="subProperty">The name of a property on <see cref="Design.View"/> which contains the
    /// <paramref name="subProperty"/> (e.g. <see cref="TableView.Style"/>).</param>
    /// <param name="declaringObject">Instance referenced by <paramref name="subProperty"/> on <see cref="Design.View"/>
    /// (e.g. current reference value of <see cref="TableView.Style"/>).</param>
    public Property(Design design, PropertyInfo property, string subProperty, object declaringObject)
        : this(design, property)
    {
        this.SubProperty = subProperty;
        this.DeclaringObject = declaringObject;
    }

    /// <summary>
    /// Gets the wrapper for the <see cref="View"/> on which this <see cref="Property"/> is changeable.
    /// </summary>
    public Design Design { get; }

    /// <summary>
    /// Gets the name of the property of <see cref="Design.View"/> in which <see cref="PropertyInfo"/>
    /// exists as a a sub-property.
    /// <para>Null if <see cref="PropertyInfo"/> is a direct property of the <see cref="Design.View"/>.</para>
    /// </summary>
    public string? SubProperty { get; }

    /// <summary>
    /// Gets the instance object which <see cref="PropertyInfo"/> should be changed on. This is either
    /// <see cref="Design.View"/> or a sub-property of that (e.g. <see cref="TableView.Style"/>).
    /// </summary>
    public object DeclaringObject { get; }

    /// <summary>
    /// Gets the property which should be changed on <see cref="DeclaringObject"/> when value is changed.
    /// </summary>
    public PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// Gets the current value stored in <see cref="PropertyInfo"/> of <see cref="DeclaringObject"/>.
    /// </summary>
    /// <returns>Current value of the property.</returns>
    public virtual object? GetValue()
    {
        return this.PropertyInfo.GetValue(this.DeclaringObject);
    }

    /// <summary>
    /// Sets a new value for the <see cref="PropertyInfo"/>.  For complex properties like <see cref="View.ColorScheme"/>
    /// this may involve other steps (e.g. interacting with <see cref="ColorSchemeManager"/>).
    /// </summary>
    /// <param name="value">New value to set.</param>
    /// <exception cref="ArgumentException">Thrown if invalid values are passed.</exception>
    public virtual void SetValue(object? value)
    {
        value = AdjustValueBeingSet(value);

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
                    v.LineRune = ConfigurationManager.Glyphs.HLine;

                    break;
                case Orientation.Vertical:
                    v.Height = v.Width;
                    v.Width = 1;
                    v.LineRune = ConfigurationManager.Glyphs.VLine;
                    break;
                default:
                    throw new ArgumentException($"Unknown Orientation {newOrientation}");
            }
        }

        this.PropertyInfo.SetValue(this.DeclaringObject, value);

        this.CallRefreshMethodsIfAny();
    }

    /// <summary>
    /// Adds an assignment block of CodeDOM code to <paramref name="args"/>.  Typically this will
    /// be in InitializeComponent method of .Designer.cs e.g.:
    /// <code>this.label1.Text = "heya";</code>
    /// </summary>
    /// <param name="args">Current state of the .Designer.cs file.</param>
    /// <exception cref="Exception">Thrown if it is not possible to generate code for the current <see cref="Property"/>.</exception>
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

    /// <summary>
    /// Gets a CodeDOM code block for the right hand side of an assignment operation e.g.:
    /// <code>"heya"</code>
    /// </summary>
    /// <returns>Right hand side code of property assignment.</returns>
    public virtual CodeExpression GetRhs()
    {
        var val = this.GetValue();
        if (val == null)
        {
            return new CodeSnippetExpression("null");
        }

        if(val is Rune rune)
        {
            char[] chars = new char[rune.Utf16SequenceLength];
            rune.EncodeToUtf16(chars);

            if(chars.Length == 1)
            {
                return new CodeObjectCreateExpression(
                    typeof(Rune),
                    new CodePrimitiveExpression(chars[0]));
            }
            else if (chars.Length == 2)
            {
                // User is setting to an emoticon or something
                return new CodeObjectCreateExpression(
                    typeof(Rune),
                    new CodePrimitiveExpression(chars[0]),
                    new CodePrimitiveExpression(chars[1]));
            }
            else
            {
                throw new Exception($"Unexpected unicode character size.  Rune was {rune}");
            }
        }

        if (val is Attribute attribute)
        {
            return new CodeSnippetExpression(attribute.ToCode());
        }

        if (val is Dim d)
        {
            return d.ToCode().ToCodeSnippetExpression();
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
                regv.Pattern.ToCodePrimitiveExpression());
        }

        if (val is Pos p)
        {
            // TODO: Get EVERYONE! not just siblings
            return p.ToCode(this.Design.GetSiblings().ToList())
                    .ToCodeSnippetExpression();
        }

        if (val is Enum e)
        {
            var toCode = new EnumToCode(e);
            return toCode.ToCode();
        }

        var type = val.GetType();

        if (type.IsArray)
        {
            var elementType = type.GetElementType();

            var values = ((Array)val).ToList();
            return new CodeArrayCreateExpression(
                elementType ?? throw new Exception($"Type {type} was an Array but {nameof(Type.GetElementType)} returned null"),
                values.Select(v => v.ToCodePrimitiveExpression()).ToArray());
        }

        if (val is IList valList)
        {
            var elementType = type.GetElementTypeEx()
                ?? throw new Exception($"Type {type} was an IList but {nameof(Type.GetElementType)} returned null");

            return new CodeObjectCreateExpression(
                new CodeTypeReference(val.GetType()),
                    new CodeArrayCreateExpression(
                        elementType,
                        valList.Cast<object>().Select(this.ValueFactory).ToArray())
                );
        }

        if (IsListWrapper(type))
        {
            return CreateListWrapperExpression((IListDataSource)val);
        }

        return val.ToCodePrimitiveExpression();
    }

    private bool IsListWrapper(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ListWrapper<>);
    }

    private CodeObjectCreateExpression CreateListWrapperExpression(IListDataSource val)
    {
        var list = val.ToList();

        var listWrapperType = val.GetType();
        
        // Get the generic type argument (T)
        var elementType = listWrapperType.GetGenericArguments()[0];

        // Create an Expression like:
        // new ListWrapper<string>(new ObservableCollection<string>(new string[]{"bob","frank"}))


        var arrayCreateExpression = new CodeArrayCreateExpression(new CodeTypeReference(elementType));

        if (list != null)
        {
            foreach (var v in list)
            {
                CodeExpression element = v == null
                    ? new CodeDefaultValueExpression(new CodeTypeReference(elementType))
                    : new CodePrimitiveExpression(v);
                arrayCreateExpression.Initializers.Add(element);
            }
        }
        
        // Create an expression for new ObservableCollection<T>(new T[]{ ... })
        var observableCollectionType = typeof(ObservableCollection<>).MakeGenericType(elementType);
        var observableCollectionCtor = new CodeObjectCreateExpression(observableCollectionType, arrayCreateExpression);


        // Create the final expression for new ListWrapper<T>(new ObservableCollection<T>(...))
        var genericListWrapperType = typeof(ListWrapper<>).MakeGenericType(elementType);
        var listWrapperCtor = new CodeObjectCreateExpression(genericListWrapperType, observableCollectionCtor);

        return listWrapperCtor;
    }
    private CodeExpression ValueFactory(object val)
    {

        var type = val.GetType();

        // TODO: Could move lots of logic in GetRHS into here
        if (type.GetGenericTypeDefinition() == typeof(SliderOption<>))
        {
            // TODO: this feels very brittle!
            var a1 = type.GetProperty(nameof(SliderOption<object>.Legend)).GetValue(val);
            var a2 = (Rune)type.GetProperty(nameof(SliderOption<object>.LegendAbbr)).GetValue(val);
            var a3 = type.GetProperty(nameof(SliderOption<object>.Data)).GetValue(val);
            

            return new CodeObjectCreateExpression(
                new CodeTypeReference(val.GetType()),
                new CodePrimitiveExpression(a1),
                new CodeObjectCreateExpression(typeof(Rune),new CodePrimitiveExpression(a2.ToString()[0])),
                new CodePrimitiveExpression(a3));
        }
        else
        {
            throw new NotSupportedException($"Cannot generate code for value '{val}'");
        }
    }

    /// <summary>
    /// Gets a CodeDOM code block for the left hand side of an assignment operation e.g.:
    /// <code>this.label1.Text</code>
    /// </summary>
    /// <returns>Left hand side code of property assignment.</returns>
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
    /// and special formatting e.g. for <see cref="NameProperty"/>.
    /// </summary>
    /// <returns>Human readable name for this property (and any sub-property it exists in).</returns>
    public virtual string GetHumanReadableName()
    {
        return this.SubProperty != null ? $"{this.SubProperty}.{this.PropertyInfo.Name}" : this.PropertyInfo.Name;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.GetHumanReadableName() + ":" + this.GetHumanReadableValue();
    }

    /// <summary>
    /// Returns human readable representation of <see cref="GetValue"/> e.g. using
    /// "Yes" and "No" instead of <see langword="true"/>/<see langword="false"/>.
    /// </summary>
    /// <returns>Human readable representation of <see cref="Property"/> current value.</returns>
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

    /// <summary>
    /// Adjust <paramref name="value"/> to match the expectations of <see cref="PropertyInfo"/>
    /// e.g. convert char to <see cref="Rune"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected object? AdjustValueBeingSet(object? value)
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
                value = Dim.Absolute(i);
            }
        }
        // Some Terminal.Gui string properties get angry at null but are ok with empty strings
        if (this.PropertyInfo.PropertyType == typeof(string))
        {
            if (value == null)
            {
                value = string.Empty;
            }
        }

        if (this.PropertyInfo.PropertyType == typeof(IListDataSource))
        {
            if (value is Array a)
            {
                
                // accept arrays as valid input values
                // for setting an IListDataSource.  Just
                // convert them to ListWrappers
                value = a.ToListDataSource();
            }
        }

        return value;
    }

    

    /// <summary>
    /// Calls any methods that update the state of the View
    /// and refresh it against its style e.g. <see cref="TableView.Update"/>.
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
        if (this.Design.View is ColorPicker cp)
        {
            cp.ApplyStyleChanges();
        }

        this.Design.View.SetNeedsDisplay();
    }
}