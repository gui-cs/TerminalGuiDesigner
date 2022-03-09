
using System.CodeDom;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using NLog;
using NStack;
using Terminal.Gui;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;
using static Terminal.Gui.TableView;

namespace TerminalGuiDesigner;

public class Design
{
    public SourceCodeFile SourceCode { get; }

    /// <summary>
    /// Name of the instance member field when the <see cref="View"/>
    /// is turned to code in a .Designer.cs file.  For example "label1"
    /// </summary>
    public string FieldName { get; set; }



    /// <summary>
    /// The view being designed.  Do not use <see cref="View.Add(Terminal.Gui.View)"/> on
    /// this instance.  Instead use <see cref="AddDesign(string, Terminal.Gui.View)"/> so that
    /// new child controls are preserved for design time changes
    /// </summary>
    public View View {get;}

    public Dictionary<PropertyInfo,SnippetProperty> SnippetProperties = new ();

    private Logger logger = LogManager.GetCurrentClassLogger();

    public Design(SourceCodeFile sourceCode, string fieldName, View view)
    {
        View = view;
        SourceCode = sourceCode;
        FieldName = fieldName;

        DeSerializeExtraProperties(fieldName);
    }

    public void CreateSubControlDesigns()
    {
        foreach (var subView in View.GetActualSubviews().ToArray())
        {
            logger.Info($"Found subView of Type '{subView.GetType()}'");

            if(subView.Data is string nameOrSerializedDesign)
            {
                subView.Data = CreateSubControlDesign(SourceCode,nameOrSerializedDesign, subView);
            }
        }
    }
    /// <summary>
    /// Removes all <see cref="SnippetProperties"/> that match the provided <paramref name="propertyName"/>.
    /// This will leave the Design drawing the value directly from the underlying <see cref="View"/> at
    /// code generation time.
    /// </summary>
    /// <param name="propertyName"></param>
    public void RemoveDesignedProperty(string propertyName)
    {
        foreach (var k in SnippetProperties.Keys.ToArray())
        {
            if (k.Name == propertyName)
            {
                SnippetProperties.Remove(k);
            }
        }
    }
    public Design CreateSubControlDesign(SourceCodeFile sourceCode, string nameOrSerializedDesign, View subView)
    {
        // HACK: if you don't pull the label out first it complains that you cant set Focusable to true
        // on the Label because its super is not focusable :(
        var super = subView.SuperView;
        if(super != null)
        {
            super.Remove(subView);
        }

        // all views can be focused so that they can be edited
        // or deleted etc
        subView.CanFocus = true;

        
        if (subView is TableView tv && tv.Table != null && tv.Table.Rows.Count == 0)
        {
            // add example rows so that it is easier to design the view
            for (int i = 0; i < 100; i++)
            {
                var row = tv.Table.NewRow();
                for (int c = 0; c < tv.Table.Columns.Count; c++)
                {
                    row[c] = $"{i},{c}";
                }
                tv.Table.Rows.Add(row);
            }
        }

        if (super != null)
        {
            super.Add(subView);
        }

        return new Design(sourceCode,nameOrSerializedDesign, subView);
    }


    /// <summary>
    /// Returns all designs in subviews of this control
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<Design> GetAllDesigns()
    {
        return GetAllDesigns(View);
    }


    /// <summary>
    /// Gets the designable properties of the hosted View
    /// </summary>
    public virtual IEnumerable<Property> GetDesignableProperties()
    {
        yield return new NameProperty(this);

        yield return new Property(this,View.GetActualTextProperty());

        yield return new Property(this, View.GetType().GetProperty(nameof(View.Width)));
        yield return new Property(this, View.GetType().GetProperty(nameof(View.Height)));

        yield return new Property(this, View.GetType().GetProperty(nameof(View.X)));
        yield return new Property(this, View.GetType().GetProperty(nameof(View.Y)));

        if(View is TableView tv)
        {
            yield return new Property(this, typeof(TableStyle).GetProperty(nameof(TableStyle.AlwaysShowHeaders)),nameof(TableView.Style),tv.Style);
        }
    }

    /// <summary>
    /// Adds declaration and initialization statements to the .Designer.cs
    /// CodeDom class.
    /// </summary>
    public void ToCode(CodeDomArgs args)
    {
        var toCode = new DesignToCode(this);
        toCode.ToCode(args);

    }
    public void DeSerializeExtraProperties(string fieldName)
    {
        var rosyln = new CodeToView(SourceCode);
        
        // no extra properties because we dont have a .Designer.cs! 
        // maybe we are half way through creating a new file pair or something
        if(!SourceCode.DesignerFile.Exists)
        {
            return;
        }

        foreach(var prop in GetDesignableProperties())
        {
            if(prop.PropertyInfo.PropertyType == typeof(Pos) || prop.PropertyInfo.PropertyType == typeof(Dim))
            {
                var rhsCode = rosyln.GetRhsCodeFor(this, fieldName, prop.PropertyInfo);

                // if there is no explicit setting of this property in the Designer.cs then who cares
                if (rhsCode == null)
                    continue;

                // theres some text in the .Designer.cs for this field so lets store that
                // that way we show "Dim.Bottom(myview)" instead of "Dim.Combine(Dim.Absolute(mylabel()), Dim.Absolute....) etc
                var value = prop.GetValue();
                SnippetProperties.Add(prop.PropertyInfo, new SnippetProperty(prop,rhsCode, value));
            }
        }
    }

    /// <summary>
    /// Returns all designable controls that are in the same container as this
    /// Does not include subcontainer controls etc.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Design> GetSiblings()
    {
        foreach(var v in View.SuperView.Subviews)
        {
            if(v == View)
            {
                continue;
            }

            if(v.Data is Design d)
            {
                yield return d;
            }
        }
    }



    private IEnumerable<Design> GetAllDesigns(View view)
    {
        List<Design> toReturn = new List<Design>();

        foreach (var subView in view.GetActualSubviews().ToArray())
        {
            if (subView.Data is Design d)
            {
                toReturn.Add(d);
            }

            // even if this subview isn't designable there might be designable ones further down
            // e.g. a ContentView of a Window
            toReturn.AddRange(GetAllDesigns(subView));
        }

        return toReturn;
    }

    public override string ToString()
    {
        return FieldName;
    }
}
