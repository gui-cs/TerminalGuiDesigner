using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class RemoveMenuOperation : Operation
{
    private MenuBar _menuBar;
    private MenuBarItem? _toRemove;
    private int idx;

    public Design Design { get; }
    public RemoveMenuOperation(Design design)
    {
        Design = design;

        // somehow user ran this command for a non tab view
        if (Design.View is not MenuBar)
        {
            throw new ArgumentException($"Design must be for a {nameof(MenuBar)} to support {nameof(AddMenuOperation)}");
        }

        _menuBar = (MenuBar)Design.View;
        _toRemove = _menuBar.GetSelectedMenuItem();

        IsImpossible = _toRemove == null;
    }

    public override bool Do()
    {
        if (_toRemove == null || !_menuBar.Menus.Contains(_toRemove))
        {
            return false;
        }

        var current = _menuBar.Menus.ToList<MenuBarItem>();
        idx = current.IndexOf(_toRemove);
        current.Remove(_toRemove);
        _menuBar.Menus = current.ToArray();
        _menuBar.SetNeedsDisplay();

        return true;
    }

    public override void Undo()
    {
        // its not there anyways
        if (_toRemove == null)
        {
            return;
        }

        var current = _menuBar.Menus.ToList<MenuBarItem>();
        current.Insert(idx, _toRemove);
        _menuBar.Menus = current.ToArray();
        _menuBar.SetNeedsDisplay();
    }

    public override void Redo()
    {
        Do();
    }

    public override string ToString()
    {
        return $"Remove Menu '{_toRemove?.Title}'";
    }
}
