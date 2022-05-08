using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.Operations
{
    internal class RemoveMenuItemOperation : MenuItemOperation
    {
        private int _removedAtIdx;

        public RemoveMenuItemOperation(MenuItem toRemove): base(toRemove)
        {
        }
        

        public override bool Do()
        {
            var children = Parent.Children.ToList<MenuItem>();
                
            _removedAtIdx = Math.Max(0,children.IndexOf(OperateOn));
            
            children.Remove(OperateOn);
            Parent.Children = children.ToArray();
            Bar?.SetNeedsDisplay();

            MenuTracker.Instance.ConvertEmptyMenus();
                
            return true;
        }

        public override void Redo()
        {
            Do();
        }

        public override void Undo()
        {
            var children = Parent.Children.ToList<MenuItem>();
                
            children.Insert(_removedAtIdx, OperateOn);
            Parent.Children = children.ToArray();
            Bar?.SetNeedsDisplay();
        }
    }
}