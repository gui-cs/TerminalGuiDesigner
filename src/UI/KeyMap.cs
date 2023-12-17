using Microsoft.Extensions.Configuration;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Serializable settings class for user keybinding/accessibility tailoring.
/// </summary>
public class KeyMap
{
    /// <summary>
    /// Builds a configuration from the specified yaml file and returns a new instance of a corresponding <see cref="KeyMap"/>
    /// </summary>
    /// <param name="configurationFile">Relative or absolute path to the configuration file to load.</param>
    /// <returns>A new instance of a <see cref="KeyMap"/> from the specified file.</returns>
    /// <exception cref="ArgumentException">If <paramref name="configurationFile"/> is a null or empty string</exception>
    public static KeyMap LoadFromConfiguration( string configurationFile )
    {
        ArgumentException.ThrowIfNullOrEmpty( configurationFile, nameof( configurationFile ) );
        return new ConfigurationBuilder( ).AddYamlFile( configurationFile, false, false ).Build( ).Get<KeyMap>( )!;
    }

    /// <summary>
    /// Gets or Sets key for editing all <see cref="Property"/> of selected <see cref="Design"/>.
    /// </summary>
    public Key EditProperties { get; set; } = Key.F4;

    /// <summary>
    /// Gets or Sets the key to pop up the right click context menu.
    /// </summary>
    public Key ShowContextMenu { get; set; } = Key.Enter;

    /// <summary>
    /// Gets or Sets the key to pop up view specific operations (e.g. add column to <see cref="TableView"/>).
    /// </summary>
    public Key ViewSpecificOperations { get; set; } = Key.ShiftMask | Key.F4;

    /// <summary>
    /// Gets or Sets the key to edit the <see cref="Property"/> of the root view being edited.
    /// </summary>
    public Key EditRootProperties { get; set; } = Key.F5;

    /// <summary>
    /// Gets or Sets the key to show pop up help.
    /// </summary>
    public Key ShowHelp { get; set; } = Key.F1;

    /// <summary>
    /// Gets or Sets the key to create a new .Designer.cs and .cs file.
    /// </summary>
    public Key New { get; set; } = Key.CtrlMask | Key.N;

    /// <summary>
    /// Gets or Sets the key to open an existing .Designer.cs file.
    /// </summary>
    public Key Open { get; set; } = Key.CtrlMask | Key.O;

    /// <summary>
    /// Gets or Sets the key to save the current changes to disk.
    /// </summary>
    public Key Save { get; set; } = Key.CtrlMask | Key.S;

    /// <summary>
    /// Gets or Sets the key to <see cref="Operation.Redo"/> the last undone operation.
    /// </summary>
    public Key Redo { get; set; } = Key.CtrlMask | Key.Y;

    /// <summary>
    /// Gets or Sets the key to <see cref="Operation.Undo"/> the last performed operation.
    /// </summary>
    public Key Undo { get; set; } = Key.CtrlMask | Key.Z;

    /// <summary>
    /// Gets or Sets the key to delete the currently selected <see cref="Design"/>.
    /// </summary>
    public Key Delete { get; set; } = Key.DeleteChar;

    /// <summary>
    /// Gets or Sets the key to turn mouse dragging on/of.
    /// </summary>
    public Key ToggleDragging { get; set; } = Key.F3;

    /// <summary>
    /// Gets or Sets the key to add a new <see cref="View"/> (and its <see cref="Design"/> wrapper)
    /// to the currently focused container view (or root).
    /// </summary>
    public Key AddView { get; set; } = Key.F2;

    /// <summary>
    /// Gets or Sets the key to toggle showing an overlay with a description of the currently focused
    /// view(s).
    /// </summary>
    public Key ToggleShowFocused { get; set; } = Key.CtrlMask | Key.L;

    /// <summary>
    /// Gets or Sets the key to toggle showing dotted borders around views that otherwise do not have
    /// visible borders (e.g. <see cref="ScrollView"/>.
    /// </summary>
    public Key ToggleShowBorders { get; set; } = Key.CtrlMask | Key.B;

    /// <summary>
    /// Gets or Sets the mouse button that opens the right click context menu.
    /// Defaults to <see cref="MouseFlags.Button3Clicked"/> which is the right
    /// mouse button.
    /// </summary>
    public MouseFlags RightClick { get; set; } = MouseFlags.Button3Clicked;

    /// <summary>
    /// Gets or Sets the key to copy currently selected views.
    /// </summary>
    public Key Copy { get; set; } = Key.CtrlMask | Key.C;

    /// <summary>
    /// Gets or Sets the key to paste the last copied views.
    /// </summary>
    public Key Paste { get; set; } = Key.CtrlMask | Key.V;

    /// <summary>
    /// Gets or Sets the key to rename the field name (when written to .Designer.cs) of
    /// the currently selected <see cref="View"/> or <see cref="MenuItem"/>.
    /// </summary>
    public Key Rename { get; set; } = Key.CtrlMask | Key.R;

    /// <summary>
    /// Gets or Sets the key to assign a new shortcut to a <see cref="MenuItem"/>.
    /// </summary>
    public Key SetShortcut { get; set; } = Key.CtrlMask | Key.T;

    /// <summary>
    /// Gets or Sets the key to select all <see cref="Design"/> in the <see cref="Editor"/>.
    /// </summary>
    public Key SelectAll { get; set; } = Key.CtrlMask | Key.A;

    /// <summary>
    /// Gets or Sets the key to nudge the currently focused view right one unit.
    /// </summary>
    public Key MoveRight { get; set; } = Key.ShiftMask | Key.CursorRight;

    /// <summary>
    /// Gets or Sets the key to nudge the currently focused view left one unit.
    /// </summary>
    public Key MoveLeft { get; set; } = Key.ShiftMask | Key.CursorLeft;

    /// <summary>
    /// Gets or Sets the key to nudge the currently focused view up one unit.
    /// </summary>
    public Key MoveUp { get; set; } = Key.ShiftMask | Key.CursorUp;

    /// <summary>
    /// Gets or Sets the key to nudge the currently focused view down one unit.
    /// </summary>
    public Key MoveDown { get; set; } = Key.ShiftMask | Key.CursorDown;

    /// <summary>
    /// Gets or Sets the key to open the <see cref="ColorSchemesUI"/> window for
    /// creating/deleting <see cref="ColorScheme"/> that can be used in the <see cref="Editor"/>.
    /// </summary>
    public Key ShowColorSchemes { get; set; } = Key.F6;

    /// <summary>
    /// Gets or Sets a custom <see cref="ColorScheme"/> to apply to multi
    /// selections in designer.
    /// <remarks>Default color is green, this is useful if you have a heavily
    /// green theme where it could get confusing what is multi selected and what
    /// just has focus/uses your custom scheme</remarks>
    /// </summary>
    public ColorSchemeBlueprint SelectionColor { get; set; } = new
        ColorSchemeBlueprint
    {
        NormalForeground = Color.BrightGreen,
        NormalBackground = Color.Green,
        HotNormalForeground = Color.BrightGreen,
        HotNormalBackground = Color.Green,
        FocusForeground = Color.BrightYellow,
        FocusBackground = Color.Green,
        HotFocusForeground = Color.BrightYellow,
        HotFocusBackground = Color.Green,
        DisabledForeground = Color.BrightGreen,
        DisabledBackground = Color.Green,
    };
}
