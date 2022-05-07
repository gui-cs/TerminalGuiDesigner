using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class AddMenuItemOperation : MenuItemOperation
{
    private MenuItem? _added;

    public AddMenuItemOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem adacentTo)
        : base(focusedView,bar,parent,adacentTo)
    {
        
    }

    public override bool Do()
    {
        return Add(_added = new MenuItem{Title = "New Item"});
    }


    public override void Redo()
    {
        if(_added != null)
            Add(_added);
    }

    public override void Undo()
    {
        if(_added == null)
            return;

        var remove = new DeleteMenuItemOperation(FocusedView,Bar,Parent,_added);
        remove.Do();
    }

    private bool Add(MenuItem menuItem)
    {
        var children = Parent.Children.ToList<MenuItem>();
        var currentItemIdx = children.IndexOf(OperateOn);

        // We are the parent but parents children don't contain
        // us.  Thats bad. TODO: log this
        if(currentItemIdx == -1)
            return false;

        int insertAt = Math.Max(0,currentItemIdx + 1);

        children.Insert(insertAt,menuItem);
        Parent.Children = children.ToArray();
        
        FocusedView.SetNeedsDisplay();
        
        return true;
    }
}
