using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class MoveMenuItemLeftOperation : MenuItemOperation
{
    public MoveMenuItemLeftOperation(MenuItem toMove)
        : base(toMove)
    {
        // TODO prevent this if a root menu item
    }

    public override bool Do()
    {
        var parentsParent = MenuTracker.Instance.GetParent(Parent, out var bar);
        
        if(parentsParent == null)
            return false;

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
            if(parentsIdx == -1)
                return false;

            int insertAt = Math.Max(0,parentsIdx + 1);

            children.Insert(insertAt, OperateOn);
            parentsParent.Children = children.ToArray();

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
        new MoveMenuItemRightOperation(OperateOn)
        .Do();
    }

}
