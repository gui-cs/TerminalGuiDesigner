using Terminal.Gui;

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
        this.RedOnBlack = new NamedColorScheme("redOnBlack",
            new ColorScheme(
        normal: new Terminal.Gui.Attribute(Terminal.Gui.Color.Red, Terminal.Gui.Color.Black),
        hotNormal: new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Black),
        focus: new Terminal.Gui.Attribute(Terminal.Gui.Color.Red, Terminal.Gui.Color.Yellow),
        hotFocus: new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Yellow),
        disabled: new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black)
            ));

        this.GreenOnBlack = new NamedColorScheme("greenOnBlack",
            new ColorScheme(
        normal : new Terminal.Gui.Attribute(Terminal.Gui.Color.Green, Terminal.Gui.Color.Black),
        hotNormal: new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightGreen, Terminal.Gui.Color.Black),
        focus: new Terminal.Gui.Attribute(Terminal.Gui.Color.Green, Terminal.Gui.Color.Magenta),
        hotFocus: new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightGreen, Terminal.Gui.Color.Magenta),
        disabled : new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black)));

        this.BlueOnBlack = new NamedColorScheme("blueOnBlack",
            new ColorScheme(
        normal : new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.Black),
        hotNormal: new Terminal.Gui.Attribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.Black),
        focus: new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.BrightYellow),
        hotFocus: new Terminal.Gui.Attribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.BrightYellow),
        disabled: new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black)));

        this.GrayOnBlack = new NamedColorScheme("greyOnBlack",
            new ColorScheme(
        normal : new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black),
        hotNormal: new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black),
        focus: new Terminal.Gui.Attribute(Terminal.Gui.Color.Black, Terminal.Gui.Color.DarkGray),
        hotFocus: new Terminal.Gui.Attribute(Terminal.Gui.Color.Black, Terminal.Gui.Color.DarkGray),
        disabled: new Terminal.Gui.Attribute(Terminal.Gui.Color.DarkGray, Terminal.Gui.Color.Black)));

        this.TerminalGuiDefault = new NamedColorScheme("tgDefault",
            new ColorScheme(
        normal : new Terminal.Gui.Attribute(Color.White, Color.Blue),
        hotNormal : new Terminal.Gui.Attribute(Color.BrightCyan, Color.Blue),
        focus : new Terminal.Gui.Attribute(Color.Black, Color.Gray),
        hotFocus : new Terminal.Gui.Attribute(Color.BrightBlue, Color.Gray),

        // HACK : Keeping this foreground as Brown because otherwise designer will think this is legit
        // the real default and assume user has not chosen it.  See: https://github.com/gui-cs/TerminalGuiDesigner/issues/133
        disabled: new Terminal.Gui.Attribute(Color.Yellow, Color.Blue)));
    }

    /// <summary>
    /// Gets a default color scheme provided out of the box as an example.
    /// Red foreground on black background.
    /// </summary>
    public NamedColorScheme RedOnBlack { get; }

    /// <summary>
    /// Gets a default color scheme provided out of the box as an example.
    /// This scheme is based on the normal blue/white scheme of Terminal.gui.
    /// </summary>
    public NamedColorScheme TerminalGuiDefault { get; }

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
