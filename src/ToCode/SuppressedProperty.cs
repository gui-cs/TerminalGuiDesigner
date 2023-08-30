using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// A <see cref="Property"/> whose value is stored but does not manifest within
/// the editor because it would make design time operation difficult e.g. <see cref="View.Visible"/>.
/// </summary>
public class SuppressedProperty : Property
{
    private object? value;

    /// <inheritdoc cref="Property(Design, PropertyInfo)"/>
    public SuppressedProperty(Design design, PropertyInfo property, object? designTimeValue)
        : base(design, property)
    {
        this.StoreInitialValueButUse(designTimeValue);
    }

    /// <inheritdoc cref="Property(Design, PropertyInfo, string, object)"/>
    public SuppressedProperty(Design design, PropertyInfo property, string subProperty, object declaringObject, object? designTimeValue)
        : base(design, property, subProperty, declaringObject)
    {
        this.StoreInitialValueButUse(designTimeValue);
    }

    /// <inheritdoc/>
    public override object? GetValue()
    {
        return this.value;
    }

    /// <inheritdoc/>
    public override void SetValue(object? value)
    {
        this.value = this.AdjustValueBeingSet(value);
    }

    private void StoreInitialValueButUse(object? designTimeValue)
    {
        // Pull the compiled/created value from the view (e.g. Visible=false)
        this.value = base.GetValue();

        // But immediately override with the design time view (e.g. Visible=true)
        base.SetValue(designTimeValue);
    }
}
