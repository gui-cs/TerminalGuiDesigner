using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

public class ColorSchemeProperty : Property
{
    public ColorSchemeProperty(Design design):base(design,
        design.View.GetType().GetProperty(nameof(View.ColorScheme))
        ?? throw new Exception("ColorScheme property has changed name!"))
    {
        
    }

    public override void ToCode(CodeDomArgs args)
    {
        // if no explicit color scheme defined
        // then we don't output any code (view's
        // scheme is inherited)
        if(!Design.HasColorScheme() )
            return;

        base.ToCode(args);
    }

    protected override string GetHumanReadableValue()
    {
        // TODO : Summarise instead of BLAH
        return Design.HasColorScheme() ?
            "BLAH" : "Inherited";
    }
}
