using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveMenuItemLeftOperation : MenuItemOperation
{
    /// <summary>
    /// The index that the menu item started off with in
    /// its parents submenu so that if we undo we can reinstate
    /// its previous position
    /// </summary>
    private int? _pulledFromIndex;

    public MoveMenuItemLeftOperation(MenuItem toMove)
        : base(toMove)
    {
        if (Parent != null)
        {
            _pulledFromIndex = Array.IndexOf(Parent.Children, OperateOn);
        }

        // TODO prevent this if a root menu item
    }

    public override bool Do()
    {
        if (Parent == null || OperateOn == null)
        {
            return false;
        }

        var parentsParent = MenuTracker.Instance.GetParent(Parent, out var bar);

        if (parentsParent == null)
        {
            return false;
        }

        // Figure out where the parent MenuBarItem was in the list because
        // after we remove ourselves from its sublist it might
        // turn into a MenuItem (i.e. we loose the reference).
        var children = parentsParent.Children.ToList<MenuItem>();
        var parentsIdx = children.IndexOf(Parent);

        // remove us
        if (new RemoveMenuItemOperation(OperateOn).Do())
        {
            // We are the parent but parents children don't contain
            // us.  Thats bad. TODO: log this
            if (parentsIdx == -1)
            {
                return false;
            }

            int insertAt = Math.Max(0, parentsIdx + 1);

            children.Insert(insertAt, OperateOn);
            parentsParent.Children = children.ToArray();

            MenuTracker.Instance.ConvertEmptyMenus();

            Bar?.SetNeedsDisplay();

            return true;
        }

        return false;
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        if (OperateOn == null)
        {
            return;
        }

        new MoveMenuItemRightOperation(OperateOn)
        {
            InsertionIndex = _pulledFromIndex
        }
        .Do();
    }
}
