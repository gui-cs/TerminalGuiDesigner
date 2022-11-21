using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Renames a <see cref="StatusItem"/> on a <see cref="StatusBar"/>.
    /// </summary>
    public class RenameStatusItemOperation : RenameOperation<StatusBar, StatusItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenameStatusItemOperation"/> class.
        /// </summary>
        /// <param name="design">Design wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="toRename">The <see cref="StatusItem"/> to rename.</param>
        /// <param name="newName">The new name to use or null to prompt user.</param>
        public RenameStatusItemOperation(Design design, StatusItem toRename, string? newName)
            : base(
                  (d) => d.Items,
                  (d, v) => d.Items = v,
                  (v) => v.Title.ToString() ?? Operation.Unnamed,
                  (v, n) => v.Title = n,
                  design,
                  toRename,
                  newName)
        {
        }
    }
}
