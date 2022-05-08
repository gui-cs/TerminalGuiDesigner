using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveMenuItemRightOperation : MenuItemOperation
{
    public MoveMenuItemRightOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem toMove)
        : base(focusedView, bar, parent, toMove)
    {
    }

    public override bool Do()
    {
        
        // When user hits shift right
        var children = Parent.Children.ToList<MenuItem>();
        var currentItemIdx = children.IndexOf(OperateOn);
        var aboveIdx = currentItemIdx - 1;

        // and there is an item above
        if(aboveIdx < 0)
            return false;
        
        var addTo = ConvertToMenuBarItem(children,aboveIdx);

        // pull us out
        children.Remove(OperateOn);

        // add us to the submenu
        var submenuChildren = addTo.Children.ToList<MenuItem>();
        submenuChildren.Add(OperateOn);

        // update the main menu
        Parent.Children = children.ToArray();
        // update the submenu
        addTo.Children = submenuChildren.ToArray();
        
        FocusedView.SetNeedsDisplay();

        return true;
    }

    public override void Redo()
    {
        // TODO
    }

    public override void Undo()
    {
        // TODO
    }


    private MenuBarItem ConvertToMenuBarItem(List<MenuItem> children, int idx)
    {
        if(children[idx] is MenuBarItem mb)
            return mb;
        
        var added = new MenuBarItem(children[idx].Title,new MenuItem[0],null);

        children.RemoveAt(idx);
        children.Insert(idx,added);
        return added;
    }
}