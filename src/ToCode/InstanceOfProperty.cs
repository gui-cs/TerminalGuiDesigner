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
    public Type MustBeDerrivedFrom { get; }

    public InstanceOfProperty(Design design, PropertyInfo property)
        : base(design, property)
    {
        this.MustBeDerrivedFrom = property.PropertyType
            ?? throw new Exception("Unable to determine property type");
    }

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
