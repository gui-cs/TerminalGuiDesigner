using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class AddMenuItemOperation : MenuItemOperation
{
    private MenuItem? _added;

    public AddMenuItemOperation(MenuItem adjacentTo) : base(adjacentTo)
    {
    }

    public override bool Do()
    {
        return Add(_added = new MenuItem());
    }

    public override void Redo()
    {
        if (_added != null)
        {
            Add(_added);
        }
    }

    public override void Undo()
    {
        if (_added == null)
        {
            return;
        }

        var remove = new RemoveMenuItemOperation(_added);
        remove.Do();
    }

    private bool Add(MenuItem menuItem)
    {
        if (Parent == null || OperateOn == null)
        {
            return false;
        }

        var children = Parent.Children.ToList<MenuItem>();
        var currentItemIdx = children.IndexOf(OperateOn);

        // We are the parent but parents children don't contain
        // us.  Thats bad. TODO: log this
        if (currentItemIdx == -1)
        {
            return false;
        }

        int insertAt = Math.Max(0, currentItemIdx + 1);

        children.Insert(insertAt, menuItem);
        Parent.Children = children.ToArray();

        Bar?.SetNeedsDisplay();

        return true;
    }
}
