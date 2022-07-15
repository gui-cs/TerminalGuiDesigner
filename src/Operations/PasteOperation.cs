using System.Data;
using System.Drawing;
using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class PasteOperation : Operation
{
    private Design _to;
    private IReadOnlyCollection<Design> _oldSelection;
    private List<AddViewOperation> _addOperations = new ();

    /// <summary>
    /// Mapping from old (Key) Views to new cloned Views (Value)
    /// </summary>
    private Dictionary<Design,Design> _clones = new ();


    public PasteOperation(Design addTo)
    {
        IsImpossible = CopyOperation.LastCopiedDesign == null;
        _to = addTo;
        _oldSelection = MultiSelectionManager.Instance.Selected;
    }

    public override bool Do()
    {
        var toCopy = CopyOperation.LastCopiedDesign;

        // if nothing to copy or calling Do() multiple times
        if(toCopy == null || _addOperations.Any())
            return false;

        bool didAny = false;

        foreach(var d in toCopy)
        {
            didAny = Paste(d) || didAny;
        }

        MigratePosRelatives();

        MultiSelectionManager.Instance.SetSelection(_clones.Values.ToArray());
        return didAny;
    }

    private bool Paste(Design d)
    {
        
        var v = new ViewFactory();
        var clone = v.Create(d.View.GetType());

        var addOperation = new AddViewOperation(_to.SourceCode,clone,_to,null);
        
        // couldn't add for some reason
        if(!addOperation.Do())
            return false;

        _addOperations.Add(addOperation);
        
        var cloneDesign = clone.Data as Design ?? throw new Exception($"AddViewOperation did not result in View of type {clone.GetType()} having a Design");

        var cloneProps = cloneDesign.GetDesignableProperties();
        var copyProps = d.GetDesignableProperties();

        foreach(var copyProp in copyProps)
        {
            var cloneProp = cloneProps.Single(p=>p.PropertyInfo == copyProp.PropertyInfo);
            cloneProp.SetValue(copyProp.GetValue());          
        }

        _clones.Add(d,cloneDesign);

        // If pasting a TableView make sure to 
        // replicate the Table too.  
        // TODO: think of a way to make this pattern

        // sustainable e.g. IPasteExtraBits or something
        if (d.View is TableView copyTv)
        {
            CloneTableView(copyTv, (TableView)cloneDesign.View);
        }
                
        // TODO: adjust X/Y etc to make clone more visible

        // TODO: Clone child designs too e.g. copy and paste a TabView
        return true;

    }

    private void CloneTableView(TableView copy, TableView pasted)
    {
        pasted.Table = copy.Table.Clone();

        foreach(DataRow row in copy.Table.Rows)
        {
            pasted.Table.Rows.Add(row.ItemArray);
        }
        pasted.Update();
    }

    public override void Undo()
    {
        foreach(var a in _addOperations)
            a.Undo();


        MultiSelectionManager.Instance.SetSelection(_oldSelection.ToArray());
    }

    public override void Redo()
    {
        foreach(var a in _addOperations)
            a.Redo();

        MultiSelectionManager.Instance.SetSelection(_clones.Values.ToArray());
    }
    private void MigratePosRelatives()
    {
        var everyone = _to.GetAllDesigns().ToArray();

        foreach (var kvp in _clones)
        {
            var pasted = kvp.Value;

            pasted.View.X = MigrateIfPosRelative(pasted.View.X, everyone);
            pasted.View.Y = MigrateIfPosRelative(pasted.View.Y, everyone);
        }
    }

    private Pos MigrateIfPosRelative(Pos pos, Design[] allDesigns)
    {
        pos.GetPosType(allDesigns,
            out var type, out _, out var relativeTo, out var side, out var offset);

        // not relative so jump out early, no need to update it
        if (type != PosType.Relative || relativeTo == null)
        {
            return pos;
        }
        
        // we are copy/pasting a relative Pos but copied selection
        // does not include the relativeTo so we cannot update
        // to the new instance
        if(!_clones.ContainsKey(relativeTo))
        {
            return pos;
        }

        // create a new PosRelative that is pointed at the pasted clone
        // instead of the original
        return PosExtensions.CreatePosRelative(_clones[relativeTo], side, offset);
    }
}
