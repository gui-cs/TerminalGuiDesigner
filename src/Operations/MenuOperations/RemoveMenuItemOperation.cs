using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Removes a <see cref="MenuItem"/> from its drop down menu.
/// This is a drop down menu item e.g. Open, Save (which might be in a sub menu)
/// not a top level menu e.g. File, Edit.
/// </summary>
public class RemoveMenuItemOperation : MenuItemOperation
{
    private int removedAtIdx;

    /// <summary>
    /// If as a result of removing this menu item any
    /// top level menu items were also removed (for being
    /// empty). Track indexes here so we can undo if necessary.
    /// </summary>
    private Dictionary<int, MenuBarItem>? prunedEmptyTopLevelMenus;

    /// <summary>
    /// If the removed MenuItem was a sub-menu item and it was the last one then
    /// its parent will have been converted from a MenuBarItem (has children)
    /// to a regular MenuItem (does not have children).  This collection
    /// stores all such replacements made during carrying out the command.
    /// </summary>
    private Dictionary<MenuBarItem, MenuItem>? convertedMenuBars;

    /// <summary>
    /// If as a result of removing this MenuItem the MenuBar ended
    /// up being completely blank and was therefore removed.  This
    /// field stores the View it was removed from (for undo purposes).
    /// </summary>
    private View? barRemovedFrom;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveMenuItemOperation"/> class.
    /// </summary>
    /// <param name="toRemove">The <see cref="MenuItem"/> that should be removed (deleted).</param>
    public RemoveMenuItemOperation(MenuItem toRemove)
        : base(toRemove)
    {
    }

    /// <summary>
    /// Gets a value indicating whether as a result of removing a this menu item any
    /// top level menu items were also removed (for being empty).
    /// </summary>
    /// <remarks>This will be true if removing the last entry (e.g. Open) under a top
    /// level menu (e.g. File).</remarks>
    public bool PrunedTopLevelMenu => this.prunedEmptyTopLevelMenus != null && this.prunedEmptyTopLevelMenus.Any();

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        this.Do();
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return;
        }

        this.Parent.Children =
        [
            .. Parent.Children[ .. removedAtIdx ],
            this.OperateOn,
            .. Parent.Children[ removedAtIdx .. ]
        ];
        this.Bar?.SetNeedsDraw();

        // if any MenuBarItem were converted to vanilla MenuItem
        // because we were removed from a sub-menu then convert
        // it back
        if (this.convertedMenuBars != null)
        {
            foreach (var converted in this.convertedMenuBars)
            {
                if(MenuTracker.Instance.TryGetParent(converted.Value,out _, out MenuBarItem? grandparent))
                {
                    int replacementIndex = Array.IndexOf(grandparent.Children, converted.Value);
                    if(replacementIndex >=0 && replacementIndex < grandparent.Children.Length)
                    {
                        grandparent.Children[ replacementIndex ] = converted.Key;
                    }
                }
            }
        }

        // if we removed any top level empty menus as a
        // side effect of the removal then put them back
        if (this.prunedEmptyTopLevelMenus != null && this.Bar != null)
        {
            var l = this.Bar.Menus.ToList<MenuBarItem>();

            // for each index they used to be at
            foreach (var kvp in this.prunedEmptyTopLevelMenus.OrderBy(k => k))
            {
                // put them back
                l.Insert(kvp.Key, kvp.Value);
            }

            this.Bar.Menus = l.ToArray();
        }

        if (this.Bar != null && this.barRemovedFrom != null)
        {
            this.barRemovedFrom.Add(this.Bar);

            // lets clear this in case the user some
            // manages to undo this command multiple
            // times
            this.barRemovedFrom = null;
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return false;
        }

        this.removedAtIdx = Math.Max( 0, Array.IndexOf( Parent.Children, OperateOn ) );
        this.Parent.Children =
        [
            .. Parent.Children[ ..removedAtIdx ],
            .. Parent.Children[ ( removedAtIdx + 1 ).. ]
        ];
        this.Bar?.SetNeedsDraw();

        if (this.Bar != null)
        {
            this.convertedMenuBars = MenuTracker.Instance.ConvertEmptyMenus();
        }

        // if a top level menu now has no children
        var empty = this.Bar?.Menus.Where(bi => bi.Children.Length == 0).ToArray();
        if (empty?.Any() == true)
        {
            // remember where they were
            this.prunedEmptyTopLevelMenus = empty.ToDictionary(e => Array.IndexOf(this.Bar.Menus, e), v => v);

            // and remove them
            this.Bar.Menus = this.Bar.Menus.Except(this.prunedEmptyTopLevelMenus.Values).ToArray();
        }

        // if we just removed the last menu header
        // leaving a completely blank menu bar
        if (this.Bar?.Menus.Length == 0 && this.Bar.SuperView != null)
        {
            // remove the bar completely
            this.Bar.CloseMenu(false);
            this.barRemovedFrom = this.Bar.SuperView;
            this.barRemovedFrom.Remove(this.Bar);
        }

        return true;
    }
}