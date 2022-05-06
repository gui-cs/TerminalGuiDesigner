using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    public class MoveMenuItemOperation : MenuItemOperation
    {

        private bool _up;
        private List<MenuItem> _siblings;
        private int _currentItemIdx;

        public MoveMenuItemOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem toMove, bool up)
            : base(focusedView, bar, parent, toMove)
        {
            _up = up;
            
            _siblings = Parent.Children.ToList<MenuItem>();
            _currentItemIdx = _siblings.IndexOf(OperateOn);

            if(_currentItemIdx < 0)
            {
                IsImpossible = true;
            }
            else
            {
                IsImpossible = up ? _currentItemIdx == 0 : _currentItemIdx == _siblings.Count-1;
            }
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
            int moveTo = Math.Max(0, (amount) + _currentItemIdx);

            // pull it out from wherever it is
            _siblings.Remove(OperateOn);

            moveTo = Math.Min(moveTo, _siblings.Count);

            // push it in at the destination
            _siblings.Insert(moveTo,OperateOn);
            Parent.Children = _siblings.ToArray();

            FocusedView.SetNeedsDisplay();

            return true;            
        }
    }
}