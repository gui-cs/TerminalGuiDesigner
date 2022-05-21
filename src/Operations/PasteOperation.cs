namespace TerminalGuiDesigner.Operations;

public class PasteOperation : Operation
{
    private Design to;
    private AddViewOperation? addOperation;

    public PasteOperation(Design addTo)
    {
        IsImpossible = CopyOperation.LastCopiedDesign == null;
        to = addTo;
    }

    public override bool Do()
    {
        var toCopy = CopyOperation.LastCopiedDesign;

        // if nothing to copy or calling Do() multiple times
        if(toCopy == null || addOperation != null)
            return false;

        var v = new ViewFactory();
        var clone = v.Create(toCopy.View.GetType());

        addOperation = new AddViewOperation(to.SourceCode,clone,to,null);
        
        // couldn't add for some reason
        if(!addOperation.Do())
            return false;
        
        var cloneDesign = clone.Data as Design ?? throw new Exception($"AddViewOperation did not result in View of type {clone.GetType()} having a Design");

        var cloneProps = cloneDesign.GetDesignableProperties();
        var copyProps = toCopy.GetDesignableProperties();

        foreach(var copyProp in copyProps)
        {
            var cloneProp = cloneProps.Single(p=>p.PropertyInfo == copyProp.PropertyInfo);
            cloneProp.SetValue(copyProp.GetValue());          
        }
        
        // TODO: adjust X/Y etc to make clone more visible

        // TODO: Clone child designs too e.g. copy and paste a TabView

        return true;
    }

    public override void Undo()
    {
        addOperation?.Undo();
    }

    public override void Redo()
    {
        addOperation?.Redo();
    }
}
