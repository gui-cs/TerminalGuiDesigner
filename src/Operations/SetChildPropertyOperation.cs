using System.Reflection;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Changes a property on a child object (e.g. <see cref="MenuItem"/>) that has no
/// <see cref="Design"/> wrapper.  Mirrors the memento pattern of <see cref="SetPropertyOperation"/>
/// but uses raw <see cref="PropertyInfo"/> rather than the <see cref="Property"/> abstraction.
/// </summary>
public class SetChildPropertyOperation : Operation
{
    private readonly object target;
    private readonly PropertyInfo property;
    private readonly object? oldValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetChildPropertyOperation"/> class.
    /// Captures the current property value as the old value immediately.
    /// </summary>
    public SetChildPropertyOperation(IApplication app, object target, PropertyInfo property)
        : base(app)
    {
        this.target = target;
        this.property = property;
        this.oldValue = property.GetValue(target);
        this.NewValue = this.oldValue;
    }

    /// <summary>Gets or sets the new value to assign. Updated on each keystroke for pending operations.</summary>
    public object? NewValue { get; set; }

    /// <summary>Gets the object being modified. Used for identity checks on pending operations.</summary>
    public object Target => this.target;

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (Equals(this.oldValue, this.NewValue))
        {
            return false;
        }

        this.property.SetValue(this.target, this.NewValue);
        (this.target as View)?.SetNeedsDraw();
        return true;
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        this.property.SetValue(this.target, this.oldValue);
        (this.target as View)?.SetNeedsDraw();
    }

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        this.property.SetValue(this.target, this.NewValue);
        (this.target as View)?.SetNeedsDraw();
    }
}
