using Terminal.Gui;
using Terminal.Gui.ViewBase;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner;

/// <summary>
/// A user defined <see cref="Scheme"/> and its name as defined
/// by the user.  The <see cref="Name"/> will be used as Field name
/// in the class code generated so must not contain illegal characters/spaces.
/// </summary>
public class NamedScheme
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedScheme"/> class.
    /// </summary>
    /// <param name="name">Name to use for the <paramref name="scheme"/>.</param>
    /// <param name="scheme"><see cref="Scheme"/> to use.</param>
    public NamedScheme(string name, Scheme scheme)
    {
        this.Name = name;
        this.Scheme = scheme;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedScheme"/> class.
    /// </summary>
    /// <param name="name">Name to use for the <see cref="Scheme"/>.</param>
    public NamedScheme(string name)
    {
        this.Name = name;
        this.Scheme = new Scheme();
    }

    /// <summary>
    /// Gets or Sets a user supplied name.  This is the name that
    /// will be used for a private field member in the .Designer.cs file
    /// that is generated when writing out the <see cref="Scheme"/>
    /// (see <see cref="SchemeToCode"/>).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or Sets the Terminal.Gui <see cref="Scheme"/> which is to be
    /// known by <see cref="Name"/>.  A <see cref="Scheme"/> describes the
    /// <see cref="Color"/> that are to be used for a <see cref="View"/> when
    /// it is in various states (<see cref="Scheme.Normal"/>, <see cref="Scheme.Focus"/> etc).
    /// </summary>
    public Scheme Scheme { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.Name;
    }
}
