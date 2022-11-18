using Terminal.Gui;

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
    /// <param name="toMove">The <see cref="MenuItem"/> to move to parent containing menu.</param>
    public MoveMenuItemLeftOperation(MenuItem toMove)
        : base(toMove)
    {
        // command is already invalid or user is trying to move a menu item that is not in a sub-menu
        if (this.IsImpossible || this.Bar == null || this.Bar.Menus.Any(m => m.Children.Contains(toMove)))
        {
            this.IsImpossible = true;
            return;
        }

        if (this.Parent != null)
        {
            this.pulledFromIndex = Array.IndexOf(this.Parent.Children, this.OperateOn);
        }
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (this.OperateOn == null || this.IsImpossible)
        {
            return;
        }

        new MoveMenuItemRightOperation(this.OperateOn)
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

        var parentsParent = MenuTracker.Instance.GetParent(this.Parent, out var bar);

        if (parentsParent == null)
        {
            return false;
        }

        // Figure out where the parent MenuBarItem was in the list because
        // after we remove ourselves from its sublist it might
        // turn into a MenuItem (i.e. we loose the reference).
        var children = parentsParent.Children.ToList<MenuItem>();
        var parentsIdx = children.IndexOf(this.Parent);

        // remove us
        if (new RemoveMenuItemOperation(this.OperateOn).Do())
        {
            // We are the parent but parents children don't contain
            // us.  Thats bad. TODO: log this
            if (parentsIdx == -1)
            {
                return false;
            }

            int insertAt = Math.Max(0, parentsIdx + 1);

            children.Insert(insertAt, this.OperateOn);
            parentsParent.Children = children.ToArray();

            MenuTracker.Instance.ConvertEmptyMenus();

            this.Bar?.SetNeedsDisplay();

            return true;
        }

        return false;
    }
}
