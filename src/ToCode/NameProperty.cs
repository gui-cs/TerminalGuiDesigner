using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

public class NameProperty : Property
{
    public NameProperty(Design design) : base(design, 
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
        return $"{GetHumanReadableName()}:{Design.FieldName}";
    }
    public override void SetValue(object? value)
    {
        if (value == null)
        {
            throw new ArgumentException("Not Possible", "You cannot set a View (Name) property to null");
        }
        
        Design.FieldName = value.ToString() ?? throw new Exception($"ToString returned null for value of Type {value.GetType().Name}");
        
    }

    public override object GetValue()
    {
        return Design.FieldName;
    }
    public override string GetLhs()
    {
        // Set View.Data to the name of the field so that we can 
        // determine later on which View instances come from which
        // Fields in the class

        return $"this.{Design.FieldName}.{nameof(View.Data)}";
    }
}
