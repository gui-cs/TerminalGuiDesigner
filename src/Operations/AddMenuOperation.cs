using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class AddMenuOperation: Operation
{
    private MenuBar _menuBar;
    private MenuBarItem? _newItem;

    public Design Design { get; }
    public AddMenuOperation(Design design)
    {
        Design = design;

        // somehow user ran this command for a non tab view
        if (Design.View is not MenuBar)
            throw new ArgumentException($"Design must be for a {nameof(MenuBar)} to support {nameof(AddMenuOperation)}");

        _menuBar = (MenuBar)Design.View;

    }

    public override bool Do()
    {
        if (_newItem != null)
            return false;

        var current = _menuBar.Menus.ToList<MenuBarItem>();
        current.Add(_newItem = new MenuBarItem() { Title = "Test" });
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
