using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveMenuItemRightOperation : MenuItemOperation
{

    /// <summary>
    /// Set to insert at a specific index.  Leave null
    /// to simply move it to the bottom of the new submenu
    /// </summary>
    public int? InsertionIndex { get; internal set; }

    public MoveMenuItemRightOperation(MenuItem toMove): base(toMove)
    {
    }

    public override bool Do()
    {
        if(Parent == null || OperateOn == null)
            return false;
        
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
        
        if(InsertionIndex != null)
        {
            submenuChildren.Insert(InsertionIndex.Value,OperateOn);
        }
        else
        {
            submenuChildren.Add(OperateOn);
        }

        // update the main menu
        Parent.Children = children.ToArray();
        // update the submenu
        addTo.Children = submenuChildren.ToArray();

        Bar?.SetNeedsDisplay();

        return true;
    }

    public override void Redo()
    {
        // TODO
    }

    public override void Undo()
    {
        if(Parent == null || OperateOn == null)
            return;

        new MoveMenuItemLeftOperation(OperateOn).Do();
    }

    private MenuBarItem ConvertToMenuBarItem(List<MenuItem> children, int idx)
    {
        if(children[idx] is MenuBarItem mb)
            return mb;
        
        var added = new MenuBarItem(children[idx].Title,new MenuItem[0],null);
        added.Data = children[idx].Data;
        added.Shortcut = children[idx].Shortcut;

        children.RemoveAt(idx);
        children.Insert(idx,added);
        return added;
    }
}