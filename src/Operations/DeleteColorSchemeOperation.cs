using static TerminalGuiDesigner.ColorSchemeManager;

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
            u.View.ColorScheme = null;
        }

        ColorSchemeManager.Instance.Remove(ToDelete);
        return true;
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