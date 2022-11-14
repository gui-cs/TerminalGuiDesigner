using System.Collections.ObjectModel;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace TerminalGuiDesigner;

/// <summary>
/// A user defined <see cref="ColorScheme"/> and its name as defined
/// by the user.  The <see cref="Name"/> will be used as Field name
/// in the class code generated so must not contain illegal characters/spaces
/// </summary>
public class NamedColorScheme
{
    public string Name { get; set; }

    public ColorScheme Scheme { get; set; }

    public NamedColorScheme(string name, ColorScheme scheme)
    {
        this.Name = name;
        this.Scheme = scheme;
    }

    public NamedColorScheme(string name)
    {
        this.Name = name;
        this.Scheme = new ColorScheme();
    }

    public override string ToString()
    {
        return this.Name;
    }
}
