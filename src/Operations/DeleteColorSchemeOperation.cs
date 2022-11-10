using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Removes a <see cref="NamedColorScheme"/> from all users and clears it from
/// <see cref="ColorSchemeManager"/>.
/// </summary>
public class DeleteColorSchemeOperation : Operation
{
    /// <summary>
    /// All users of <see cref="ToDelete"/> which were found at the time the operation
    /// was constructed.
    /// </summary>
    private Design[] users;
    private Design rootDesign;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteColorSchemeOperation"/> class.
    /// </summary>
    /// <param name="design">Any <see cref="Design"/> from which the root design can be obtained (required
    /// to find users of <paramref name="toDelete"/>.</param>
    /// <param name="toDelete">The <see cref="NamedColorScheme"/> to delete.  Any <see cref="Design"/> that
    /// use this scheme will revert to '(Inherited)' (null).</param>
    public DeleteColorSchemeOperation(Design design, NamedColorScheme toDelete)
    {
        this.ToDelete = toDelete;
        this.users = design.GetAllDesigns().Where(d => d.UsesColorScheme(toDelete.Scheme)).ToArray();
        this.rootDesign = design;
    }

    /// <summary>
    /// Gets the <see cref="NamedColorScheme"/> that will be deleted when operation is run (see <see cref="Do"/>).
    /// This will be removed from <see cref="ColorSchemeManager"/> and all users.
    /// </summary>
    public NamedColorScheme ToDelete { get; }

    /// <inheritdoc/>
    protected override bool DoImpl()
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

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
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
}