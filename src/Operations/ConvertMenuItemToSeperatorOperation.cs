using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    /// <summary>
    /// <para>
    /// Converts a <see cref="MenuItem"/> into a Separator (horizontal line in menu).
    /// In Terminal.Gui this is represented as a null in the <see cref="MenuBar"/>.
    /// </para>
    /// </summary>
    public class ConvertMenuItemToSeperatorOperation : MenuItemOperation
    {
        private int removedAtIdx;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertMenuItemToSeperatorOperation"/> class.
        /// </summary>
        /// <param name="toConvert">A <see cref="MenuItem"/> to replace with a separator (null) in it's parent <see cref="MenuBar"/>.</param>
        public ConvertMenuItemToSeperatorOperation(MenuItem toConvert)
            : base(toConvert)
        {
        }

        /// <inheritdoc/>
        public override bool Do()
        {
            if (this.Parent == null || this.OperateOn == null)
            {
                return false;
            }

            var children = this.Parent.Children.ToList<MenuItem?>();

            this.removedAtIdx = Math.Max(0, children.IndexOf(this.OperateOn));
            children[this.removedAtIdx] = null;

            this.Parent.Children = children.ToArray();
            this.Bar?.SetNeedsDisplay();

            return true;
        }

        /// <inheritdoc/>
        public override void Redo()
        {
            this.Do();
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            if (this.Parent == null || this.OperateOn == null)
            {
                return;
            }

            var children = this.Parent.Children.ToList<MenuItem>();

            children[this.removedAtIdx] = this.OperateOn;
            this.Parent.Children = children.ToArray();
            this.Bar?.SetNeedsDisplay();
        }
    }
}