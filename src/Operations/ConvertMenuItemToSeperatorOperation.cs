using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    internal class ConvertMenuItemToSeperatorOperation : MenuItemOperation
    {
        private int removedAtIdx;

        public ConvertMenuItemToSeperatorOperation(MenuItem toRemove) : base(toRemove)
        {
        }

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

        public override void Redo()
        {
            this.Do();
        }

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