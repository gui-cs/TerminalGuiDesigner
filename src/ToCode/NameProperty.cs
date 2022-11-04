using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

public class NameProperty : Property
{
    public NameProperty(Design design) : base(
        design,
        typeof(Design).GetProperty(nameof(TerminalGuiDesigner.Design.FieldName))
        ?? throw new MissingFieldException("Expected property was missing from Design")
        )
    {
    }

    public override string GetHumanReadableName()
    {
        return "(Name)";
    }

    public override string ToString()
    {
        return $"{this.GetHumanReadableName()}:{this.Design.FieldName}";
    }

    public override void SetValue(object? value)
    {
        var chosen = value?.ToString();

        if (string.IsNullOrWhiteSpace(chosen))
        {
            throw new ArgumentException("Not Possible", "You cannot set a View (Name) property to null");
        }

        this.Design.FieldName = this.Design.GetUniqueFieldName(chosen);
    }

    public override object GetValue()
    {
        return this.Design.FieldName;
    }

    public override string GetLhs()
    {
        // Set View.Data to the name of the field so that we can
        // determine later on which View instances come from which
        // Fields in the class

        return $"this.{this.Design.FieldName}.{nameof(View.Data)}";
    }
}
