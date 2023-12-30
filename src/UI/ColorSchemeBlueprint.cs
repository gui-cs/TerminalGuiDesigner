using System.Text.Json.Serialization;
using Terminal.Gui;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Serializable version of <see cref="ColorScheme"/>.
/// </summary>
[YamlSerializable]
public record ColorSchemeBlueprint( ColorName NormalForeground, ColorName NormalBackground, ColorName HotNormalForeground, ColorName HotNormalBackground, ColorName FocusForeground, ColorName FocusBackground, ColorName HotFocusForeground, ColorName HotFocusBackground, ColorName DisabledForeground, ColorName DisabledBackground )
{
    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Normal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName NormalForeground { get; init; } = NormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Normal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName NormalBackground { get; init; } = NormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotNormal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName HotNormalForeground { get; init; } = HotNormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotNormal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName HotNormalBackground { get; init; } = HotNormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Focus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName FocusForeground { get; init; } = FocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Focus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName FocusBackground { get; init; } = FocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotFocus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName HotFocusForeground { get; init; } = HotFocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotFocus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    public ColorName HotFocusBackground { get; init; } = HotFocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Disabled"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    [YamlMember( typeof(ColorName), ScalarStyle = ScalarStyle.Plain, DefaultValuesHandling = DefaultValuesHandling.Preserve)]
    public ColorName DisabledForeground { get; init; } = DisabledForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Disabled"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ColorName> ))]
    [YamlMember( typeof(ColorName), ScalarStyle = ScalarStyle.Plain, DefaultValuesHandling = DefaultValuesHandling.Preserve)]
    public ColorName DisabledBackground { get; init; } = DisabledBackground;

    /// <summary>
    /// Gets a new <see cref="ColorScheme"/> from the blueprint.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public ColorScheme Scheme => new ColorScheme
    {
        Normal = new Attribute(this.NormalForeground, this.NormalBackground),
        HotNormal = new Attribute(this.HotNormalForeground, this.HotNormalBackground),
        Focus = new Attribute(this.FocusForeground, this.FocusBackground),
        HotFocus = new Attribute(this.HotFocusForeground, this.HotFocusBackground),
        Disabled = new Attribute(this.DisabledForeground, this.DisabledBackground),
    };
}