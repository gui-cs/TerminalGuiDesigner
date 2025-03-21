using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// <para>
/// Adds a new item on a menu.  These are sub items (e.g. Copy, Open)
/// not top level menus (File, Edit etc).  For top level menus see
/// <see cref="AddMenuOperation"/> instead.
/// </para>
/// <para>
/// Item added may to be a sub-menu drop down (e.g. File=>New=>ADDHERE).
/// </para>
/// </summary>
public class AddMenuItemOperation : MenuItemOperation
{
    private MenuItem? added;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddMenuItemOperation"/> class. When
    /// performed the operation will add a new <see cref="MenuItem"/> below <paramref name="adjacentTo"/>.
    /// </summary>
    /// <param name="adjacentTo">An existing <see cref="MenuItem"/> to add the new one below.  Must be a
    /// sub-menu (e.g. Open, Copy) not a top level menu (e.g. File, Edit).</param>
    public AddMenuItemOperation(MenuItem adjacentTo)
        : base(adjacentTo)
    {
    }

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        if (this.added != null)
        {
            this.Add(this.added);
        }
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        if (this.added == null)
        {
            return;
        }

        var remove = new RemoveMenuItemOperation(this.added);
        remove.Do();
    }

    /// <summary>
    /// Adds a new blank <see cref="MenuItem"/> to the menu.
    /// </summary>
    /// <returns>True if a new <see cref="MenuItem"/> was successfully added.</returns>
    protected override bool DoImpl()
    {
        return this.Add(this.added = new MenuItem());
    }

    private bool Add(MenuItem menuItem)
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return false;
        }

        var children = this.Parent.Children.ToList<MenuItem>();
        var currentItemIdx = children.IndexOf(this.OperateOn);

        // We are the parent but parents children don't contain
        // us.  Thats bad. TODO: log this
        if (currentItemIdx == -1)
        {
            return false;
        }

        int insertAt = Math.Max(0, currentItemIdx + 1);

        children.Insert(insertAt, menuItem);
        this.Parent.Children = children.ToArray();

        this.Bar?.SetNeedsDraw();

        return true;
    }
}
