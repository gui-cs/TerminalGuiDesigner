using Terminal.Gui;
using Terminal.Gui.Drawing;
using Terminal.Gui.Views;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Removes a <see cref="NamedScheme"/> from all users and clears it from
/// <see cref="SchemeManager"/>.
/// </summary>
public class DeleteSchemeOperation : Operation
{
    /// <summary>
    /// All users of <see cref="ToDelete"/> which were found at the time the operation
    /// was constructed.
    /// </summary>
    private Design[] users;
    private Design rootDesign;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSchemeOperation"/> class.
    /// </summary>
    /// <param name="design">Any <see cref="Design"/> from which the root design can be obtained (required
    /// to find users of <paramref name="toDelete"/>.</param>
    /// <param name="toDelete">The <see cref="NamedScheme"/> to delete.  Any <see cref="Design"/> that
    /// use this scheme will revert to '(Inherited)' (null).</param>
    public DeleteSchemeOperation(Design design, NamedScheme toDelete)
    {
        this.ToDelete = toDelete;
        this.users = design.GetAllDesigns().Where(d => d.UsesScheme(toDelete.Scheme)).ToArray();
        this.rootDesign = design;
    }

    /// <summary>
    /// Gets the <see cref="NamedScheme"/> that will be deleted when operation is run (see <see cref="Operation.Do"/>).
    /// This will be removed from <see cref="SchemeManager"/> and all users.
    /// </summary>
    public NamedScheme ToDelete { get; }

    /// <inheritdoc/>
    protected override void RedoImpl()
    {
        this.Do();
    }

    /// <inheritdoc/>
    protected override void UndoImpl()
    {
        foreach (var u in this.users)
        {
            // go back to using this explicit scheme before we deleted it
            u.State.OriginalScheme = this.ToDelete.Scheme;
            u.View.Scheme = this.ToDelete.Scheme;
        }

        SchemeManager.Instance.AddOrUpdateScheme(this.ToDelete.Name, this.ToDelete.Scheme, this.rootDesign);
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        foreach (var u in this.users)
        {
            // we are no longer using a custom scheme
            u.State.OriginalScheme = null;

            // we use the default (usually thats to inherit from parent)
            u.View.Scheme = this.GetDefaultScheme(u);
        }

        SchemeManager.Instance.Remove(this.ToDelete);
        return true;
    }

    private Scheme? GetDefaultScheme(Design d)
    {
        if (d.IsRoot)
        {
            switch (d.View)
            {
                case Dialog: return Colors.Schemes["Dialog"];
                case Window: return Colors.Schemes["Base"];
                default: return null;
            }
        }

        if (d.View is MenuBar)
        {
            
            return Colors.Schemes["Menu"];
        }

        return null;
    }
}