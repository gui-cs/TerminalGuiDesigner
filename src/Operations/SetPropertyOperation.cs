using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Delegate for providing a new value for a <see cref="Property"/> e.g. by launching a modal dialog.
/// </summary>
/// <param name="property">The property to change.  Usually this is a wrapper for <see cref="PropertyInfo"/>
/// but sometimes it is a meta setting like <see cref="NameProperty"/> (field name in .Designer.cs).</param>
/// <param name="currentValue">The current value held by <see cref="Property"/>.</param>
/// <returns>The new value to set for the property.</returns>
public delegate object? PropertyValueGetterDelegate(Property property, object? currentValue);

/// <summary>
/// Changes the value of a <see cref="Property"/> on one or more <see cref="Designs"/>.  Usually this is a <see cref="PropertyInfo"/>
/// on a <see cref="View"/> but can be a meta property like <see cref="NameProperty"/> (field name in .Designer.cs).
/// </summary>
public class SetPropertyOperation : Operation
{
    private readonly PropertyValueGetterDelegate? valueGetter;

    private readonly SetPropertyMemento[] mementos;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPropertyOperation"/> class.
    /// Operation that changes <paramref name="property"/> to have a new value.  Value is
    /// sought by invoking the <paramref name="valueGetter"/> delegate at <see cref="IOperation.Do"/>
    /// time.  Throw <see cref="OperationCanceledException"/> in delegate if you want to perform last
    /// minute cancellation instead of returning a new value to set.
    /// </summary>
    /// <param name="design">A single <see cref="Design"/> on which to change a single <paramref name="property"/>.</param>
    /// <param name="property">Property to change (see <see cref="Design.GetDesignableProperties()"/>).</param>
    /// <param name="valueGetter">Delegate for fetching the new value for the <paramref name="property"/> when
    /// command is run e.g. via a <see cref="Modals"/> dialog.</param>
    public SetPropertyOperation(Design design, Property property, PropertyValueGetterDelegate valueGetter)
        : this(design, property, property.GetValue(), null)
    {
        this.valueGetter = valueGetter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPropertyOperation"/> class.
    /// Operation that changes the <paramref name="property"/> to have a specific <paramref name="newValue"/>.
    /// </summary>
    /// <param name="design">A single <see cref="Design"/> on which to change a single <paramref name="property"/>.</param>
    /// <param name="property">Property to change (see <see cref="Design.GetDesignableProperties()"/>).</param>
    /// <param name="oldValue">The old value that <paramref name="property"/> had.</param>
    /// <param name="newValue">The new value you want to assign to <paramref name="property"/>.</param>
    public SetPropertyOperation(Design design, Property property, object? oldValue, object? newValue)
    {
        this.mementos = new[]
        {
            new SetPropertyMemento(design, property, oldValue),
        };

        this.NewValue = newValue;

        // don't let user rename the root
        if (property is NameProperty && design.IsRoot)
        {
            this.IsImpossible = true;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPropertyOperation"/> class.
    /// Constructor for setting the same property on multiple views at once (e.g. change color scheme on
    /// all multi selected views).
    /// </summary>
    /// <param name="designs">All <see cref="Design"/> for which you want to change
    /// <paramref name="propertyName"/>.</param>
    /// <param name="propertyName">The name of a designable <see cref="Property"/> on
    /// all the <paramref name="designs"/> (See <see cref="Design.GetDesignableProperties"/>).</param>
    /// <param name="valueGetter">Delegate for fetching the new value for
    /// the <paramref name="propertyName"/> when command is run e.g. via a <see cref="Modals"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName"/> is not found amongst <paramref name="designs"/> properties.</exception>
    public SetPropertyOperation(Design[] designs, string propertyName, PropertyValueGetterDelegate valueGetter)
    {
        this.valueGetter = valueGetter;
        var mementos = new List<SetPropertyMemento>();

        foreach (var d in designs)
        {
            var p = d.GetDesignableProperty(propertyName);
            if (p != null)
            {
                mementos.Add(new SetPropertyMemento(d, p, p.GetValue()));
            }
        }

        this.mementos = mementos.ToArray();

        if (mementos.Count == 0)
        {
            throw new ArgumentException($"Could not find designable Property called '{propertyName}' on {designs.Length} Design instances");
        }
    }

    /// <summary>
    /// Gets or Sets the new value to assign to <see cref="Property"/>.
    /// </summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// Gets all <see cref="Design"/> that will be updated by this <see cref="Operation"/>.
    /// </summary>
    public IReadOnlyCollection<Design> Designs
    {
        get
        {
            return this.mementos.Select(m => m.Design).ToList().AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        foreach (var m in this.mementos)
        {
            m.Property.SetValue(m.OldValue);
        }
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        foreach (var m in this.mementos)
        {
            m.Property.SetValue(this.NewValue);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.mementos.First().Property.GetHumanReadableName();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.valueGetter != null)
        {
            // theres nothing to set!
            if (this.mementos.Length == 0)
            {
                return false;
            }

            try
            {
                // Are we setting on a single Design and/or for a bunch of Designs that share the same current value
                // for this property?
                var currentVals = this.mementos.Select(p => p.Property.GetValue()).Distinct().ToArray();

                // are we setting multiple at once? and the current values are different? If so then pass null for current value (no consensus)
                this.NewValue = currentVals.Length == 1 ?
                    this.valueGetter(this.mementos[0].Property, currentVals[0]) :
                    this.valueGetter(this.mementos[0].Property, null);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        foreach (var m in this.mementos)
        {
            m.Property.SetValue(this.NewValue);
        }

        return true;
    }

    private class SetPropertyMemento
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetPropertyMemento"/> class.
        /// Captures a value change for a <see cref="Property"/> in a reversible Memento pattern class.
        /// </summary>
        /// <param name="design">Wrapper for <see cref="View"/> on which you want to change the <paramref name="property"/>.</param>
        /// <param name="property">What to change.</param>
        /// <param name="oldValue">The new value to change it to.</param>
        public SetPropertyMemento(Design design, Property property, object? oldValue)
        {
            this.Design = design;
            this.Property = property;
            this.OldValue = oldValue;
        }

        public Design Design { get; }

        public Property Property { get; }

        public object? OldValue { get; }
    }
}
