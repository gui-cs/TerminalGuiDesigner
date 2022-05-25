using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    internal class ConvertMenuItemToSeperatorOperation : MenuItemOperation
    {
        private int _removedAtIdx;

        public ConvertMenuItemToSeperatorOperation(MenuItem toRemove) : base(toRemove)
        {
        }

        public override bool Do()
        {
            if (Parent == null || OperateOn == null)
                return false;

            var children = Parent.Children.ToList<MenuItem?>();

            _removedAtIdx = Math.Max(0, children.IndexOf(OperateOn));
            children[_removedAtIdx] = null;

            Parent.Children = children.ToArray();
            Bar?.SetNeedsDisplay();

            return true;
        }

        public override void Redo()
        {
            Do();
        }

        public override void Undo()
        {
            if (Parent == null || OperateOn == null)
                return;

            var children = Parent.Children.ToList<MenuItem>();

            children[_removedAtIdx] = OperateOn;
            Parent.Children = children.ToArray();
            Bar?.SetNeedsDisplay();
        }
    }
}