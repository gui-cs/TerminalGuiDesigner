using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.Operations;

public class SetPropertyOperation : Operation
{
    public Design Design { get; }
    public Property Property { get; }
    public object? OldValue { get; }
    public object? NewValue { get; set;}

    public SetPropertyOperation(Design design,Property property, object? oldValue, object? NewValue)
    {
        Design = design;
        Property = property;
        OldValue = oldValue;
        this.NewValue = NewValue;
    }

    public override bool Do()
    {
        Property.SetValue(NewValue);
        return true;
    }

    public override void Undo()
    {
        Property.SetValue(OldValue);
    }

    public override void Redo()
    {
        Do();
    }
}
