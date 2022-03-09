using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.Operations;

public class SetPropertyOperation : IOperation
{

    public Design Design { get; }
    public Property Property { get; }
    public object OldValue { get; }
    public object NewValue { get; }

    public SetPropertyOperation(Design design,Property property, object oldValue, object NewValue)
    {
        Design = design;
        Property = property;
        OldValue = oldValue;
        this.NewValue = NewValue;
    }

    public void Do()
    {
        Design.RemoveDesignedProperty(Property.PropertyInfo.Name);
        Property.SetValue(NewValue);
    }

    public void Undo()
    {
        Design.RemoveDesignedProperty(Property.PropertyInfo.Name);
        Property.SetValue(OldValue);
    }

    public void Redo()
    {
        Do();
    }
}
