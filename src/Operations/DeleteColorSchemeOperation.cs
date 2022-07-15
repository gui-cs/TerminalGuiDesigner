using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DeleteColorSchemeOperation : Operation
{
    public NamedColorScheme ToDelete { get; }

    private Design[] _users;

    public DeleteColorSchemeOperation(Design design, NamedColorScheme toDelete)
    {
        ToDelete = toDelete;
        _users = design.GetAllDesigns().Where(d=>d.UsesColorScheme(toDelete.Scheme)).ToArray();
    }

    public override bool Do()
    {
        foreach (var u in _users)
        {
            u.View.ColorScheme = GetDefaultColorScheme(u);
        }

        ColorSchemeManager.Instance.Remove(ToDelete);
        return true;
    }

    private ColorScheme? GetDefaultColorScheme(Design d)
    {
        if (d.IsRoot)
        {
            switch (d.View)
            {
                case Dialog: return Colors.Dialog;
                case Window: return Colors.Base;
                default: return null;
            }
        }

        if(d.View is MenuBar)
            return Colors.Menu;

        return null;
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        foreach (var u in _users)
        {
            u.View.ColorScheme = ToDelete.Scheme;
        }

        ColorSchemeManager.Instance.AddOrUpdateScheme(ToDelete.Name,ToDelete.Scheme);
    }
}