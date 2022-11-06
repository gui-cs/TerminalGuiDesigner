using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class AddMenuItemOperation : MenuItemOperation
{
    private MenuItem? added;

    public AddMenuItemOperation(MenuItem adjacentTo)
        : base(adjacentTo)
    {
    }

    public override bool Do()
    {
        return this.Add(this.added = new MenuItem());
    }

    public override void Redo()
    {
        if (this.added != null)
        {
            this.Add(this.added);
        }
    }

    public override void Undo()
    {
        if (this.added == null)
        {
            return;
        }

        var remove = new RemoveMenuItemOperation(this.added);
        remove.Do();
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

        this.Bar?.SetNeedsDisplay();

        return true;
    }
}
