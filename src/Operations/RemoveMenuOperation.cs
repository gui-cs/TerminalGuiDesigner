using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class RemoveMenuOperation : Operation
{
    private MenuBar menuBar;
    private MenuBarItem? toRemove;
    private int idx;

    public Design Design { get; }
    public RemoveMenuOperation(Design design)
    {
        this.Design = design;

        // somehow user ran this command for a non tab view
        if (this.Design.View is not MenuBar)
        {
            throw new ArgumentException($"Design must be for a {nameof(MenuBar)} to support {nameof(AddMenuOperation)}");
        }

        this.menuBar = (MenuBar)this.Design.View;
        this.toRemove = this.menuBar.GetSelectedMenuItem();

        this.IsImpossible = this.toRemove == null;
    }

    public override bool Do()
    {
        if (this.toRemove == null || !this.menuBar.Menus.Contains(this.toRemove))
        {
            return false;
        }

        var current = this.menuBar.Menus.ToList<MenuBarItem>();
        this.idx = current.IndexOf(this.toRemove);
        current.Remove(this.toRemove);
        this.menuBar.Menus = current.ToArray();
        this.menuBar.SetNeedsDisplay();

        return true;
    }

    public override void Undo()
    {
        // its not there anyways
        if (this.toRemove == null)
        {
            return;
        }

        var current = this.menuBar.Menus.ToList<MenuBarItem>();
        current.Insert(this.idx, this.toRemove);
        this.menuBar.Menus = current.ToArray();
        this.menuBar.SetNeedsDisplay();
    }

    public override void Redo()
    {
        this.Do();
    }

    public override string ToString()
    {
        return $"Remove Menu '{this.toRemove?.Title}'";
    }
}
