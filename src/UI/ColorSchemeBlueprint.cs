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
            Normal = new Attribute(this.NormalForeground, this.NormalBackground),
            HotNormal = new Attribute(this.HotNormalForeground, this.HotNormalBackground),
            Focus = new Attribute(this.FocusForeground, this.FocusBackground),
            HotFocus = new Attribute(this.HotFocusForeground, this.HotFocusBackground),
            Disabled = new Attribute(this.DisabledForeground, this.DisabledBackground),
        };
    }
}