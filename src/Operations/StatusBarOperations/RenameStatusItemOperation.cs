using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Renames a <see cref="StatusItem"/> on a <see cref="StatusBar"/>.
    /// </summary>
    public class RenameStatusItemOperation : StatusItemOperation
    {
        private string? originalName;
        private string? newName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameStatusItemOperation"/> class.
        /// </summary>
        /// <param name="design">Design wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="toRename">The <see cref="StatusItem"/> to rename.</param>
        public RenameStatusItemOperation(Design design, StatusItem toRename)
            : base(design, toRename)
        {
            this.originalName = toRename.Title.ToString() ?? Operation.Unnamed;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Rename '{this.originalName}'";
        }

        /// <inheritdoc/>
        public override void Redo()
        {
            this.Item.Title = this.newName;
            this.StatusBar.SetNeedsDisplay();
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            this.Item.Title = this.originalName;
            this.StatusBar.SetNeedsDisplay();
        }

        /// <inheritdoc/>
        protected override bool DoImpl()
        {
            if (Modals.GetString("Rename StatusItem", "Column Name", this.originalName, out var newName))
            {
                this.newName = newName;
                this.Item.Title = newName;
                this.StatusBar.SetNeedsDisplay();
                return true;
            }

            return false;
        }
    }
}
