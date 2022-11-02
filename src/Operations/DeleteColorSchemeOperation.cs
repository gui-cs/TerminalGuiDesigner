using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DeleteColorSchemeOperation : Operation
{
    public NamedColorScheme ToDelete { get; }

    private Design[] _users;
    private Design _rootDesign;

    public DeleteColorSchemeOperation(Design design, NamedColorScheme toDelete)
    {
        ToDelete = toDelete;
        _users = design.GetAllDesigns().Where(d=>d.UsesColorScheme(toDelete.Scheme)).ToArray();
        _rootDesign = design;
    }

    public override bool Do()
    {
        foreach (var u in _users)
        {
            // we are no longer using a custom scheme
            u.State.OriginalScheme = null;

            // we use the default (usually thats to inherit from parent)
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
            // go back to using this explicit scheme before we deleted it
            u.State.OriginalScheme = ToDelete.Scheme;
            u.View.ColorScheme = ToDelete.Scheme;
        }

        ColorSchemeManager.Instance.AddOrUpdateScheme(ToDelete.Name,ToDelete.Scheme, _rootDesign);
    }
}