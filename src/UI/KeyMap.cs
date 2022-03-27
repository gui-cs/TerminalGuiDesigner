using Terminal.Gui;

namespace TerminalGuiDesigner.UI;

public class KeyMap
{
    public Key EditProperties {get;set;} = Key.F4;
    public Key ViewSpecificOperations {get;set;} = Key.ShiftMask | Key.F4;
    public Key EditRootProperties { get; internal set; } = Key.F5;
}
