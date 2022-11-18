using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Abstract base class for <see cref="Operation"/> which operate on a single
    /// <see cref="StatusItem"/> in a <see cref="StatusBar"/>.
    /// </summary>
    public abstract class StatusItemOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusItemOperation"/> class.
        /// </summary>
        /// <param name="design">Wrapper for a <see cref="Terminal.Gui.StatusBar"/>.</param>
        /// <param name="item">The <see cref="StatusItem"/> to operate on.</param>
        public StatusItemOperation(Design design, StatusItem item)
        {
            this.Item = item;
            this.Design = design;
            this.Category = item.Title.ToString() ?? Operation.Unnamed;

            // somehow user ran this command for a non StatusBar
            if (this.Design.View is not Terminal.Gui.StatusBar sb)
            {
                throw new ArgumentException($"Design must be for a {nameof(Terminal.Gui.StatusBar)} to support {nameof(StatusItemOperation)}");
            }

            this.StatusBar = sb;

            if (!this.StatusBar.Items.Contains(item))
            {
                throw new ArgumentException($"Item provided does not belong to the {nameof(Terminal.Gui.StatusBar)}");
            }
        }

        /// <summary>
        /// Gets the wrapper for a <see cref="StatusBar"/> <see cref="View"/>.
        /// </summary>
        protected Design Design { get; }

        /// <summary>
        /// Gets the <see cref="StatusItem"/> to operate on.
        /// </summary>
        protected StatusItem Item { get; }

        /// <summary>
        /// Gets the <see cref="StatusBar"/> <see cref="View"/> wrapped by <see cref="Design"/>.
        /// </summary>
        protected StatusBar StatusBar { get; }
    }
}
