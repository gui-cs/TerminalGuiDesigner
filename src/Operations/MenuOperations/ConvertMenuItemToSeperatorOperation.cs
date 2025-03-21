using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// <para>
/// Converts a <see cref="MenuItem"/> into a Separator (horizontal line in menu).
/// In Terminal.Gui this is represented as a null in the <see cref="MenuBar"/>.
/// </para>
/// </summary>
public class ConvertMenuItemToSeperatorOperation : MenuItemOperation
{
    private int removedAtIdx;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMenuItemToSeperatorOperation"/> class.
    /// </summary>
    /// <param name="toConvert">A <see cref="MenuItem"/> to replace with a separator (null) in it's parent <see cref="MenuBar"/>.</param>
    public ConvertMenuItemToSeperatorOperation(MenuItem toConvert)
        : base(toConvert)
    {
    }

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        this.Do();
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return;
        }

        var children = this.Parent.Children.ToList<MenuItem>();

        children[this.removedAtIdx] = this.OperateOn;
        this.Parent.Children = children.ToArray();
        this.Bar?.SetNeedsDraw();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return false;
        }

        var children = this.Parent.Children.ToList<MenuItem?>();

        this.removedAtIdx = Math.Max(0, children.IndexOf(this.OperateOn));
        children[this.removedAtIdx] = null;

        this.Parent.Children = children.ToArray();
        this.Bar?.SetNeedsDraw();

        return true;
    }
}