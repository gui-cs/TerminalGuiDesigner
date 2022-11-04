using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveMenuItemRightOperation : MenuItemOperation
{
    /// <summary>
    /// Set to insert at a specific index.  Leave null
    /// to simply move it to the bottom of the new submenu
    /// </summary>
    public int? InsertionIndex { get; internal set; }

    public MoveMenuItemRightOperation(MenuItem toMove)
        : base(toMove)
    {
    }

    public override bool Do()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return false;
        }

        // When user hits shift right
        var children = this.Parent.Children.ToList<MenuItem>();
        var currentItemIdx = children.IndexOf(this.OperateOn);
        var aboveIdx = currentItemIdx - 1;

        // and there is an item above
        if (aboveIdx < 0)
        {
            return false;
        }

        var addTo = this.ConvertToMenuBarItem(children, aboveIdx);

        // pull us out
        children.Remove(this.OperateOn);

        // add us to the submenu
        var submenuChildren = addTo.Children.ToList<MenuItem>();

        if (this.InsertionIndex != null)
        {
            submenuChildren.Insert(this.InsertionIndex.Value, this.OperateOn);
        }
        else
        {
            submenuChildren.Add(this.OperateOn);
        }

        // update the main menu
        this.Parent.Children = children.ToArray();
        // update the submenu
        addTo.Children = submenuChildren.ToArray();

        this.Bar?.SetNeedsDisplay();

        return true;
    }

    public override void Redo()
    {
        // TODO
    }

    public override void Undo()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return;
        }

        new MoveMenuItemLeftOperation(this.OperateOn).Do();
    }

    private MenuBarItem ConvertToMenuBarItem(List<MenuItem> children, int idx)
    {
        if (children[idx] is MenuBarItem mb)
        {
            return mb;
        }

        var added = new MenuBarItem(children[idx].Title, new MenuItem[0], null);
        added.Data = children[idx].Data;
        added.Shortcut = children[idx].Shortcut;

        children.RemoveAt(idx);
        children.Insert(idx, added);
        return added;
    }
}