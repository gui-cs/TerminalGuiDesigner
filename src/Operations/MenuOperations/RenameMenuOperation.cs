using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Renames a top level <see cref="MenuBarItem"/> on a <see cref="MenuBar"/> (File, Edit etc).
/// </summary>
public class RenameMenuOperation : RenameOperation<MenuBar, MenuBarItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameMenuOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="MenuBar"/> upon which you wish to operate.</param>
    /// <param name="toRename">The <see cref="MenuBarItem"/> to rename.</param>
    /// <param name="newName">The new name to use.</param>
    public RenameMenuOperation(Design design, MenuBarItem toRename, string? newName)
        : base(
            v => v.Menus,
            (v, a) => v.Menus = a,
            s => s.Title.ToString() ?? "blank menu",
            (v, s) => v.Title = s,
            design,
            toRename,
            newName)
    {
    }
}
