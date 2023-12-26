using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.UI;

/// <summary>Serializable settings class for user keybinding/accessibility tailoring.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.General, Converters = new[] {typeof(JsonStringEnumConverter<MouseFlags> )})]
public sealed record KeyMap(
    string EditProperties,
    string ShowContextMenu,
    string ViewSpecificOperations,
    string EditRootProperties,
    string ShowHelp,
    string New,
    string Open,
    string Save,
    string Redo,
    string Undo,
    string Delete,
    string ToggleDragging,
    string AddView,
    string ToggleShowFocused,
    string ToggleShowBorders,
    MouseFlags RightClick,
    string Copy,
    string Paste,
    string Rename,
    string SetShortcut,
    string SelectAll,
    string MoveRight,
    string MoveLeft,
    string MoveUp,
    string MoveDown,
    string ShowColorSchemes )
{
    /// <summary>Initializes a new instance of the <see cref="KeyMap" /> class.</summary>
    public KeyMap( )
        : this(
            Key.F4.ToString( ),
            Key.Enter.ToString( ),
            Key.F4.WithShift.ToString( ),
            Key.F5.ToString( ),
            Key.F1.ToString( ),
            Key.N.WithCtrl.ToString( ),
            Key.O.WithCtrl.ToString( ),
            Key.S.WithCtrl.ToString( ),
            Key.Y.WithCtrl.ToString( ),
            Key.Z.WithCtrl.ToString( ),
            Key.Delete.ToString( ),
            Key.F3.ToString( ),
            Key.F2.ToString( ),
            Key.L.WithCtrl.ToString( ),
            Key.B.WithCtrl.ToString( ),
            MouseFlags.Button3Clicked,
            Key.C.WithCtrl.ToString( ),
            Key.V.WithCtrl.ToString( ),
            Key.R.WithCtrl.ToString( ),
            Key.T.WithCtrl.ToString( ),
            Key.A.WithCtrl.ToString( ),
            Key.CursorRight.WithShift.ToString( ),
            Key.CursorLeft.WithShift.ToString( ),
            Key.CursorUp.WithShift.ToString( ),
            Key.CursorDown.WithShift.ToString( ),
            Key.F6.ToString( ) )
    {
        // Empty - returns default instance from primary constructor.
    }

    /// <summary>
    ///   Gets the string to add a new <see cref="View" /> (and its <see cref="Design" /> wrapper) to the currently focused container
    ///   view (or root).
    /// </summary>
    public string AddView { get; init; } = AddView;

    /// <summary>Gets the string to copy currently selected views.</summary>
    public string Copy { get; init; } = Copy;

    /// <summary>Gets the string to delete the currently selected <see cref="Design" />.</summary>
    public string Delete { get; init; } = Delete;

    /// <summary>
    ///   Gets the string for editing all <see cref="Property" /> of selected <see cref="Design" />s.
    /// </summary>
    public string EditProperties { get; init; } = EditProperties;

    /// <summary>
    ///   Gets the string to edit the <see cref="Property" /> of the root view being edited.
    /// </summary>
    public string EditRootProperties { get; init; } = EditRootProperties;

    /// <summary>Gets the string to nudge the currently focused view down one unit.</summary>
    public string MoveDown { get; init; } = MoveDown;

    /// <summary>Gets the string to nudge the currently focused view left one unit.</summary>
    public string MoveLeft { get; init; } = MoveLeft;

    /// <summary>Gets the string to nudge the currently focused view right one unit.</summary>
    public string MoveRight { get; init; } = MoveRight;

    /// <summary>Gets the string to nudge the currently focused view up one unit.</summary>
    public string MoveUp { get; init; } = MoveUp;

    /// <summary>Gets the string to create a new .Designer.cs and .cs file.</summary>
    public string New { get; init; } = New;

    /// <summary>Gets the string to open an existing .Designer.cs file.</summary>
    public string Open { get; init; } = Open;

    /// <summary>Gets the string to paste the last copied views.</summary>
    public string Paste { get; init; } = Paste;

    /// <summary>Gets the string to <see cref="Operation.Redo" /> the last undone operation.</summary>
    public string Redo { get; init; } = Redo;

    /// <summary>
    ///   Gets the string to rename the field name (when written to .Designer.cs) of the currently selected <see cref="View" /> or
    ///   <see cref="MenuItem" />.
    /// </summary>
    public string Rename { get; init; } = Rename;

    /// <summary>
    ///   Gets the mouse button that opens the right click context menu. Defaults to <see cref="MouseFlags.Button3Clicked" /> which is
    ///   the right mouse button.
    /// </summary>
    [JsonConverter( typeof( JsonStringEnumConverter<MouseFlags> ) )]
    public MouseFlags RightClick { get; init; } = RightClick;

    /// <summary>Gets the string to save the current changes to disk.</summary>
    public string Save { get; init; } = Save;

    /// <summary>
    ///   Gets the string to select all <see cref="Design" /> in the <see cref="Editor" />.
    /// </summary>
    public string SelectAll { get; init; } = SelectAll;

    /// <summary>
    ///   Gets a custom <see cref="ColorScheme" /> to apply to multi selections in designer.
    ///   <remarks>
    ///     Default color is green, this is useful if you have a heavily green theme where it could get confusing what is multi selected
    ///     and what just has focus/uses your custom scheme
    ///   </remarks>
    /// </summary>
    public ColorSchemeBlueprint SelectionColor { get; init; } = new(
        Color.BrightGreen,
        Color.Green,
        Color.BrightGreen,
        Color.Green,
        Color.BrightYellow,
        Color.Green,
        Color.BrightYellow,
        Color.Green,
        Color.BrightGreen,
        Color.Green );

    /// <summary>Gets the string to assign a new shortcut to a <see cref="MenuItem" />.</summary>
    public string SetShortcut { get; init; } = SetShortcut;

    /// <summary>
    ///   Gets the string to open the <see cref="ColorSchemesUI" /> window for creating/deleting <see cref="ColorScheme" /> that can be
    ///   used in the <see cref="Editor" />.
    /// </summary>
    public string ShowColorSchemes { get; init; } = ShowColorSchemes;

    /// <summary>Gets the string to pop up the right click context menu.</summary>
    public string ShowContextMenu { get; init; } = ShowContextMenu;

    /// <summary>Gets the string to show pop up help.</summary>
    public string ShowHelp { get; init; } = ShowHelp;

    /// <summary>Gets the string to turn mouse dragging on/of.</summary>
    public string ToggleDragging { get; init; } = ToggleDragging;

    /// <summary>
    ///   Gets the string to toggle showing dotted borders around views that otherwise do not have visible borders (e.g.
    ///   <see cref="ScrollView" />).
    /// </summary>
    public string ToggleShowBorders { get; init; } = ToggleShowBorders;

    /// <summary>
    ///   Gets the string to toggle showing an overlay with a description of the currently focused view(s).
    /// </summary>
    public string ToggleShowFocused { get; init; } = ToggleShowFocused;

    /// <summary>Gets the string to <see cref="Operation.Undo" /> the last performed operation.</summary>
    public string Undo { get; init; } = Undo;

    /// <summary>
    ///   Gets the string to pop up view specific operations (e.g. add column to <see cref="TableView" />).
    /// </summary>

    public string ViewSpecificOperations { get; init; } = ViewSpecificOperations;

    /// <summary>
    ///   Builds a configuration from the specified yaml file and returns a new instance of a corresponding <see cref="KeyMap" />.
    /// </summary>
    /// <param name="configurationFile">Relative or absolute path to the configuration file to load.</param>
    /// <param name="optional">Whether this file is optional and can be ignored if missing.</param>
    /// <returns>A new instance of a <see cref="KeyMap" /> from the specified file.</returns>
    /// <exception cref="ArgumentException">If <paramref name="configurationFile" /> is a null or empty string.</exception>
    public static KeyMap LoadFromYamlConfigurationFile( string configurationFile = "Keys.yaml", bool optional = true )
    {
        KeyMap? map = new ConfigurationBuilder( ).AddYamlFile( configurationFile, optional, false ).Build( ).Get<KeyMap>( );
        return map ?? new( );
    }

    /// <summary>
    ///   Builds a configuration from the specified yaml file and returns a new instance of a corresponding <see cref="KeyMap" />.
    /// </summary>
    /// <param name="configurationFile">Relative or absolute path to the configuration file to load.</param>
    /// <param name="optional">Whether this file is optional and can be ignored if missing.</param>
    /// <returns>A new instance of a <see cref="KeyMap" /> from the specified file.</returns>
    /// <exception cref="ArgumentException">If <paramref name="configurationFile" /> is a null or empty string.</exception>
    public static KeyMap LoadFromJsonConfigurationFile( string configurationFile = "Keys.json", bool optional = true, string? section = null )
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder( ).AddJsonFile( configurationFile, optional, false ).Build( );
        return section switch
        {
            null => configurationRoot.Get<KeyMap>( ),
            { Length: > 0 } => configurationRoot.GetRequiredSection( section ).Get<KeyMap>( ),
            _ => null
        } ?? new( );
    }
}
