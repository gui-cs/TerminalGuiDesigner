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
public record ColorSchemeBlueprint( Color NormalForeground, Color NormalBackground, Color HotNormalForeground, Color HotNormalBackground, Color FocusForeground, Color FocusBackground, Color HotFocusForeground, Color HotFocusBackground, Color DisabledForeground, Color DisabledBackground )
{
    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Normal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color NormalForeground { get; init; } = NormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Normal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color NormalBackground { get; init; } = NormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotNormal"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color HotNormalForeground { get; init; } = HotNormalForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotNormal"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color HotNormalBackground { get; init; } = HotNormalBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Focus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color FocusForeground { get; init; } = FocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Focus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color FocusBackground { get; init; } = FocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotFocus"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color HotFocusForeground { get; init; } = HotFocusForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.HotFocus"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    public Color HotFocusBackground { get; init; } = HotFocusBackground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Disabled"/> <see cref="Attribute.Foreground"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    [YamlMember( typeof(Color), ScalarStyle = ScalarStyle.Plain, DefaultValuesHandling = DefaultValuesHandling.Preserve)]
    public Color DisabledForeground { get; init; } = DisabledForeground;

    /// <summary>
    /// Gets the <see cref="Color"/> to use for <see cref="ColorScheme.Disabled"/> <see cref="Attribute.Background"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Color> ))]
    [YamlMember( typeof(Color), ScalarStyle = ScalarStyle.Plain, DefaultValuesHandling = DefaultValuesHandling.Preserve)]
    public Color DisabledBackground { get; init; } = DisabledBackground;

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