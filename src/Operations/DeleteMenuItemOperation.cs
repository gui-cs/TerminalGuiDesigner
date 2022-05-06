using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.Operations
{
    internal class DeleteMenuItemOperation : MenuItemOperation
    {
        private int _removedAtIdx;

        public DeleteMenuItemOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem toRemove)
            : base(focusedView, bar, parent, toRemove)
        {
        }
        

        public override bool Do()
        {
            var children = Parent.Children.ToList<MenuItem>();
                
            _removedAtIdx = Math.Max(0,children.IndexOf(OperateOn));
            
            children.Remove(OperateOn);
            Parent.Children = children.ToArray();
            FocusedView.SetNeedsDisplay();

            if(!children.Any())
            {
                Bar?.CloseMenu();
            }
                
            return children.Any();
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
            FocusedView.SetNeedsDisplay();
        }
    }
}