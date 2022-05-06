using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    internal class MoveMenuItemOperation : MenuItemOperation
    {

        private bool _up;

        public MoveMenuItemOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem toMove, bool up)
            : base(focusedView, bar, parent, toMove)
        {
            _up = up;
        }

        public override bool Do()
        {
            return Move(_up ? -1:1);
        }
        public override void Redo()
        {
            Do();   
        }

        public override void Undo()
        {
            Move(_up ? 1:-1);
        }

        private bool Move(int amount)
        {
            
            var children = Parent.Children.ToList<MenuItem>();
            var currentItemIdx = children.IndexOf(OperateOn);

            // We are the parent but parents children don't contain
            // us.  Thats bad. TODO: log this
            if(currentItemIdx == -1)
                return false;

            int moveTo = Math.Max(0, (amount) + currentItemIdx);

            // pull it out from wherever it is
            children.Remove(OperateOn);

            moveTo = Math.Min(moveTo, children.Count);

            // push it in at the destination
            children.Insert(moveTo,OperateOn);
            Parent.Children = children.ToArray();

            FocusedView.SetNeedsDisplay();

            return true;            
        }
    }
}