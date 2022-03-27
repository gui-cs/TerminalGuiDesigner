using Terminal.Gui;

namespace TerminalGuiDesigner.UI;

public class KeyMap
{
    public Key EditProperties {get;set;} = Key.F4;
    public Key ViewSpecificOperations {get;set;} = Key.ShiftMask | Key.F4;
    public Key EditRootProperties { get; internal set; } = Key.F5;
    public Key ShowHelp { get; internal set; } = Key.CtrlMask | Key.H;
    public Key New { get; internal set; } = Key.CtrlMask | Key.N;
    public Key Open { get; internal set; } = Key.CtrlMask | Key.O;
    public Key Save { get; internal set; } = Key.CtrlMask | Key.S;
}
