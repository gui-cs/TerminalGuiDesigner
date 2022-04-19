namespace TerminalGuiDesigner;

public static class AttributeExtensions
{
    public static string ToCode(this Terminal.Gui.Attribute a)
    {
        return $"Terminal.Gui.Attribute.Make(Color.{a.Foreground},Color.{a.Background})";
    }
}
