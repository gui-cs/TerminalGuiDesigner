using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Moves a top level <see cref="MenuBarItem"/> (File, Edit etc) left or right within the ordering
/// of menus in a <see cref="MenuBar"/>.
/// </summary>
public class MoveMenuOperation : MoveOperation<MenuBar, MenuBarItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoveMenuOperation"/> class.
    /// Creates an operation that will change the ordering of top level menus within
    /// a <see cref="MenuBar"/>.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="design">Wrapper for a <see cref="MenuBar"/>.</param>
    /// <param name="toMove">The top level menu to move.</param>
    /// <param name="adjustment">Negative to move menu left, positive to move menu right.</param>
    public MoveMenuOperation(IApplication app, Design design, MenuBarItem toMove, int adjustment)
        : base(
            app,
            v => v.SubViews.OfType<MenuBarItem>().ToArray(),
            (v, a) => v.Menus = a,
            s => s.Title.ToString() ?? "blank menu",
            design,
            toMove,
            adjustment)
    {
    }
}
