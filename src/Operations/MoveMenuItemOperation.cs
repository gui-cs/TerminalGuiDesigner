using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Moves a <see cref="MenuItem"/> up or down within the same menu.  To move
/// to sub-menu or out of sub-menu see <see cref="MoveMenuItemLeftOperation"/>
/// and <see cref="MoveMenuItemRightOperation"/> instead.
/// </summary>
public class MoveMenuItemOperation : MenuItemOperation
{
    private bool up;
    private List<MenuItem>? siblings;
    private int currentItemIdx;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveMenuItemOperation"/> class.
    /// </summary>
    /// <param name="toMove">The <see cref="MenuItem"/> that should change places relative to other <see cref="MenuItem"/>
    /// on its <see cref="MenuBarItem"/>.</param>
    /// <param name="up">True to move up on the screen (array index decreases).  False to move down on the screen (array index increases).</param>
    public MoveMenuItemOperation(MenuItem toMove, bool up)
        : base(toMove)
    {
        this.up = up;

        // command is invalid
        if (this.IsImpossible || this.Parent == null || this.OperateOn == null)
        {
            this.IsImpossible = true;
            return;
        }

        this.siblings = this.Parent.Children.ToList<MenuItem>();
        this.currentItemIdx = this.siblings.IndexOf(this.OperateOn);

        if (this.currentItemIdx < 0)
        {
            this.IsImpossible = true;
        }
        else
        {
            this.IsImpossible = up ? this.currentItemIdx == 0 : this.currentItemIdx == this.siblings.Count - 1;
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
        this.Move(this.up ? 1 : -1);
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        return this.Move(this.up ? -1 : 1);
    }

    private bool Move(int amount)
    {
        if (this.Parent == null || this.OperateOn == null || this.siblings == null)
        {
            return false;
        }

        int moveTo = Math.Max(0, amount + this.currentItemIdx);

        // pull it out from wherever it is
        this.siblings.Remove(this.OperateOn);

        moveTo = Math.Min(moveTo, this.siblings.Count);

        // push it in at the destination
        this.siblings.Insert(moveTo, this.OperateOn);
        this.Parent.Children = this.siblings.ToArray();

        this.Bar?.SetNeedsDisplay();

        return true;
    }
}