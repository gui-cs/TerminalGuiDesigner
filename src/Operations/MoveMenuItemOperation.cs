using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveMenuItemOperation : MenuItemOperation
{
    private bool up;
    private List<MenuItem>? siblings;
    private int currentItemIdx;

    public MoveMenuItemOperation(MenuItem toMove, bool up)
        : base(toMove)
    {
        this.up = up;

        if (this.Parent == null || this.OperateOn == null)
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

    public override bool Do()
    {
        return this.Move(this.up ? -1 : 1);
    }

    public override void Redo()
    {
        this.Do();
    }

    public override void Undo()
    {
        this.Move(this.up ? 1 : -1);
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