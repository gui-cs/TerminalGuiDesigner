using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;

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
    /// <param name="app">The application instance.</param>
    /// <param name="toMove">Moves the <paramref name="toMove"/> to the sub-menu of the <see cref="MenuItem"/> above it.</param>
    public MoveMenuItemRightOperation(IApplication app, MenuItem toMove)
        : base(app, toMove)
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            this.IsImpossible = true;
            return;
        }

        var items = this.Parent.GetMenuItems(out _);
        int idx = items.IndexOf(toMove);

        // Can't move right if we're the first item (no item above to become parent)
        if (idx <= 0)
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
    protected override void RedoImpl()
    {
        if (this.OperateOn == null)
        {
            return;
        }

        new MoveMenuItemRightOperation(App, this.OperateOn).Do();
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return;
        }

        new MoveMenuItemLeftOperation(App, this.OperateOn).Do();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return false;
        }

        var children = this.Parent.GetMenuItems(out _);
        var currentItemIdx = children.IndexOf(this.OperateOn);
        var aboveIdx = currentItemIdx - 1;

        if (aboveIdx < 0)
        {
            return false;
        }

        var itemAbove = children[aboveIdx];

        // Remove us from current menu first
        this.Parent.RemoveMenuItem(this.OperateOn);

        if (itemAbove is MenuBarItem existingMbi)
        {
            // Item above already has a sub-menu — add to it
            if (this.InsertionIndex != null)
            {
                existingMbi.InsertMenuItem(this.InsertionIndex.Value, this.OperateOn);
            }
            else
            {
                var subItems = existingMbi.GetMenuItems(out _);
                subItems.Add(this.OperateOn);
                existingMbi.SetMenuItems(subItems);
            }
        }
        else
        {
            // Convert plain MenuItem to MenuBarItem with OperateOn as first child.
            // Re-read children since RemoveMenuItem may have shifted indexes.
            children = this.Parent.GetMenuItems(out _);
            var newAboveIdx = children.IndexOf(itemAbove);

            var newMbi = new MenuBarItem(itemAbove.Title, new MenuItem[] { this.OperateOn });
            newMbi.Data = itemAbove.Data;
            newMbi.Key = itemAbove.Key;

            children.RemoveAt(newAboveIdx);
            children.Insert(newAboveIdx, newMbi);
            this.Parent.SetMenuItems(children);
        }

        this.Bar?.SetNeedsDraw();
        return true;
    }

    private MenuBarItem ConvertToMenuBarItem(List<MenuItem> children, int idx)
    {
        if (children[idx] is MenuBarItem mb)
        {
            return mb;
        }

        var added = new MenuBarItem()
        {
            Title = children[idx].Title
        };
        added.Data = children[idx].Data;
        added.Key = children[idx].Key;

        children.RemoveAt(idx);
        children.Insert(idx, added);
        return added;
    }
}