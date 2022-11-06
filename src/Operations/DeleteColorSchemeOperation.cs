using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class DeleteColorSchemeOperation : Operation
{
    public NamedColorScheme ToDelete { get; }

    private Design[] users;
    private Design rootDesign;

    public DeleteColorSchemeOperation(Design design, NamedColorScheme toDelete)
    {
        this.ToDelete = toDelete;
        this.users = design.GetAllDesigns().Where(d => d.UsesColorScheme(toDelete.Scheme)).ToArray();
        this.rootDesign = design;
    }

    public override bool Do()
    {
        foreach (var u in this.users)
        {
            // we are no longer using a custom scheme
            u.State.OriginalScheme = null;

            // we use the default (usually thats to inherit from parent)
            u.View.ColorScheme = this.GetDefaultColorScheme(u);
        }

        ColorSchemeManager.Instance.Remove(this.ToDelete);
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

        if (d.View is MenuBar)
        {
            return Colors.Menu;
        }

        return null;
    }

    public override void Redo()
    {
        this.Do();
    }

    public override void Undo()
    {
        foreach (var u in this.users)
        {
            // go back to using this explicit scheme before we deleted it
            u.State.OriginalScheme = this.ToDelete.Scheme;
            u.View.ColorScheme = this.ToDelete.Scheme;
        }

        ColorSchemeManager.Instance.AddOrUpdateScheme(this.ToDelete.Name, this.ToDelete.Scheme, this.rootDesign);
    }
}