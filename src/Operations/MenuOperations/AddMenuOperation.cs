using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// <see cref="Operation"/> for adding a new top level menu to a <see cref="MenuBar"/> (e.g. File, Edit).
/// </summary>
public class AddMenuOperation : AddOperation<MenuBar, MenuBarItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddMenuOperation"/> class.
    /// Calling <see cref="Operation.Do"/> will add a new top level menu to the <see cref="MenuBar"/>
    /// wrapped by <paramref name="design"/>.
    /// </summary>
    /// <param name="design"><see cref="Design"/> wrapper for a view of Type <see cref="MenuBar"/>.</param>
    /// <param name="name">Optional explicit name to add with or null to prompt user interactively.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="design"/> is not wrapping a <see cref="MenuBar"/>.</exception>
    public AddMenuOperation(Design design, string? name)
        : base(
            (v) => v.Menus,
            (v, a) => v.Menus = a,
            (s) => s.Title.ToString() ?? "blank menu",
            (v, n) => new(n, new MenuItem[] { new() { Title = ViewFactory.DefaultMenuItemText } }),
            design,
            name)
    {
    }
}
