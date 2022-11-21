using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    public class SetShortcutOperation : GenericArrayElementOperation<StatusBar, StatusItem>
    {
        private Key originalShortcut;
        private Key? shortcut;

        public SetShortcutOperation(Design design, StatusItem statusItem, Key? shortcut)
            : base(
                  (v) => v.Items,
                  (v, a) => v.Items = a,
                  (e) => e.Title?.ToString() ?? Operation.Unnamed,
                  design,
                  statusItem)
        {
            this.shortcut = shortcut;
            this.originalShortcut = statusItem.Shortcut;
        }

        /// <inheritdoc/>
        public override void Redo()
        {
            if (this.shortcut == null)
            {
                return;
            }

            this.OperateOn.SetShortcut(this.shortcut.Value);
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            this.OperateOn.SetShortcut(this.originalShortcut);
        }

        /// <inheritdoc/>
        protected override bool DoImpl()
        {
            if (this.shortcut == null)
            {
                this.shortcut = Modals.GetShortcut();
            }

            this.OperateOn.SetShortcut(this.shortcut.Value);
            return true;
        }
    }
}
