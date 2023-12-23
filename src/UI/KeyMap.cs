using Microsoft.Extensions.Configuration;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Serializable settings class for user keybinding/accessibility tailoring.
/// </summary>
public record KeyMap(
    Key EditProperties,
    Key ShowContextMenu,
    Key ViewSpecificOperations,
    Key EditRootProperties,
    Key ShowHelp,
    Key New,
    Key Open,
    Key Save,
    Key Redo,
    Key Undo,
    Key Delete,
    Key ToggleDragging,
    Key AddView,
    Key ToggleShowFocused,
    Key ToggleShowBorders,
    MouseFlags RightClick,
    Key Copy,
    Key Paste,
    Key Rename,
    Key SetShortcut,
    Key SelectAll,
    Key MoveRight,
    Key MoveLeft,
    Key MoveUp,
    Key MoveDown,
    Key ShowColorSchemes )
{
    /// <summary>Initializes a new instance of the <see cref="KeyMap"/> class.</summary>
    public KeyMap( )
        : this(
            Key.F4,
            Key.Enter,
            KeyCode.ShiftMask | KeyCode.F4,
            Key.F5,
            Key.F1,
            KeyCode.CtrlMask | KeyCode.N,
            KeyCode.CtrlMask | KeyCode.O,
            KeyCode.CtrlMask | KeyCode.S,
            KeyCode.CtrlMask | KeyCode.Y,
            KeyCode.CtrlMask | KeyCode.Z,
            Key.DeleteChar,
            Key.F3,
            Key.F2,
            KeyCode.CtrlMask | KeyCode.L,
            KeyCode.CtrlMask | KeyCode.B,
            MouseFlags.Button3Clicked,
            KeyCode.CtrlMask | KeyCode.C,
            KeyCode.CtrlMask | KeyCode.V,
            KeyCode.CtrlMask | KeyCode.R,
            KeyCode.CtrlMask | KeyCode.T,
            KeyCode.CtrlMask | KeyCode.A,
            KeyCode.ShiftMask | KeyCode.CursorRight,
            KeyCode.ShiftMask | KeyCode.CursorLeft,
            KeyCode.ShiftMask | KeyCode.CursorUp,
            KeyCode.ShiftMask | KeyCode.CursorDown,
            Key.F6 )
    {
        // Empty - returns default instance from primary constructor.
    }

    /// <summary>
    /// Gets the key for editing all <see cref="Property"/> of selected <see cref="Design"/>s.
    /// </summary>
    public Key EditProperties { get; init; } = EditProperties;

    /// <summary>
    /// Gets the key to pop up the right click context menu.
    /// </summary>
    public Key ShowContextMenu { get; init; } = ShowContextMenu;

    /// <summary>
    /// Gets the key to pop up view specific operations (e.g. add column to <see cref="TableView"/>).
    /// </summary>
    public Key ViewSpecificOperations { get; init; } = ViewSpecificOperations;

    /// <summary>
    /// Gets the key to edit the <see cref="Property"/> of the root view being edited.
    /// </summary>
    public Key EditRootProperties { get; init; } = EditRootProperties;

    /// <summary>
    /// Gets the key to show pop up help.
    /// </summary>
    public Key ShowHelp { get; init; } = ShowHelp;

    /// <summary>
    /// Gets the key to create a new .Designer.cs and .cs file.
    /// </summary>
    public Key New { get; init; } = New;

    /// <summary>
    /// Gets the key to open an existing .Designer.cs file.
    /// </summary>
    public Key Open { get; init; } = Open;

    /// <summary>
    /// Gets the key to save the current changes to disk.
    /// </summary>
    public Key Save { get; init; } = Save;

    /// <summary>
    /// Gets the key to <see cref="Operation.Redo"/> the last undone operation.
    /// </summary>
    public Key Redo { get; init; } = Redo;

    /// <summary>
    /// Gets the key to <see cref="Operation.Undo"/> the last performed operation.
    /// </summary>
    public Key Undo { get; init; } = Undo;

    /// <summary>
    /// Gets the key to delete the currently selected <see cref="Design"/>.
    /// </summary>
    public Key Delete { get; init; } = Delete;

    /// <summary>
    /// Gets the key to turn mouse dragging on/of.
    /// </summary>
    public Key ToggleDragging { get; init; } = ToggleDragging;

    /// <summary>
    /// Gets the key to add a new <see cref="View"/> (and its <see cref="Design"/> wrapper)
    /// to the currently focused container view (or root).
    /// </summary>
    public Key AddView { get; init; } = AddView;

    /// <summary>
    /// Gets the key to toggle showing an overlay with a description of the currently focused
    /// view(s).
    /// </summary>
    public Key ToggleShowFocused { get; init; } = ToggleShowFocused;

    /// <summary>
    /// Gets the key to toggle showing dotted borders around views that otherwise do not have
    /// visible borders (e.g. <see cref="ScrollView"/>).
    /// </summary>
    public Key ToggleShowBorders { get; init; } = ToggleShowBorders;

    /// <summary>
    /// Gets the mouse button that opens the right click context menu.
    /// Defaults to <see cref="MouseFlags.Button3Clicked"/> which is the right
    /// mouse button.
    /// </summary>
    public MouseFlags RightClick { get; init; } = RightClick;

    /// <summary>
    /// Gets the key to copy currently selected views.
    /// </summary>
    public Key Copy { get; init; } = Copy;

    /// <summary>
    /// Gets the key to paste the last copied views.
    /// </summary>
    public Key Paste { get; init; } = Paste;

    /// <summary>
    /// Gets the key to rename the field name (when written to .Designer.cs) of
    /// the currently selected <see cref="View"/> or <see cref="MenuItem"/>.
    /// </summary>
    public Key Rename { get; init; } = Rename;

    /// <summary>
    /// Gets the key to assign a new shortcut to a <see cref="MenuItem"/>.
    /// </summary>
    public Key SetShortcut { get; init; } = SetShortcut;

    /// <summary>
    /// Gets the key to select all <see cref="Design"/> in the <see cref="Editor"/>.
    /// </summary>
    public Key SelectAll { get; init; } = SelectAll;

    /// <summary>
    /// Gets the key to nudge the currently focused view right one unit.
    /// </summary>
    public Key MoveRight { get; init; } = MoveRight;

    /// <summary>
    /// Gets the key to nudge the currently focused view left one unit.
    /// </summary>
    public Key MoveLeft { get; init; } = MoveLeft;

    /// <summary>
    /// Gets the key to nudge the currently focused view up one unit.
    /// </summary>
    public Key MoveUp { get; init; } = MoveUp;

    /// <summary>
    /// Gets the key to nudge the currently focused view down one unit.
    /// </summary>
    public Key MoveDown { get; init; } = MoveDown;

    /// <summary>
    /// Gets the key to open the <see cref="ColorSchemesUI"/> window for
    /// creating/deleting <see cref="ColorScheme"/> that can be used in the <see cref="Editor"/>.
    /// </summary>
    public Key ShowColorSchemes { get; init; } = ShowColorSchemes;

    /// <summary>
    /// Gets a custom <see cref="ColorScheme"/> to apply to multi
    /// selections in designer.
    /// <remarks>Default color is green, this is useful if you have a heavily
    /// green theme where it could get confusing what is multi selected and what
    /// just has focus/uses your custom scheme</remarks>
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

    /// <summary>
    /// Builds a configuration from the specified yaml file and returns a new instance of a corresponding <see cref="KeyMap"/>.
    /// </summary>
    /// <param name="configurationFile">Relative or absolute path to the configuration file to load.</param>
    /// <returns>A new instance of a <see cref="KeyMap"/> from the specified file.</returns>
    /// <exception cref="ArgumentException">If <paramref name="configurationFile"/> is a null or empty string.</exception>
    public static KeyMap LoadFromConfiguration( string? configurationFile = "Keys.yaml" )
    {
        return new ConfigurationBuilder().AddYamlFile( configurationFile, false, false ).Build().Get<KeyMap>() ?? new KeyMap();
    }
}
