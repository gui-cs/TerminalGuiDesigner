using Terminal.Gui;
using TerminalGuiDesigner.FromCode;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// <para>
/// Describes the <see cref="Design.FieldName"/> property.  This is the private
/// field name that will be assigned to instance members in .Designer.cs file.</para>
/// <para>
/// Within the .Designer.cs file generated the FieldName will be stored in <see cref="View.Data"/>,
/// this allows runtime instances created from the .Designer.cs (see <see cref="CodeToView"/>) to
/// be effectively mapped to this value e.g.:
/// <code>
/// this.label1.Data = "label1";
/// </code>
/// </para>
/// </summary>
public class NameProperty : Property
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NameProperty"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="View"/> for which you want
    /// user to be able to adjust <see cref="Design.FieldName"/>.</param>
    /// <exception cref="MissingFieldException">Thrown if reflection cannot find
    /// <see cref="Design.FieldName"/>.</exception>
    public NameProperty(Design design)
        : base(
        design,
        typeof(Design).GetProperty(nameof(TerminalGuiDesigner.Design.FieldName)) ?? throw new MissingFieldException("Expected property was missing from Design"))
    {
    }

    /// <summary>
    /// Returns "(Name)", this is consistent with Windows Forms designer.
    /// </summary>
    /// <returns>Fixed string "(Name)".</returns>
    public override string GetHumanReadableName()
    {
        return "(Name)";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{this.GetHumanReadableName()}:{this.Design.FieldName}";
    }

    /// <summary>
    /// Updates the <see cref="Design.FieldName"/> to <paramref name="value"/> after
    /// first ensuring it does not collide with any other field names and is valid
    /// as a field name.
    /// </summary>
    /// <param name="value">New name for the <see cref="Design.FieldName"/> (will be adjusted if invalid e.g. "123Hey").</param>
    public override void SetValue(object? value)
    {
        this.Design.FieldName = this.Design.GetUniqueFieldName(value?.ToString());
    }

    /// <summary>
    /// Returns <see cref="Design.FieldName"/> of wrapped <see cref="Design"/>.
    /// </summary>
    /// <returns>Field name current value.</returns>
    public override object GetValue()
    {
        return this.Design.FieldName;
    }

    /// <summary>
    /// Returns code string for the <see cref="View.Data"/> property of a <see cref="View"/>.
    /// This is where <see cref="Design.FieldName"/> is serialized to when writing out a .Designer.cs
    /// file (so it can be discovered later when file is loaded - see <see cref="CodeToView"/>).
    /// </summary>
    /// <returns>Left hand assignment operand for the <see cref="View.Data"/> property on the private
    /// field that represents the <see cref="Design"/> wrapped by this instance.</returns>
    public override string GetLhs()
    {
        // Set View.Data to the name of the field so that we can
        // determine later on which View instances come from which
        // Fields in the class
        return $"this.{this.Design.FieldName}.{nameof(View.Data)}";
    }
}
