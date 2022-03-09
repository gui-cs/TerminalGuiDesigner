
using System.Reflection;
using TerminalGuiDesigner.UI.Windows;

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
        Property.SetValue(NewValue);
    }

    public void Undo()
    {
        Property.SetValue(OldValue);
    }

    public void Redo()
    {
        Do();
    }
}
