using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Creates new copies of all <see cref="Design"/> captured by a <see cref="CopyOperation"/>.
/// </summary>
public class PasteOperation : Operation
{
    private readonly Design to;
    private readonly IReadOnlyCollection<Design> oldSelection;
    private readonly List<AddViewOperation> addOperations = new();

    /// <summary>
    /// Mapping from old (Key) Views to new cloned Views (Value).
    /// </summary>
    private readonly Dictionary<Design, Design> clones = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PasteOperation"/> class.
    /// </summary>
    /// <param name="addTo">The container <see cref="Design"/> into which to
    /// add the <see cref="Design"/>.  This allows for copying from one container
    /// (e.g. <see cref="TabView"/>) but pasting into another.</param>
    public PasteOperation(Design addTo)
    {
        this.IsImpossible = CopyOperation.LastCopiedDesign == null;
        this.to = addTo;
        this.oldSelection = SelectionManager.Instance.Selected;
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        foreach (var a in this.addOperations)
        {
            a.Undo();
        }

        SelectionManager.Instance.SetSelection(this.oldSelection.ToArray());
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        foreach (var a in this.addOperations)
        {
            a.Redo();
        }

        SelectionManager.Instance.SetSelection(this.clones.Values.ToArray());
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        var toCopy = CopyOperation.LastCopiedDesign;

        // if nothing to copy or calling Do() multiple times
        if (toCopy == null || this.addOperations.Any())
        {
            return false;
        }

        bool didAny = false;

        foreach (var d in toCopy)
        {
            didAny = this.Paste(d) || didAny;
        }

        this.MigratePosRelatives();

        SelectionManager.Instance.ForceSetSelection(this.clones.Values.ToArray());
        return didAny;
    }

    private bool Paste(Design d)
    {
        var v = new ViewFactory();
        var clone = v.Create(d.View.GetType());

        var addOperation = new AddViewOperation(clone, this.to, null);

        // couldn't add for some reason
        if (!addOperation.Do())
        {
            return false;
        }

        this.addOperations.Add(addOperation);

        var cloneDesign = clone.Data as Design ?? throw new Exception($"AddViewOperation did not result in View of type {clone.GetType()} having a Design");

        var cloneProps = cloneDesign.GetDesignableProperties();
        var copyProps = d.GetDesignableProperties();

        foreach (var copyProp in copyProps)
        {
            var cloneProp = cloneProps.Single(p => p.PropertyInfo == copyProp.PropertyInfo);
            cloneProp.SetValue(copyProp.GetValue());
        }

        this.clones.Add(d, cloneDesign);

        // If pasting a TableView make sure to
        // replicate the Table too.
        // TODO: think of a way to make this pattern

        // sustainable e.g. IPasteExtraBits or something
        if (d.View is TableView copyTv)
        {
            this.CloneTableView(copyTv, (TableView)cloneDesign.View);
        }

        // TODO: adjust X/Y etc to make clone more visible

        // TODO: Clone child designs too e.g. copy and paste a TabView
        return true;
    }

    private void CloneTableView(TableView copy, TableView pasted)
    {
        pasted.Table = copy.Table.Clone();

        foreach (DataRow row in copy.Table.Rows)
        {
            pasted.Table.Rows.Add(row.ItemArray);
        }

        pasted.Update();
    }

    private void MigratePosRelatives()
    {
        var everyone = this.to.GetAllDesigns().ToArray();

        foreach (var kvp in this.clones)
        {
            var pasted = kvp.Value;

            pasted.View.X = this.MigrateIfPosRelative(pasted.View.X, everyone);
            pasted.View.Y = this.MigrateIfPosRelative(pasted.View.Y, everyone);
        }
    }

    private Pos MigrateIfPosRelative(Pos pos, Design[] allDesigns)
    {
        pos.GetPosType(
            allDesigns,
            out var type,
            out _,
            out var relativeTo,
            out var side,
            out var offset);

        // not relative so jump out early, no need to update it
        if (type != PosType.Relative || relativeTo == null)
        {
            return pos;
        }

        // we are copy/pasting a relative Pos but copied selection
        // does not include the relativeTo so we cannot update
        // to the new instance
        if (!this.clones.ContainsKey(relativeTo))
        {
            return pos;
        }

        // create a new PosRelative that is pointed at the pasted clone
        // instead of the original
        return PosExtensions.CreatePosRelative(this.clones[relativeTo], side, offset);
    }
}
