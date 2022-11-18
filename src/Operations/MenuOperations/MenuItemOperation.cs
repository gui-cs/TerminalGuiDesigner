using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Abstract base class for <see cref="Operation"/> which affect a <see cref="MenuItem"/>
/// on a <see cref="MenuBar"/>.
/// </summary>
public abstract class MenuItemOperation : Operation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItemOperation"/> class.
    /// </summary>
    /// <param name="operateOn"><see cref="MenuItem"/> that you will operate on.</param>
    protected MenuItemOperation(MenuItem operateOn)
    {
        // if taking a new line add an extra menu item
        // menuItem.Parent doesn't work for root menu items
        var parent = MenuTracker.Instance.GetParent(operateOn, out var bar);

        if (parent == null || bar == null)
        {
            this.IsImpossible = true;
            return;
        }

        this.Bar = bar;
        this.Parent = parent;
        this.OperateOn = operateOn;
    }

    /// <summary>
    /// Gets the big long bar view that goes at the top of windows
    /// on which the <see cref="OperateOn"/> <see cref="MenuItem"/>
    /// resides.
    /// </summary>
    public MenuBar? Bar { get; private set; }

    /// <summary>
    /// Gets the collection that contains the menu item being operated on.
    /// This may be a top level entry on the <see cref="Bar"/> (File, Edit etc)
    /// or it could be a sub entry of that if <see cref="OperateOn"/> is in a sub menu
    /// (e.g. File=>*New*=>Document - where Parent is *New*).
    /// </summary>
    public MenuBarItem? Parent { get; private set; }

    /// <summary>
    /// Gets the <see cref="MenuItem"/> that will be affected by this <see cref="Operation"/>.
    /// </summary>
    protected MenuItem? OperateOn { get; private set; }
}