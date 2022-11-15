using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner;

/// <summary>
/// A user defined <see cref="ColorScheme"/> and its name as defined
/// by the user.  The <see cref="Name"/> will be used as Field name
/// in the class code generated so must not contain illegal characters/spaces.
/// </summary>
public class NamedColorScheme
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedColorScheme"/> class.
    /// </summary>
    /// <param name="name">Name to use for the <paramref name="scheme"/>.</param>
    /// <param name="scheme"><see cref="ColorScheme"/> to use.</param>
    public NamedColorScheme(string name, ColorScheme scheme)
    {
        this.Name = name;
        this.Scheme = scheme;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedColorScheme"/> class.
    /// </summary>
    /// <param name="name">Name to use for the <see cref="Scheme"/>.</param>
    public NamedColorScheme(string name)
    {
        this.Name = name;
        this.Scheme = new ColorScheme();
    }

    /// <summary>
    /// Gets or Sets a user supplied name.  This is the name that
    /// will be used for a private field member in the .Designer.cs file
    /// that is generated when writing out the <see cref="Scheme"/>
    /// (see <see cref="ColorSchemeToCode"/>).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or Sets the Terminal.Gui <see cref="ColorScheme"/> which is to be
    /// known by <see cref="Name"/>.  A <see cref="ColorScheme"/> describes the
    /// <see cref="Color"/> that are to be used for a <see cref="View"/> when
    /// it is in various states (<see cref="ColorScheme.Normal"/>, <see cref="ColorScheme.Focus"/> etc).
    /// </summary>
    public ColorScheme Scheme { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.Name;
    }
}
