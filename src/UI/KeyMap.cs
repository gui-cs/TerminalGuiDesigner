using Terminal.Gui;

namespace TerminalGuiDesigner.UI;

public class KeyMap
{
    public Key EditProperties {get;set;} = Key.F4;

    public Key ShowContextMenu {get;set;} = Key.Enter;
    public Key ViewSpecificOperations {get;set;} = Key.ShiftMask | Key.F4;
    public Key EditRootProperties { get; set; } = Key.F5;
    public Key ShowHelp { get; set; } = Key.CtrlMask | Key.E;
    public Key New { get; set; } = Key.CtrlMask | Key.N;
    public Key Open { get; set; } = Key.CtrlMask | Key.O;
    public Key Save { get; set; } = Key.CtrlMask | Key.S;
    public Key Redo { get; set; } = Key.CtrlMask | Key.Y;
    public Key Undo { get; set; } = Key.CtrlMask | Key.Z;
    public Key Delete { get; set; } = Key.DeleteChar;
    public Key ToggleDragging { get; set; } = Key.F3;
    public Key AddView { get; set; } = Key.F2;
    public Key ToggleShowFocused { get; set; } = Key.CtrlMask | Key.L;
    public MouseFlags RightClick { get; set; } = MouseFlags.Button3Clicked;
    public Key Copy { get; set; } = Key.CtrlMask | Key.C;
    public Key Paste { get; set; } = Key.CtrlMask | Key.V;
    public Key Rename {get; set;} = Key.CtrlMask | Key.R;
    public Key SetShortcut {get; set;} = Key.CtrlMask | Key.T;
}
