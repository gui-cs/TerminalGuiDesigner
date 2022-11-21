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
    private readonly Design[]? toCopy;

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
        this.toCopy = CopyOperation.LastCopiedDesign;
        this.toCopy = this.PruneChildViews(this.toCopy);

        this.IsImpossible = this.toCopy == null || this.toCopy.Length == 0;
        this.to = addTo;
        this.oldSelection = SelectionManager.Instance.Selected;

        // don't let user copy and paste a view into itself!
        if (this.toCopy?.Contains(this.to) ?? false)
        {
            this.IsImpossible = true;
        }

        // don't let user copy a container into one of its own child containers.
        if (this.toCopy?.Any(c => c.GetAllChildDesigns(c.View).Contains(this.to)) ?? false)
        {
            this.IsImpossible = true;
        }
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
        // if nothing to copy or calling Do() multiple times
        if (this.toCopy == null || this.addOperations.Any())
        {
            return false;
        }

        bool didAny = false;

        foreach (var d in this.toCopy)
        {
            didAny = this.Paste(d) || didAny;
        }

        this.MigratePosRelatives();

        var newSelection = this.PruneChildViews(this.clones.Values.ToArray());

        if (newSelection != null)
        {
            SelectionManager.Instance.ForceSetSelection(newSelection);
        }
        else
        {
            SelectionManager.Instance.Clear(false);
        }

        return didAny;
    }

    private bool Paste(Design d)
    {
        return this.Paste(d, this.to);
    }

    private void Paste(View copy, Design into)
    {
        // TODO this is going to miss 'drop through' View which themselves contain Data
        if (copy.Data is Design copyDesign)
        {
            this.Paste(copyDesign, into);
        }
        else
        {
            // its not a Design so its probably an artifact of Terminal.Gui e.g. ContentView etc
            // so we need to recurse into its children but not actually replicate this View
            foreach (var sub in copy.GetActualSubviews())
            {
                this.Paste(sub, into);
            }
        }
    }

    private bool Paste(Design copy, Design into)
    {
        var v = new ViewFactory();
        var clone = v.Create(copy.View.GetType());
        var addOperation = new AddViewOperation(clone, into, null);

        // couldn't add for some reason
        if (!addOperation.Do())
        {
            return false;
        }

        this.addOperations.Add(addOperation);

        var cloneDesign = clone.Data as Design ?? throw new Exception($"AddViewOperation did not result in View of type {clone.GetType()} having a Design");

        this.CopyProperties(copy, cloneDesign);


        if (clone is TabView tabView)
        {
            this.CloneTabView((TabView)copy.View, tabView);
        }
        else
        if (copy.IsContainerView)
        {
            foreach (var content in copy.View.GetActualSubviews())
            {
                this.Paste(content, cloneDesign);
            }
        }

        return true;
    }


    private void CopyProperties(Design from, Design toClone)
    {
        var cloneProps = toClone.GetDesignableProperties();
        var copyProps = from.GetDesignableProperties();

        foreach (var copyProp in copyProps)
        {
            var cloneProp = cloneProps.Single(p => p.PropertyInfo == copyProp.PropertyInfo);
            cloneProp.SetValue(copyProp.GetValue());
        }

        this.clones.Add(from, toClone);

        // If pasting a TableView make sure to
        // replicate the Table too.
        // TODO: think of a way to make this pattern

        // sustainable e.g. IPasteExtraBits or something
        if (from.View is TableView copyTv)
        {
            this.CloneTableView(copyTv, (TableView)toClone.View);
        }

        // TODO: adjust X/Y etc to make clone more visible

        // TODO: Clone child designs too e.g. copy and paste a TabView
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

    private void CloneTabView(TabView copy, TabView pasted)
    {
        // clear tabs in the pasted view as they will just come from ViewFactory
        foreach (var tab in pasted.Tabs.ToArray())
        {
            pasted.RemoveTab(tab);
        }

        // add a new Tab for each one in the source
        foreach (var copyTab in copy.Tabs)
        {
            var tab = pasted.AddEmptyTab(copyTab.Text?.ToString() ?? Operation.Unnamed);

            // copy the tab contents
            copy.SelectedTab = copyTab;
            pasted.SelectedTab = tab;

            foreach (var copySub in copyTab.View.GetActualSubviews())
            {
                this.Paste(copySub, (Design)pasted.Data);
            }
        }
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
