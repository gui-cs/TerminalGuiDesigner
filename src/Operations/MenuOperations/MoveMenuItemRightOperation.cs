using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Moves a <see cref="MenuItem"/> into a sub-menu of the <see cref="MenuItem"/>
/// above it.  If it is the first item to be moved then it results in the creation
/// of a new sub-menu.  In Terminal.Gui this means converting the above <see cref="MenuItem"/>
/// into a <see cref="MenuBarItem"/> (the class for menu items that contain sub-menu items).
/// </summary>
public class MoveMenuItemRightOperation : MenuItemOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoveMenuItemRightOperation"/> class.
    /// </summary>
    /// <param name="toMove">Moves the <paramref name="toMove"/> to the sub-menu of the <see cref="MenuItem"/> above it.</param>
    public MoveMenuItemRightOperation(MenuItem toMove)
        : base(toMove)
    {
        if (this.Parent?.GetChildrenIndex(toMove) == 0)
        {
            this.IsImpossible = true;
        }
    }

    /// <summary>
    /// Gets or Sets insertion at a specific index within the destination
    /// sub-menu.  Leave null to simply move it to the bottom of the new
    /// sub-menu.
    /// </summary>
    public int? InsertionIndex { get; set; }

    /// <inheritdoc/>
    public override void Redo()
    {
        if (this.OperateOn == null)
        {
            return;
        }

        new MoveMenuItemRightOperation(this.OperateOn).Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return;
        }

        new MoveMenuItemLeftOperation(this.OperateOn).Do();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
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

        // add us to the sub-menu
        var submenuChildren = addTo.Children.ToList<MenuItem>();

        if (this.InsertionIndex != null)
        {
            submenuChildren.Insert(
                Math.Min(this.InsertionIndex.Value, submenuChildren.Count),
                this.OperateOn);
        }
        else
        {
            submenuChildren.Add(this.OperateOn);
        }

        // update the main menu
        this.Parent.Children = children.ToArray();

        // update the sub-menu
        addTo.Children = submenuChildren.ToArray();

        this.Bar?.SetNeedsDraw();

        return true;
    }

    private MenuBarItem ConvertToMenuBarItem(List<MenuItem> children, int idx)
    {
        if (children[idx] is MenuBarItem mb)
        {
            return mb;
        }

        var added = new MenuBarItem(children[idx].Title, new MenuItem[0], null);
        added.Data = children[idx].Data;
        added.ShortcutKey = children[idx].ShortcutKey;

        children.RemoveAt(idx);
        children.Insert(idx, added);
        return added;
    }
}