using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

public class NameProperty : Property
{
    public NameProperty(Design design) : base(design, typeof(Design).GetProperty("FieldName"))
    {

    }

    public override string ToString()
    {
        return $"(Name):{Design.FieldName}";
    }
    public override void SetValue(object value)
    {
        if (value == null)
        {
            throw new ArgumentException("Not Possible", "You cannot set a View (Name) property to null");
        }
        else
        {
            Design.FieldName = value.ToString();
        }
    }

    public override object GetValue()
    {
        return Design.FieldName;
    }
    protected override string GetLhs()
    {
        // Set View.Data to the name of the field so that we can 
        // determine later on which View instances come from which
        // Fields in the class

        return $"this.{Design.FieldName}.{nameof(View.Data)}";
    }
}
