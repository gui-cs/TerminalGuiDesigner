using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// <para><see cref="Property"/> wrapper for <see cref="View.ColorScheme"/>.
/// </para>
/// <para>
/// In Terminal.Gui the <see cref="View.ColorScheme"/> has inbuilt inheritance
/// to parental containers.  This class handles detecting if the current value comes
/// from a parent or is explicitly declared.  Also interacts with <see cref="ColorSchemeManager"/>
/// which tracks what <see cref="ColorScheme"/> user has declared and/or is using in
/// the editor so that they can be written out to .Designer.cs at ToCode time.
/// </para>
/// </summary>
public class ColorSchemeProperty : Property
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorSchemeProperty"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="View"/> upon which the
    /// <see cref="View.ColorScheme"/> is to be managed.</param>
    /// <exception cref="Exception">Thrown if Terminal.Gui public API is
    /// changed and <see cref="View.ColorScheme"/> can no longer be found
    /// with reflection.</exception>
    public ColorSchemeProperty(Design design)
        : base(
            design,
            design.View.GetType().GetProperty(nameof(View.ColorScheme)) ?? throw new Exception("ColorScheme property has changed name!"))
    {
    }

    /// <summary>
    /// Adds an assignment block of CodeDOM code to <paramref name="args"/>.  In the case
    /// of <see cref="ColorSchemeProperty"/> this is a reference assignment to a privately declared
    /// <see cref="ColorScheme"/> within <see cref="CodeDomArgs.Class"/> e.g.:
    /// <code>this.btn1.ColorScheme = this.greenOnBlack;</code>
    /// </summary>
    /// <param name="args">Current state of the .Designer.cs file.</param>
    /// <remarks>If no explicit <see cref="ColorScheme"/> is set
    /// (i.e. current scheme is inherited) then this method has
    /// no effect (no code is added).</remarks>
    public override void ToCode(CodeDomArgs args)
    {
        // if no explicit color scheme defined
        // then we don't output any code (view's
        // scheme is inherited)
        if (!this.Design.HasKnownColorScheme())
        {
            return;
        }

        // Note that this branch calls GetRhs()
        base.ToCode(args);
    }

    /// <summary>
    /// Gets a CodeDOM code block for the right hand side of an assignment operation. In the
    /// case of <see cref="ColorSchemeProperty"/> this is a reference assignment to a privately declared
    /// <see cref="ColorScheme"/> within <see cref="CodeDomArgs.Class"/> e.g.:
    /// <code>this.greenOnBlack;</code>
    /// </summary>
    /// <returns>Right hand side code of property assignment.</returns>
    public override CodeExpression GetRhs()
    {
        var s = this.GetValue() as ColorScheme;

        var name = ColorSchemeManager.Instance.GetNameForColorScheme(s
            ?? throw new Exception("GetRhs is only valid when there is a known ColorScheme"));

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exception("GetRhs is only valid when there is a known ColorScheme");
        }

        return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name);
    }

    /// <summary>
    /// Returns the explicitly declared <see cref="ColorScheme"/> on the <see cref="View"/>
    /// this <see cref="Property"/> is pointed at or null if the current value is inherited
    /// from a parent container.
    /// </summary>
    /// <returns>Explicitly declared <see cref="ColorScheme"/> or null.</returns>
    public override object? GetValue()
    {
        return this.Design.State.OriginalScheme ?? this.Design.View.GetExplicitColorScheme();
    }

    /// <inheritdoc/>
    public override void SetValue(object? value)
    {
        base.SetValue(value);

        this.Design.State.OriginalScheme = value as ColorScheme;
    }

    /// <summary>
    /// <para>Returns the human readable value for <see cref="ColorScheme"/> (if explicitly defined on
    /// the <see cref="View"/>) as tracked by <see cref="ColorSchemeManager"/>.  This will be the
    /// private field name in the .Designer.cs e.g. "greenOnBlack".
    /// </para>
    /// <para>If no explicit <see cref="ColorScheme"/> is defined (i.e. value is inherited from parent
    /// container) then the string "(Inherited)" is returned.</para>
    /// </summary>
    /// <returns>Name of explicit <see cref="ColorScheme"/> as tracked by <see cref="ColorSchemeManager"/>
    /// or "(Inherited)".</returns>
    protected override string GetHumanReadableValue()
    {
        const string inherited = "(Inherited)";

        if (!this.Design.HasKnownColorScheme())
        {
            return inherited;
        }

        var s = this.GetValue() as ColorScheme;

        if (s == null)
        {
            return inherited;
        }

        return ColorSchemeManager.Instance.GetNameForColorScheme(s) ?? "Unknown ColorScheme";
    }
}
