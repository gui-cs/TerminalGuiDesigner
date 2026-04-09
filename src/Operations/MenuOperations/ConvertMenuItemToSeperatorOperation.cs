using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// <para>
/// Converts a <see cref="MenuItem"/> into a Separator (horizontal line in menu).
/// In the new Terminal.Gui API this is represented as a Line view.
/// </para>
/// </summary>
public class ConvertMenuItemToSeperatorOperation : MenuItemOperation
{
    private int removedAtIdx;
    private Line? addedLine;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMenuItemToSeperatorOperation"/> class.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="toConvert">A <see cref="MenuItem"/> to replace with a separator (Line) in it's parent menu.</param>
    public ConvertMenuItemToSeperatorOperation(IApplication app, MenuItem toConvert)
        : base(app, toConvert)
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
        if (this.Parent == null || this.OperateOn == null || this.addedLine == null)
        {
            return;
        }

        var menu = this.Parent.GetChildMenu(out _);
        if (menu == null)
        {
            return;
        }

        // Find the index of the separator line and restore MenuItem at that position
        var allViews = menu.SubViews.ToList();
        int lineIdx = allViews.IndexOf(this.addedLine);

        if (lineIdx >= 0)
        {
            // Remove the separator
            menu.Remove(this.addedLine);

            // Rebuild views with MenuItem at the correct position
            var nonLineViews = allViews.Where(v => v != this.addedLine).ToList();
            nonLineViews.Insert(Math.Min(lineIdx, nonLineViews.Count), this.OperateOn);

            // Clear and re-add all views in order
            foreach (var v in allViews.Where(v => v != this.addedLine).ToList())
            {
                menu.Remove(v);
            }

            foreach (var v in nonLineViews)
            {
                menu.Add(v);
            }
        }

        this.Bar?.SetNeedsDraw();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.Parent == null || this.OperateOn == null)
        {
            return false;
        }

        var menu = this.Parent.GetChildMenu(out _);
        if (menu == null)
        {
            return false;
        }

        var items = this.Parent.GetMenuItems(out _);
        this.removedAtIdx = Math.Max(0, items.IndexOf(this.OperateOn));

        // Find the actual index in SubViews
        var allViews = menu.SubViews.ToList();
        int actualIdx = allViews.IndexOf(this.OperateOn);

        if (actualIdx < 0)
        {
            return false;
        }

        // Remove the MenuItem
        menu.Remove(this.OperateOn);

        // Create Line separator and rebuild views with it at the correct position
        this.addedLine = new Line { Orientation = Terminal.Gui.ViewBase.Orientation.Horizontal };
        var newViews = allViews.Where(v => v != this.OperateOn).ToList();
        newViews.Insert(Math.Min(actualIdx, newViews.Count), this.addedLine);

        // Clear and re-add all views in order
        foreach (var v in allViews.Where(v => v != this.OperateOn).ToList())
        {
            menu.Remove(v);
        }

        foreach (var v in newViews)
        {
            menu.Add(v);
        }

        this.Bar?.SetNeedsDraw();

        return true;
    }
}