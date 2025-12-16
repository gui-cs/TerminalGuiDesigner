using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Moves a <see cref="MenuItem"/> out from a sub menu and into
/// the next level of menu up. If it is the last item in it's menu
/// then that menu will be deleted after it is removed.
/// </summary>
public class MoveMenuItemLeftOperation : MenuItemOperation
{
    /// <summary>
    /// The index that the menu item started off with in
    /// its parents sub-menu so that if we undo we can reinstate
    /// its previous position.
    /// </summary>
    private int? pulledFromIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveMenuItemLeftOperation"/> class.
    /// This operation pulls a <see cref="MenuItem"/> out of a sub-menu onto the level above.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="toMove">The <see cref="MenuItem"/> to move to parent containing menu.</param>
    public MoveMenuItemLeftOperation(IApplication app, MenuItem toMove)
        : base(app, toMove)
    {
        // command is already invalid
        if (this.IsImpossible || this.Bar == null)
        {
            this.IsImpossible = true;
            return;
        }

        // Check if the item is in a top-level PopoverMenu (can't move left from there)
        if (this.Bar.SubViews.OfType<MenuBarItem>().Any(m => m.PopoverMenu?.Root?.SubViews.Contains(toMove) == true))
        {
            this.IsImpossible = true;
            return;
        }

        if (this.Parent != null)
        {
            var items = this.Parent.GetMenuItems();
            this.pulledFromIndex = items.IndexOf(this.OperateOn);
        }
    }

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        this.Do();
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        if (this.OperateOn == null || this.IsImpossible)
        {
            return;
        }

        new MoveMenuItemRightOperation(App, this.OperateOn)
        {
            InsertionIndex = this.pulledFromIndex,
        }
        .Do();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return false;
        }

        if (!MenuTracker.Instance.TryGetParent(Parent, out _, out MenuItem? parentsParent))
        {
            return false;
        }

        // Figure out where the parent is in the list
        var parentsParentItems = parentsParent.GetMenuItems();
        var parentsIdx = parentsParentItems.IndexOf(this.Parent);

        // remove us from our current location
        if (new RemoveMenuItemOperation(App, this.OperateOn).Do())
        {
            // We are the parent but parents children don't contain us.  That's bad. TODO: log this
            if (parentsIdx == -1)
            {
                return false;
            }

            int insertAt = Math.Max(0, parentsIdx + 1);

            // Insert into the parent's parent menu
            parentsParent.InsertMenuItem(insertAt, this.OperateOn);

            MenuTracker.Instance.ConvertEmptyMenus();

            this.Bar?.SetNeedsDraw();

            return true;
        }

        return false;
    }
}
