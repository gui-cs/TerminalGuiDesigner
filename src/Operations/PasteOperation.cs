using System.Drawing;

namespace TerminalGuiDesigner.Operations;

public class PasteOperation : Operation
{
    private Design to;
    private MultiSelectionManager _selectionManager;
    private IReadOnlyCollection<Design> _oldSelection;
    private List<AddViewOperation> _addOperations = new ();
    private List<Design> _clones = new ();


    public PasteOperation(Design addTo,MultiSelectionManager selectionManager)
    {
        IsImpossible = CopyOperation.LastCopiedDesign == null;
        to = addTo;
        _selectionManager = selectionManager;
        _oldSelection = selectionManager.Selected;
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
        
        _selectionManager.SetSelection(_clones.ToArray());
        return didAny;
    }

    private bool Paste(Design d)
    {
        
        var v = new ViewFactory();
        var clone = v.Create(d.View.GetType());

        var addOperation = new AddViewOperation(to.SourceCode,clone,to,null);
        
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

        _clones.Add(cloneDesign);
        
        // TODO: adjust X/Y etc to make clone more visible

        // TODO: Clone child designs too e.g. copy and paste a TabView
        return true;

    }

    public override void Undo()
    {
        foreach(var a in _addOperations)
            a.Undo();


        _selectionManager.SetSelection(_oldSelection.ToArray());
    }

    public override void Redo()
    {
        foreach(var a in _addOperations)
            a.Redo();

        _selectionManager.SetSelection(_clones.ToArray());
    }
}
