using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// <see cref="Operation"/> for adding a new top level menu to a <see cref="MenuBar"/> (e.g. File, Edit).
/// </summary>
public class AddMenuOperation : Operation
{
    /// <summary>
    /// <para>
    /// <see cref="AddMenuOperation"/> adds a new top level menu (e.g. File, Edit etc).  In the designer
    /// all menus must have at least 1 <see cref="MenuItem"/> under them so it will be
    /// created with a single <see cref="MenuItem"/> in it already.  That item will
    /// bear this text.
    /// </para>
    /// <para>
    /// This string should be used by any other areas of code that want to create new <see cref="MenuItem"/> under
    /// a top/sub menu (e.g. <see cref="ViewFactory"/>).
    /// </para>
    /// </summary>
    public const string DefaultMenuItemText = "Edit Me";

    private MenuBar menuBar;
    private MenuBarItem? newItem;
    private string? name;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddMenuOperation"/> class.
    /// Calling <see cref="Do"/> will add a new top level menu to the <see cref="MenuBar"/>
    /// wrapped by <paramref name="design"/>.
    /// </summary>
    /// <param name="design"><see cref="Design"/> wrapper for a view of Type <see cref="MenuBar"/>.</param>
    /// <param name="name">Optional explicit name to add with or null to prompt user interactively.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="design"/> is not wrapping a <see cref="MenuBar"/>.</exception>
    public AddMenuOperation(Design design, string? name)
    {
        this.Design = design;
        this.name = name;

        // somehow user ran this command for a non tab view
        if (this.Design.View is not MenuBar)
        {
            throw new ArgumentException($"Design must be for a {nameof(MenuBar)} to support {nameof(AddMenuOperation)}");
        }

        this.menuBar = (MenuBar)this.Design.View;
    }

    /// <summary>
    /// Gets the <see cref="Design"/> which will be operated on by this operation.  The
    /// <see cref="Design.View"/> will be a <see cref="MenuBar"/>.
    /// </summary>
    public Design Design { get; }

    /// <inheritdoc/>
    public override void Undo()
    {
        // its not there anyways
        if (this.newItem == null || !this.menuBar.Menus.Contains(this.newItem))
        {
            return;
        }

        var current = this.menuBar.Menus.ToList<MenuBarItem>();
        current.Remove(this.newItem);
        this.menuBar.Menus = current.ToArray();
        this.menuBar.SetNeedsDisplay();
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        // its already there
        if (this.newItem == null || this.menuBar.Menus.Contains(this.newItem))
        {
            return;
        }

        var current = this.menuBar.Menus.ToList<MenuBarItem>();
        current.Add(this.newItem);
        this.menuBar.Menus = current.ToArray();
        this.menuBar.SetNeedsDisplay();
    }

    /// <summary>
    /// Performs the operation.  Adding a new top level menu.
    /// </summary>
    /// <remarks>Calling this method multiple times will not result in more new menus.</remarks>
    /// <returns>True if the menu was added.  False if making repeated calls or user cancels naming the new menu etc.</returns>
    protected override bool DoImpl()
    {
        // if we have already run this operation
        if (this.newItem != null)
        {
            return false;
        }

        string? uniqueName = this.name;

        if (uniqueName == null)
        {
            if (!Modals.GetString("Name", "Name", "MyMenu", out uniqueName))
            {
                // user canceled adding
                return false;
            }
        }

        uniqueName = uniqueName?.MakeUnique(
                this.menuBar.Menus.Select(c => c.Title.ToString())
                .Where(t => t != null)
                .Cast<string>());

        var current = this.menuBar.Menus.ToList<MenuBarItem>();
        current.Add(this.newItem = new MenuBarItem(uniqueName, new MenuItem[] { new MenuItem { Title = DefaultMenuItemText } }));
        this.menuBar.Menus = current.ToArray();
        this.menuBar.SetNeedsDisplay();

        return true;
    }
}
