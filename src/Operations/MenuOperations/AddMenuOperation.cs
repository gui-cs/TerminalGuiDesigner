using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// <see cref="Operation"/> for adding a new top level menu to a <see cref="MenuBar"/> (e.g. File, Edit).
/// </summary>
public class AddMenuOperation : AddOperation<MenuBar, MenuBarItem>
{
    /// <summary>
    /// <para>
    /// <see cref="AddMenuOperation"/> adds a new top level menu (e.g. File, Edit etc).  In the designer
    /// all menus must have at least 1 <see cref="MenuItem"/> under them so it will be
    /// created with a single <see cref="MenuItem"/> in it already.  That item will
    /// bear this text.
    /// </para>
    /// <para>
    /// This string should be used by any other areas of code that want to create new <see cref="MenuItem"/> under
    /// a top/sub menu (e.g. <see cref="ViewFactory"/>).
    /// </para>
    /// </summary>
    public const string DefaultMenuItemText = "Edit Me";

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
            v => v.Menus,
            (v, a) => v.Menus = a,
            s => s.Title.ToString() ?? "blank menu",
            n => { return new MenuBarItem(n, new MenuItem[] { new MenuItem { Title = DefaultMenuItemText } }); },
            design,
            name)
    {
    }
}
