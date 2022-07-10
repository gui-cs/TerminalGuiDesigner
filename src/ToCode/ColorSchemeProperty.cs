using System.CodeDom;
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

        // Note that this branch calls GetRhs()
        base.ToCode(args);
    }

    public override CodeExpression GetRhs()
    {
        var s  = GetValue() as ColorScheme;

        if(s == null)
            return new CodeDefaultValueExpression();

        var scheme = ColorSchemeManager.Instance.GetNameForColorScheme(s);

        if(scheme == null)
            return new CodeDefaultValueExpression();

        return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),scheme);
    }

    protected override string GetHumanReadableValue()
    {
        const string inherited =  "(Inherited)";

        if(!Design.HasColorScheme())
            return inherited;

        var s  = GetValue() as ColorScheme;

        if(s == null)
            return inherited;

        return ColorSchemeManager.Instance.GetNameForColorScheme(s) ?? "Unknown ColorScheme";
    }
}
