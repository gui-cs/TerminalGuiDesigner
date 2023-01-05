using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Removes a top level menu from a <see cref="MenuBar"/> e.g. File, Edit.
/// For removing items under a menu (e.g. Open/Save) see <see cref="RemoveMenuItemOperation"/>
/// instead.
/// </summary>
public class RemoveMenuOperation : RemoveOperation<MenuBar, MenuBarItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveMenuOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="MenuBar"/> upon which you wish to operate.</param>
    /// <param name="toRemove">The <see cref="MenuBarItem"/> to remove.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="MenuBar"/>.</exception>
    public RemoveMenuOperation(Design design, MenuBarItem toRemove)
        : base(
            v => v.Menus,
            (v, a) => v.Menus = a,
            s => s.Title.ToString() ?? "blank menu",
            design,
            toRemove)
    {
    }
}