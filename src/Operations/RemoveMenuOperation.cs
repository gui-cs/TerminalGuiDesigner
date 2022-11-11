using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Removes a top level menu from a <see cref="MenuBar"/> e.g. File, Edit.
/// For removing items under a menu (e.g. Open/Save) see <see cref="RemoveMenuItemOperation"/>
/// instead.
/// </summary>
public class RemoveMenuOperation : Operation
{
    private readonly MenuBar menuBar;
    private readonly MenuBarItem? toRemove;
    private int idx;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveMenuOperation"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="MenuBar"/> upon which you wish to operate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="MenuBar"/>.</exception>
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

    /// <summary>
    /// Gets the <see cref="MenuBar"/> from which to remove.
    /// </summary>
    public Design Design { get; }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Remove Menu '{this.toRemove?.Title}'";
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
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
}