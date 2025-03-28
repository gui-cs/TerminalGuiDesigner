using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Changes the <see cref="Shortcut"/> of a <see cref="Shortcut"/> on
    /// a <see cref="StatusBar"/>.
    /// </summary>
    public class SetShortcutOperation : GenericArrayElementOperation<StatusBar, Shortcut>
    {
        private Key originalShortcut;
        private Key shortcut;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetShortcutOperation"/> class.
        /// </summary>
        /// <param name="design">Wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="statusItem">The <see cref="Shortcut"/> whose shortcut you want to change.</param>
        /// <param name="shortcut">The new shortcut or null to prompt user at runtime.</param>
        public SetShortcutOperation(Design design, Shortcut statusItem, Key? shortcut)
            : base(
                  (v) => v.GetShortcuts(),
                  (v, a) => v.SetShortcuts(a),
                  (e) => e.Title?.ToString() ?? Operation.Unnamed,
                  design,
                  statusItem)
        {
            this.shortcut = shortcut ?? Key.Empty;
            this.originalShortcut = statusItem.Key;
        }

        /// <inheritdoc/>
        protected override void RedoImpl()
        {
            if (this.shortcut == Key.Empty)
            {
                return;
            }

            this.OperateOn.Key = this.shortcut;
        }

        /// <inheritdoc/>
        protected override void UndoImpl()
        {
            this.OperateOn.Key = this.originalShortcut;
        }

        /// <inheritdoc/>
        protected override bool DoImpl()
        {
            if (this.shortcut == Key.Empty)
            {
                this.shortcut = Modals.GetShortcut();
            }

            this.OperateOn.Key = this.shortcut;
            return true;
        }
    }
}
