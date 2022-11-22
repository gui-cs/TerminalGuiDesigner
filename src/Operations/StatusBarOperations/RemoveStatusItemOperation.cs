using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Removes a <see cref="StatusItem"/> from a <see cref="StatusBar"/>.
    /// </summary>
    public class RemoveStatusItemOperation : RemoveOperation<StatusBar, StatusItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveStatusItemOperation"/> class.
        /// </summary>
        /// <param name="design">Wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="toRemove">A <see cref="StatusItem"/> to remove from bar.</param>
        public RemoveStatusItemOperation(Design design, StatusItem toRemove)
            : base(
                  (v) => v.Items,
                  (v, a) => v.Items = a,
                  (e) => e.Title?.ToString() ?? Operation.Unnamed,
                  design,
                  toRemove)
        {
        }
    }
}
