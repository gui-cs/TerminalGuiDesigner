namespace TerminalGuiDesigner;

/// <summary>
/// Provides some out of the box nicely set up color schemes
/// users can select from when they haven't created any of their
/// own yet.
/// </summary>
public class DefaultColorSchemes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultColorSchemes"/> class.
    /// Creates a new instance and sets the <see cref="NamedColorScheme"/> defaults
    /// based on the current driver.
    /// </summary>
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

        this.GrayOnBlack = new NamedColorScheme("greyOnBlack");
        this.GrayOnBlack.Scheme.Normal = new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black);
        this.GrayOnBlack.Scheme.HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black);
        this.GrayOnBlack.Scheme.Focus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Black, Terminal.Gui.Color.DarkGray);
        this.GrayOnBlack.Scheme.HotFocus = new Terminal.Gui.Attribute(Terminal.Gui.Color.Black, Terminal.Gui.Color.DarkGray);
        this.GrayOnBlack.Scheme.Disabled = new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black);
    }

    /// <summary>
    /// Gets a default color scheme provided out of the box as an example.
    /// Red foreground on black background.
    /// </summary>
    public NamedColorScheme RedOnBlack { get; }

    /// <summary>
    /// Gets a  default color scheme provided out of the box as an example.
    /// Green foreground on black background.
    /// </summary>
    public NamedColorScheme GreenOnBlack { get; }

    /// <summary>
    /// Gets a  default color scheme provided out of the box as an example.
    /// Blue foreground on black background.
    /// </summary>
    public NamedColorScheme BlueOnBlack { get; }

    /// <summary>
    /// Gets a  default color scheme provided out of the box as an example.
    /// Gray foreground on black background.
    /// </summary>
    public NamedColorScheme GrayOnBlack { get; }

    /// <summary>
    /// Returns all default color schemes.
    /// </summary>
    /// <returns>All default color schemes configured.</returns>
    public IEnumerable<NamedColorScheme> GetDefaultSchemes()
    {
        return typeof(DefaultColorSchemes)
            .GetProperties().Where(p => p.PropertyType == typeof(NamedColorScheme))
            .Select(p => p.GetValue(this))
            .Cast<NamedColorScheme>();
    }

    /// <summary>
    /// Returns a specific <see cref="NamedColorScheme"/>.
    /// </summary>
    /// <remarks>These are camel case because they will ultimately end up as private fields in a .Designer.cs file (if used).</remarks>
    /// <param name="name">Name of default e.g. greenOnBlack.</param>
    /// <returns>The named scheme.</returns>
    public NamedColorScheme GetDefaultScheme(string name)
    {
        return this.GetDefaultSchemes().First(s => s.Name.Equals(name));
    }
}
