using Terminal.Gui;

namespace TerminalGuiDesigner.UI;

public class KeyMap
{
    public Key EditProperties { get; set; } = Key.F4;

    public Key ShowContextMenu { get; set; } = Key.Enter;
    public Key ViewSpecificOperations { get; set; } = Key.ShiftMask | Key.F4;
    public Key EditRootProperties { get; set; } = Key.F5;
    public Key ShowHelp { get; set; } = Key.F1;
    public Key New { get; set; } = Key.CtrlMask | Key.N;
    public Key Open { get; set; } = Key.CtrlMask | Key.O;
    public Key Save { get; set; } = Key.CtrlMask | Key.S;
    public Key Redo { get; set; } = Key.CtrlMask | Key.Y;
    public Key Undo { get; set; } = Key.CtrlMask | Key.Z;
    public Key Delete { get; set; } = Key.DeleteChar;
    public Key ToggleDragging { get; set; } = Key.F3;
    public Key AddView { get; set; } = Key.F2;
    public Key ToggleShowFocused { get; set; } = Key.CtrlMask | Key.L;
    public Key ToggleShowBorders { get; set; } = Key.CtrlMask | Key.B;
    public MouseFlags RightClick { get; set; } = MouseFlags.Button3Clicked;
    public Key Copy { get; set; } = Key.CtrlMask | Key.C;
    public Key Paste { get; set; } = Key.CtrlMask | Key.V;
    public Key Rename { get; set; } = Key.CtrlMask | Key.R;
    public Key SetShortcut { get; set; } = Key.CtrlMask | Key.T;
    public Key SelectAll { get; set; } = Key.CtrlMask | Key.A;
    public Key MoveRight { get; set; } = Key.ShiftMask | Key.CursorRight;
    public Key MoveLeft { get; set; } = Key.ShiftMask | Key.CursorLeft;
    public Key MoveUp { get; set; } = Key.ShiftMask | Key.CursorUp;
    public Key MoveDown { get; set; } = Key.ShiftMask | Key.CursorDown;
    public Key ShowColorSchemes { get; set; } = Key.F6;

    /// <summary>
    /// Custom <see cref="ColorScheme"/> to apply to multi selections in designer.
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
        DisabledBackground = Color.Green
    };
}
