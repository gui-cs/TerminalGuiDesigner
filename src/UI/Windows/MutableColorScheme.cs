#nullable disable
using Terminal.Gui;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Version of <see cref="Scheme"/> with setters, for use with <see cref="SchemeEditor"/>
/// </summary>
class MutableScheme
{
    public Attribute Disabled { get; set; }
    public Attribute Focus { get; set; }
    public Attribute HotFocus { get; set; }
    public Attribute HotNormal { get; set; }
    public Attribute Normal { get; set; }

    internal Scheme ToScheme()
    {
        return new Scheme
        {
            Normal = Normal,
            HotNormal = HotNormal,
            Focus = Focus,
            HotFocus = HotFocus,
            Disabled = Disabled,
        };
    }
}