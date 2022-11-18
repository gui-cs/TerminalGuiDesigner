using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Moves a top level <see cref="MenuBarItem"/> (File, Edit etc) left or right within the ordering
/// of menus in a <see cref="MenuBar"/>.
/// </summary>
public class MoveMenuOperation : Operation
{
    /// <summary>
    /// Gets the number of index positions the <see cref="MenuBarItem"/> will
    /// be moved. Negative for left, positive for right.
    /// </summary>
    private readonly int adjustment;
    private readonly int originalIdx;
    private readonly int newIndex;
    private MenuBar menuBar;
    private MenuBarItem toMove;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveMenuOperation"/> class.
    /// Creates an operation that will change the ordering of top level menus within
    /// a <see cref="MenuBar"/>.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="MenuBar"/>.</param>
    /// <param name="toMove">The top level menu to move.</param>
    /// <param name="adjustment">Negative to move menu left, positive to move menu right.</param>
    public MoveMenuOperation(Design design, MenuBarItem toMove, int adjustment)
    {
        if (design.View is not MenuBar mb)
        {
            throw new ArgumentException($"Design must wrap a {nameof(MenuBar)} to be used with this method.");
        }

        this.menuBar = mb;
        this.toMove = toMove;
        this.originalIdx = Array.IndexOf(this.menuBar.Menus, toMove);

        if (this.originalIdx == -1)
        {
            throw new ArgumentException(nameof(toMove), $"{nameof(toMove)} {nameof(MenuBarItem)} did not belong to the passed {nameof(design)}");
        }

        // calculate new index without falling off array
        this.newIndex = Math.Max(0, Math.Min(this.menuBar.Menus.Length - 1, this.originalIdx + adjustment));

        // they are moving it nowhere?!
        if (this.newIndex == this.originalIdx)
        {
            this.IsImpossible = true;
        }

        this.adjustment = adjustment;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this.adjustment == 0)
        {
            return $"Bad Command '{this.GetType().Name}'";
        }

        if (this.adjustment < 0)
        {
            return $"Move '{this.toMove.Title}' Left";
        }

        if (this.adjustment > 0)
        {
            return $"Move '{this.toMove.Title}' Right";
        }

        return base.ToString();
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        var menus = this.menuBar.Menus.ToList();

        menus.Remove(this.toMove);
        menus.Insert(this.originalIdx, this.toMove);

        this.menuBar.Menus = menus.Cast<MenuBarItem>().ToArray();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        var menus = this.menuBar.Menus.ToList();

        menus.Remove(this.toMove);
        menus.Insert(this.newIndex, this.toMove);

        this.menuBar.Menus = menus.Cast<MenuBarItem>().ToArray();
        return true;
    }
}
