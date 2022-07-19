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
        if(!Design.HasKnownColorScheme() )
            return;

        // Note that this branch calls GetRhs()
        base.ToCode(args);
    }

    public override CodeExpression GetRhs()
    {
        var s  = GetValue() as ColorScheme;

        var name = ColorSchemeManager.Instance.GetNameForColorScheme(s 
            ?? throw new Exception("GetRhs is only valid when there is a known ColorScheme"));

        if(string.IsNullOrWhiteSpace(name))
            throw new Exception("GetRhs is only valid when there is a known ColorScheme");

        return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),name);
    }
    public override object? GetValue()
    {
        if (SelectionManager.Instance.Selected.Contains(Design))
            return SelectionManager.Instance.GetOriginalExplicitColorScheme(Design);

        return Design.View.GetExplicitColorScheme();
    }
    protected override string GetHumanReadableValue()
    {
        const string inherited =  "(Inherited)";

        if(!Design.HasKnownColorScheme())
            return inherited;

        var s  = GetValue() as ColorScheme;

        if(s == null)
            return inherited;

        return ColorSchemeManager.Instance.GetNameForColorScheme(s) ?? "Unknown ColorScheme";
    }
}
