using Terminal.Gui;

namespace TerminalGuiDesigner
{
    /// <summary>
    /// Provides some out of the box nicely set up color schemes
    /// users can select from when they haven't created any of their
    /// own yet.
    /// </summary>
    public class DefaultColorSchemes
    {
        public NamedColorScheme RedOnBlack;

        public NamedColorScheme GreenOnBlack;

        public NamedColorScheme BlueOnBlack;

        public DefaultColorSchemes()
        {
            this.RedOnBlack = new NamedColorScheme("redOnBlack");
            this.RedOnBlack.Scheme.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Red, Terminal.Gui.Color.Black);
            this.RedOnBlack.Scheme.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Black);
            this.RedOnBlack.Scheme.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Red, Terminal.Gui.Color.Brown);
            this.RedOnBlack.Scheme.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Brown);
            this.RedOnBlack.Scheme.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black);
            
            this.GreenOnBlack = new NamedColorScheme("greenOnBlack");
            this.GreenOnBlack.Scheme.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Green, Terminal.Gui.Color.Black);
            this.GreenOnBlack.Scheme.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightGreen, Terminal.Gui.Color.Black);
            this.GreenOnBlack.Scheme.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Green, Terminal.Gui.Color.Magenta);
            this.GreenOnBlack.Scheme.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightGreen, Terminal.Gui.Color.Magenta);
            this.GreenOnBlack.Scheme.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black);

            this.BlueOnBlack = new NamedColorScheme("blueOnBlack");
            this.BlueOnBlack.Scheme.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.Black);
            this.BlueOnBlack.Scheme.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.Black);
            this.BlueOnBlack.Scheme.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.BrightYellow);
            this.BlueOnBlack.Scheme.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.BrightYellow);
            this.BlueOnBlack.Scheme.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black);
        }

        public IEnumerable<NamedColorScheme> GetDefaultSchemes()
        {
            return typeof(DefaultColorSchemes)
                .GetFields().Where(f => f.FieldType == typeof(NamedColorScheme))
                .Select(f=>f.GetValue(this))
                .Cast<NamedColorScheme>();
        }
    }
}
