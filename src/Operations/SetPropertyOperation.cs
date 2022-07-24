using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.Operations;

public class SetPropertyOperation : Operation
{
    private Func<Property, object?>? _valueGetter;

    public Design Design { get; }
    public Property Property { get; }
    public object? OldValue { get; }
    public object? NewValue { get; set;}

    /// <summary>
    /// Operation that changes <paramref name="property"/> to have a new value.  Value is sought by invoking
    /// the <paramref name="valueGetter"/> delegate at <see cref="IOperation.Do"/> time.  Throw <see cref="OperationCanceledException"/>
    /// in delegate if you want to perform last minute cancellation instead of returning a new value to set.
    /// </summary>
    /// <param name="design"></param>
    /// <param name="property"></param>
    /// <param name="valueGetter"></param>
    public SetPropertyOperation(Design design,Property property, Func<Property, object?> valueGetter)
        :this(design, property, property.GetValue(), null)
    {
        _valueGetter = valueGetter;
    }

    /// <summary>
    /// Operation that changes the <paramref name="property"/> to have the <see cref="newValue"/>
    /// </summary>
    /// <param name="design"></param>
    /// <param name="property"></param>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public SetPropertyOperation(Design design,Property property, object? oldValue, object? newValue)
    {
        Design = design;
        Property = property;
        OldValue = oldValue;
        this.NewValue = newValue;

        // don't let user rename the root
        if(property is NameProperty && design.IsRoot)
            IsImpossible = true;
    }

    public override bool Do()
    {
        if(_valueGetter != null)
        {
            try
            {
                NewValue = _valueGetter(Property);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        Property.SetValue(NewValue);
        return true;
    }

    public override void Undo()
    {
        Property.SetValue(OldValue);
    }

    public override void Redo()
    {
        Property.SetValue(NewValue);
    }

    public override string ToString()
    {
        return Property.GetHumanReadableName();
    }
}
