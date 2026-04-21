using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// <para>
/// Converts a <see cref="MenuItem"/> into a Separator (horizontal line in menu).
/// In the designer the separator is stored as a <see cref="MenuItem"/> with
/// <see cref="SeparatorTitle"/> as its <see cref="MenuItem.Title"/>.
/// When code is generated it becomes a <c>new Line { Orientation = Orientation.Horizontal }</c>
/// and on load those Line views are converted back to sentinel MenuItems.
/// </para>
/// </summary>
public class ConvertMenuItemToSeperatorOperation : MenuItemOperation
{
    /// <summary>The title used to mark a MenuItem as a separator in the designer.</summary>
    public const string SeparatorTitle = "---";

    private string? originalTitle;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMenuItemToSeperatorOperation"/> class.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="toConvert">A <see cref="MenuItem"/> to convert into a separator.</param>
    public ConvertMenuItemToSeperatorOperation(IApplication app, MenuItem toConvert)
        : base(app, toConvert)
    {
        if (toConvert.Title?.ToString() == SeparatorTitle)
        {
            IsImpossible = true;
        }
    }

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        this.Do();
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        if (this.OperateOn == null)
        {
            return;
        }

        this.OperateOn.Title = this.originalTitle ?? string.Empty;
        this.Bar?.SetNeedsDraw();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.OperateOn == null)
        {
            return false;
        }

        this.originalTitle = this.OperateOn.Title?.ToString();
        this.OperateOn.Title = SeparatorTitle;
        this.Bar?.SetNeedsDraw();
        return true;
    }
}
