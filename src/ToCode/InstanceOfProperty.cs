using System.CodeDom;
using System.Linq.Expressions;
using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// <see cref="Property"/> which requires setting to one of several classes derrived from
/// a shared base.  Classes must have a blank constructor e.g.<see cref="SpinnerView.Style"/>.
/// </summary>
public class InstanceOfProperty : Property
{
    /// <summary>
    /// Places a restriction on the <see cref="Property.SetValue"/> that
    /// can be supplied. When setting the value must be a class derrived
    /// from this <see cref="Type"/>.
    /// </summary>
    public Type MustBeDerivedFrom { get; }

    /// <summary>
    /// Creates a new instance of designable property <paramref name="property"/>.
    /// In which values set <see cref="MustBeDerivedFrom"/> the supplied
    /// <see cref="PropertyInfo.PropertyType"/>
    /// </summary>
    /// <param name="design"></param>
    /// <param name="property"></param>
    /// <exception cref="Exception"></exception>
    public InstanceOfProperty(Design design, PropertyInfo property)
        : base(design, property)
    {
        this.MustBeDerivedFrom = property.PropertyType
            ?? throw new Exception("Unable to determine property type");
    }

    /// <inheritdoc/>
    public override CodeExpression GetRhs()
    {
        var instance = this.GetValue();
        if (instance != null)
        {
            return new CodeObjectCreateExpression(instance.GetType());
        }
        else
        {
            return new CodeSnippetExpression("null");
        }
    }
}
