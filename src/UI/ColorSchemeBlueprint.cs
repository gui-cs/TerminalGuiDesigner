using System.Text.Json.Serialization;
using Terminal.Gui;
using Terminal.Gui.Drawing;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Serializable version of <see cref="Scheme"/>.
/// </summary>
[YamlSerializable]
public record SchemeBlueprint( Color NormalForeground, Color NormalBackground, Color HotNormalForeground, Color HotNormalBackground, Color FocusForeground, Color FocusBackground, Color HotFocusForeground, Color HotFocusBackground, Color DisabledForeground, Color DisabledBackground )
{
    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Terminal.Gui.Drawing.Scheme.Normal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public Color NormalForeground { get; init; } = NormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.Normal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public Color NormalBackground { get; init; } = NormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.HotNormal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public Color HotNormalForeground { get; init; } = HotNormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.HotNormal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public Color HotNormalBackground { get; init; } = HotNormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.Focus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public Color FocusForeground { get; init; } = FocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.Focus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public Color FocusBackground { get; init; } = FocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.HotFocus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public Color HotFocusForeground { get; init; } = HotFocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.HotFocus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public Color HotFocusBackground { get; init; } = HotFocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.Disabled"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    public Color DisabledForeground { get; init; } = DisabledForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="Scheme.Disabled"/> <see cref="Attribute.Background"/>.
    /// </summary>
    public Color DisabledBackground { get; init; } = DisabledBackground;

    /// <summary>
    /// Gets a new <see cref="Scheme"/> from the blueprint.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public Scheme Scheme => new Scheme
    {
        Normal = new Attribute(this.NormalForeground, this.NormalBackground),
        HotNormal = new Attribute(this.HotNormalForeground, this.HotNormalBackground),
        Focus = new Attribute(this.FocusForeground, this.FocusBackground),
        HotFocus = new Attribute(this.HotFocusForeground, this.HotFocusBackground),
        Disabled = new Attribute(this.DisabledForeground, this.DisabledBackground),
    };
}