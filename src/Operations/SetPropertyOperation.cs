using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Delegate for providing a new value for a <see cref="Property"/> e.g. by launching a modal dialog
/// </summary>
/// <param name="property"></param>
/// <param name="currentValue"></param>
/// <returns></returns>
public delegate object? PropertyValueGetterDelegate(Property property, object? currentValue);

public class SetPropertyOperation : Operation
{   

    private PropertyValueGetterDelegate? _valueGetter;

    private class SetPropertyMemento
    {
        public Design Design { get; }

        public Property Property { get; }
        public object? OldValue { get; }

        public SetPropertyMemento(Design design, Property property, object? oldValue)
        {
            Design = design;
            Property = property;
            OldValue = oldValue;
        }
    }

    SetPropertyMemento[] _mementos;

    public object? NewValue { get; set; }

    public Design Design
    {
        get
        {
            return _mementos.Length == 1 ?
                _mementos[0].Design
                : throw new Exception("Design property cannot be used when operation is configured to update multiple views at once");
        }
    }

    /// <summary>
    /// Operation that changes <paramref name="property"/> to have a new value.  Value is sought by invoking
    /// the <paramref name="valueGetter"/> delegate at <see cref="IOperation.Do"/> time.  Throw <see cref="OperationCanceledException"/>
    /// in delegate if you want to perform last minute cancellation instead of returning a new value to set.
    /// </summary>
    /// <param name="design"></param>
    /// <param name="property"></param>
    /// <param name="valueGetter"></param>
    public SetPropertyOperation(Design design,Property property, PropertyValueGetterDelegate valueGetter)
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
        _mementos = new[] {
            new SetPropertyMemento(design,property,oldValue)
        };

        this.NewValue = newValue;

        // don't let user rename the root
        if(property is NameProperty && design.IsRoot)
            IsImpossible = true;
    }

    /// <summary>
    /// Constructor for setting the same property on multiple views at once (e.g. change color scheme on
    /// all multi selected views).
    /// </summary>
    /// <param name="designs"></param>
    /// <param name="propertyName"></param>
    /// <param name="valueGetter"></param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName"/> is not found amongst <paramref name="designs"/> properties</exception>
    public SetPropertyOperation(Design[] designs, string propertyName, PropertyValueGetterDelegate valueGetter)
    {
        _valueGetter = valueGetter;
        var mementos = new List<SetPropertyMemento>();

        foreach (var d in designs)
        {
            var p = d.GetDesignableProperty(propertyName);
            if(p != null)
            {
                mementos.Add(new SetPropertyMemento(d, p, p.GetValue()));
            }
        }

        _mementos = mementos.ToArray();
        
        if(mementos.Count == 0)
            throw new ArgumentException($"Could not find designable Property called '{propertyName}' on {designs.Length} Design instances");
    }

    public override bool Do()
    {
        if(_valueGetter != null)
        {

            // theres nothing to set!
            if (_mementos.Length == 0)
            {
                return false;
            }

            try
            {
                // Are we setting on a single Design and/or for a bunch of Designs that share the same current value
                // for this property?
                var currentVals = _mementos.Select(p => p.Property.GetValue()).Distinct().ToArray();

                NewValue = currentVals.Length == 1 ?
                    // yes
                    _valueGetter(_mementos[0].Property, currentVals[0]) :
                    // we are setting multiple at once and the current values are different so just tell the user theres no value
                    _valueGetter(_mementos[0].Property, null); 
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        foreach(var m in _mementos)
        {
            m.Property.SetValue(NewValue);
        }
        return true;
    }

    public override void Undo()
    {

        foreach (var m in _mementos)
        {
            m.Property.SetValue(m.OldValue);
        }
    }

    public override void Redo()
    {
        foreach (var m in _mementos)
        {
            m.Property.SetValue(NewValue);
        }
    }

    public override string ToString()
    {
        return _mementos.First().Property.GetHumanReadableName();
    }
}
