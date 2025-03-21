#nullable disable
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Version of <see cref="ColorScheme"/> with setters, for use with <see cref="ColorSchemeEditor"/>
/// </summary>
class MutableColorScheme
{
    public Attribute Disabled { get; set; }
    public Attribute Focus { get; set; }
    public Attribute HotFocus { get; set; }
    public Attribute HotNormal { get; set; }
    public Attribute Normal { get; set; }

    internal ColorScheme ToColorScheme()
    {
        return new ColorScheme
        {
            Normal = Normal,
            HotNormal = HotNormal,
            Focus = Focus,
            HotFocus = HotFocus,
            Disabled = Disabled,
        };
    }
}