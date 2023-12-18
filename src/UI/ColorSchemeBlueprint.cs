using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Serializable version of <see cref="ColorScheme"/>.
/// </summary>
public record ColorSchemeBlueprint( ColorName NormalForeground, ColorName NormalBackground, ColorName HotNormalForeground, ColorName HotNormalBackground, ColorName FocusForeground, ColorName FocusBackground, ColorName HotFocusForeground, ColorName HotFocusBackground, ColorName DisabledForeground, ColorName DisabledBackground )
{
    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Normal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public ColorName NormalForeground { get; init; } = NormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Normal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public ColorName NormalBackground { get; init; } = NormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotNormal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public ColorName HotNormalForeground { get; init; } = HotNormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotNormal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public ColorName HotNormalBackground { get; init; } = HotNormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Focus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public ColorName FocusForeground { get; init; } = FocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Focus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public ColorName FocusBackground { get; init; } = FocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotFocus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public ColorName HotFocusForeground { get; init; } = HotFocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotFocus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public ColorName HotFocusBackground { get; init; } = HotFocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Disabled"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public ColorName DisabledForeground { get; init; } = DisabledForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Disabled"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public ColorName DisabledBackground { get; init; } = DisabledBackground;

    /// <summary>
    /// Gets a new <see cref="ColorScheme"/> from the blueprint.
    /// </summary>
    public ColorScheme Scheme => new ColorScheme
    {
        Normal = new Attribute(this.NormalForeground, this.NormalBackground),
        HotNormal = new Attribute(this.HotNormalForeground, this.HotNormalBackground),
        Focus = new Attribute(this.FocusForeground, this.FocusBackground),
        HotFocus = new Attribute(this.HotFocusForeground, this.HotFocusBackground),
        Disabled = new Attribute(this.DisabledForeground, this.DisabledBackground),
    };
}