using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.Operations
{
    internal class RemoveMenuItemOperation : MenuItemOperation
    {
        private int _removedAtIdx;

        public RemoveMenuItemOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem toRemove)
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

            ConvertMenuBarItemToRegularItemIfEmpty(Parent);

            if(!children.Any())
            {
                RefreshMenus();   
            }
                
            return true;
        }


        internal void RefreshMenus()
        {
            if(Bar != null && Bar.IsMenuOpen)
            {
                Bar.CloseMenu();
                Bar.OpenMenu();
            }
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

        private void ConvertMenuBarItemToRegularItemIfEmpty(MenuBarItem bar)
        {
            // bar still has more children so don't convert
            if(bar.Children.Any())
                return;

            var parent = MenuTracker.Instance.GetParent(bar,out _);

            if(parent == null)
                return;

            var children = parent.Children.ToList<MenuItem>();
            var idx = children.IndexOf(parent);

            if(idx < 0)
                return;
            
            // bar has no children so convert to MenuItem
            var added = new MenuItem {Title = parent.Title};

            children.RemoveAt(idx);
            children.Insert(idx,added);

            parent.Children = children.ToArray();
        }
    }
}