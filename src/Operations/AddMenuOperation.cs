using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

public class AddMenuOperation: Operation
{
    private MenuBar _menuBar;
    private MenuBarItem? _newItem;

    public Design Design { get; }

    private string? _name;

    /// <summary>
    /// Adds a new top level menu to a <see cref="MenuBar"/>. 
    /// </summary>
    /// <param name="design"></param>
    /// <param name="name">Optional explicit name to add with or null to prompt user interactively</param>
    /// <exception cref="ArgumentException"></exception>
    public AddMenuOperation(Design design, string? name)
    {
        Design = design;
        _name = name;

        // somehow user ran this command for a non tab view
        if (Design.View is not MenuBar)
            throw new ArgumentException($"Design must be for a {nameof(MenuBar)} to support {nameof(AddMenuOperation)}");

        _menuBar = (MenuBar)Design.View;

    }

    public override bool Do()
    {
        if (_newItem != null)
            return false;

        if (_name == null)
            if (Modals.GetString("Name", "Name", "MyMenu", out string? newName) && !string.IsNullOrWhiteSpace(newName))
            {
                _name = newName;
            }
            else
                return false; //user cancelled naming the new menu


        var current = _menuBar.Menus.ToList<MenuBarItem>();
        current.Add(_newItem = new MenuBarItem(_name,new MenuItem[] { new MenuItem { Title = "Edit Me" } }));
        _menuBar.Menus = current.ToArray();
        _menuBar.SetNeedsDisplay();

        return true;
    }

    public override void Undo()
    {
        // its not there anyways
        if (_newItem == null || !_menuBar.Menus.Contains(_newItem))
            return;

        var current = _menuBar.Menus.ToList<MenuBarItem>();
        current.Remove(_newItem);
        _menuBar.Menus = current.ToArray();
        _menuBar.SetNeedsDisplay();
    }

    public override void Redo()
    {
        // its already there
        if (_newItem == null || _menuBar.Menus.Contains(_newItem))
            return;

        var current = _menuBar.Menus.ToList<MenuBarItem>();
        current.Add(_newItem);
        _menuBar.Menus = current.ToArray();
        _menuBar.SetNeedsDisplay();
    }
}
