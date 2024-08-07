using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Operation for adding a new <see cref="Shortcut"/> to a <see cref="StatusBar"/>.
    /// </summary>
    public class AddStatusItemOperation : AddOperation<StatusBar, Shortcut>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddStatusItemOperation"/> class.
        /// </summary>
        /// <param name="design">Wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="name">Name for the new item created or null to prompt user.</param>
        public AddStatusItemOperation(Design design, string? name)
            : base(
                  (d) => d.GetShortcuts(),
                  (d, v) => d.SetShortcuts(v),
                  (v) => v.Title.ToString() ?? Operation.Unnamed,
                  (d, name) => { return new Shortcut(KeyCode.Null, name, null); },
                  design,
                  name)
        {
        }
    }
}
