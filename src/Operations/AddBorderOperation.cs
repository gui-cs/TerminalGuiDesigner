using Terminal.Gui;

namespace TerminalGuiDesigner.Operations;

public class AddBorderOperation : Operation
{
    private Border? _border;

    public Design Design { get; }

    public AddBorderOperation(Design design)
    {
        Design = design;
        
        if(Design.View.Border != null)
            IsImpossible = true;
    }


    public override void Do()
    {
        Design.View.Border = _border = new Border
        {
            Child = Design.View,
            BorderStyle = BorderStyle.Single,
        };

        Design.LoadBorderProperties();
    }

    public override void Redo()
    {
        Design.View.Border = _border;
    }

    public override void Undo()
    {
        Design.View.Border = null;
        Design.ClearBorderProperties();
    }
}
