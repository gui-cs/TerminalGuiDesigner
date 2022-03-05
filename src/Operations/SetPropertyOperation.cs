
using System.Reflection;

namespace TerminalGuiDesigner.Operations;

public class SetPropertyOperation : IOperation
{

    public Design Design { get; }
    public PropertyInfo Property { get; }
    public object OldValue { get; }
    public object NewValue { get; }

    public SetPropertyOperation(Design design,PropertyInfo property, object oldValue, object NewValue)
    {
        Design = design;
        Property = property;
        OldValue = oldValue;
        this.NewValue = NewValue;
    }

    public void Do()
    {
        Design.SetDesignablePropertyValue(Property,NewValue);
    }

    public void Undo()
    {
        Design.SetDesignablePropertyValue(Property,OldValue);
    }

    public void Redo()
    {
        Do();
    }
}
