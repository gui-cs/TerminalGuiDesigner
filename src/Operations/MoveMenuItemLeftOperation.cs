using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveMenuItemLeftOperation : MenuItemOperation
{
    public MoveMenuItemLeftOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem toMove)
        : base(focusedView, bar, parent, toMove)
    {
        // TODO prevent this if a root menu item
    }

    public override bool Do()
    {
        var parentsParent = MenuTracker.Instance.GetParent(Parent, out var bar);
        
        if(parentsParent == null)
            return false;

        // remove us
        if(new RemoveMenuItemOperation(FocusedView,Bar,Parent,OperateOn).Do())
        {
            // if that worked then add us to the root
            var children = parentsParent.Children.ToList<MenuItem>();
            var parentsIdx = children.IndexOf(Parent);

            // We are the parent but parents children don't contain
            // us.  Thats bad. TODO: log this
            if(parentsIdx == -1)
                return false;

            int insertAt = Math.Max(0,parentsIdx + 1);

            children.Insert(insertAt, OperateOn);
            parentsParent.Children = children.ToArray();
            
            FocusedView.SetNeedsDisplay();
            
            return true;
        }

        return false;
        
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        new MoveMenuItemRightOperation(FocusedView,Bar,Parent,OperateOn)
        .Do();
    }

}
