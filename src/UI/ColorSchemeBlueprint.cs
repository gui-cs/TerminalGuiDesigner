using Terminal.Gui;
using YamlDotNet.Serialization;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI
{
    public class ColorSchemeBlueprint
    {
        public Color NormalForeground { get; set; }
        public Color NormalBackground { get; set; }
        public Color HotNormalForeground { get; set; }
        public Color HotNormalBackground { get; set; }
        public Color FocusForeground { get; set; }
        public Color FocusBackground { get; set; }
        public Color HotFocusForeground { get; set; }
        public Color HotFocusBackground { get; set; }
        public Color DisabledForeground { get; set; }
        public Color DisabledBackground { get; set; }

        [YamlIgnore]
        public ColorScheme Scheme => new ColorScheme
        {
            Normal = new Attribute(NormalForeground, NormalBackground),
            HotNormal = new Attribute(HotNormalForeground, HotNormalBackground),
            Focus = new Attribute(FocusForeground, FocusBackground),
            HotFocus = new Attribute(HotFocusForeground, HotFocusBackground),
            Disabled = new Attribute(DisabledForeground, DisabledBackground),
        };
    }
}