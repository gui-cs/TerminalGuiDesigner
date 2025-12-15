using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Renames a <see cref="Shortcut"/> on a <see cref="StatusBar"/>.
    /// </summary>
    public class RenameStatusItemOperation : RenameOperation<StatusBar, Shortcut>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenameStatusItemOperation"/> class.
        /// </summary>
        /// <param name="app">The application instance.</param>
        /// <param name="design">Design wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="toRename">The <see cref="Shortcut"/> to rename.</param>
        /// <param name="newName">The new name to use or null to prompt user.</param>
        public RenameStatusItemOperation(IApplication app, Design design, Shortcut toRename, string? newName)
            : base(
                  app,
                  (d) => d.GetShortcuts(),
                  (d, v) => d.SetShortcuts(v),
                  (v) => v.Title.ToString() ?? Operation.Unnamed,
                  (v, n) => v.Title = n,
                  design,
                  toRename,
                  newName)
        {
        }
    }
}
