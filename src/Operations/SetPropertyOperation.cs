
using System.Reflection;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.Operations;

public class SetPropertyOperation : IOperation
{

    public Design Design { get; }
    public Property Property { get; }
    public object OldValue { get; }
    public object NewValue { get; }

    public SetPropertyOperation(Design design,Property property, object oldValue, object NewValue)
    {
        Design = design;
        Property = property;
        OldValue = oldValue;
        this.NewValue = NewValue;
    }


    /*
    /// <summary>
    /// Returns a <see cref="SnippetProperty"/> or the actual value of 
    /// <paramref name="p"/> on the <see cref="View"/>
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public virtual object GetDesignablePropertyValue(Property p)
    {
        if (SnippetProperties.ContainsKey(p.PropertyInfo))
        {
            return SnippetProperties[p.PropertyInfo];
        }

        return p.GetValue();
    }

    public void SetDesignablePropertyValue(PropertyInfo property, object? value)
    {

        if (value == null)
        {
            property.SetValue(View, null);
            return;
        }

        if (property.DeclaringType == typeof(TableStyle))
        {
            property.SetValue(((TableView)View).Style, value);
            return;
        }

        


        // todo do this properly with undo history and stuff
        property.SetValue(View, value.ToPrimitive());
    }
    */
    public void Do()
    {
        // if we are changing a value to a complex designed value type (e.g. Pos or Dim)
        if (Property is SnippetProperty snip)
        {
            Design.SetSnippetProperty(snip,NewValue);
            return;
        }

        Property.SetValue(NewValue);
    }

    public void Undo()
    {
        // if we are changing a value to a complex designed value type (e.g. Pos or Dim)
        if (Property is SnippetProperty snip)
        {
            Design.SetSnippetProperty(snip, OldValue);
            return;
        }

        Property.SetValue(OldValue);
    }

    public void Redo()
    {
        Do();
    }
}
