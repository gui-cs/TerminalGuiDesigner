using System.Reflection;
using NLog;
using Terminal.Gui;
using Terminal.Gui.Graphs;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using static Terminal.Gui.TableView;
using static Terminal.Gui.TabView;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

public class Design
{
    public SourceCodeFile SourceCode { get; }

    /// <summary>
    /// Name of the instance member field when the <see cref="View"/>
    /// is turned to code in a .Designer.cs file.  For example "label1"
    /// </summary>
    public string FieldName { get; set; }

    public bool IsRoot => FieldName.Equals(RootDesignName);

    public const string RootDesignName = "root";

    private List<Property> _designableProperties;

    private Logger logger = LogManager.GetCurrentClassLogger();

    public Property GetDesignableProperty(string propertyName)
    {
        return GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(propertyName));
    }


    /// <summary>
    /// The view being designed.  Do not use <see cref="View.Add(Terminal.Gui.View)"/> on
    /// this instance.  Instead use <see cref="AddDesign(string, Terminal.Gui.View)"/> so that
    /// new child controls are preserved for design time changes
    /// </summary>
    public View View {get;}
    public bool IsContainerView { get
        {
            // TODO: View and probably others (group box)?
            return View is TabView;
        }
    }


    public Design(SourceCodeFile sourceCode, string fieldName, View view)
    {
        View = view;
        SourceCode = sourceCode;
        FieldName = fieldName;

        _designableProperties = new List<Property>(LoadDesignableProperties());
    }

    public void CreateSubControlDesigns()
    {
        CreateSubControlDesigns(View);
    }

    private void CreateSubControlDesigns(View view)
    {
        foreach (var subView in view.GetActualSubviews().ToArray())
        {
            logger.Info($"Found subView of Type '{subView.GetType()}'");

            if(subView.Data is string name)
            {
                subView.Data = CreateSubControlDesign(SourceCode,name, subView);
            }
            
            CreateSubControlDesigns(subView);
        }
    }

    public Design CreateSubControlDesign(SourceCodeFile sourceCode, string name, View subView)
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

        return new Design(sourceCode,name, subView);
    }




    /// <summary>
    /// Gets the designable properties of the hosted View
    /// </summary>
    public IEnumerable<Property> GetDesignableProperties()
    {
        return _designableProperties;
    }

    protected virtual IEnumerable<Property> LoadDesignableProperties()
    {
        yield return new NameProperty(this);
        yield return new Property(this,View.GetActualTextProperty());
        
        yield return CreateProperty(nameof(View.Width));
        yield return CreateProperty(nameof(View.Height));

        yield return CreateProperty(nameof(View.X));
        yield return CreateProperty(nameof(View.Y));

        yield return CreateProperty(nameof(View.TextAlignment));

        if (View is Button)
        {
            yield return CreateProperty(nameof(Button.IsDefault));
        }
        if (View is CheckBox)
        {
            yield return CreateProperty(nameof(CheckBox.Checked));
        }
        if(View is GraphView gv)
        {
            yield return CreateProperty(nameof(GraphView.GraphColor));

            yield return CreateSubProperty(nameof(HorizontalAxis.Visible),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(HorizontalAxis.Increment),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(HorizontalAxis.ShowLabelsEvery),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(HorizontalAxis.Minimum),nameof(GraphView.AxisX),gv.AxisX);

            yield return CreateSubProperty(nameof(VerticalAxis.Visible),nameof(GraphView.AxisY),gv.AxisY);
            yield return CreateSubProperty(nameof(VerticalAxis.Increment),nameof(GraphView.AxisY),gv.AxisY);
            yield return CreateSubProperty(nameof(VerticalAxis.ShowLabelsEvery),nameof(GraphView.AxisY),gv.AxisY);
            yield return CreateSubProperty(nameof(VerticalAxis.Minimum),nameof(GraphView.AxisY),gv.AxisY);
        }

        if (View is Window)
        {
            yield return CreateProperty(nameof(Window.Title));
        }
        
        if (View is TableView tv)
        {
            yield return CreateSubProperty(nameof(TableStyle.AlwaysShowHeaders),nameof(TableView.Style),tv.Style);
            yield return CreateSubProperty(nameof(TableStyle.ExpandLastColumn), nameof(TableView.Style), tv.Style);
            yield return CreateSubProperty(nameof(TableStyle.InvertSelectedCellFirstCharacter), nameof(TableView.Style), tv.Style);
            yield return CreateSubProperty(nameof(TableStyle.ShowHorizontalHeaderOverline), nameof(TableView.Style), tv.Style);
            yield return CreateSubProperty(nameof(TableStyle.ShowHorizontalHeaderUnderline), nameof(TableView.Style), tv.Style);
            yield return CreateSubProperty(nameof(TableStyle.ShowVerticalCellLines), nameof(TableView.Style), tv.Style);
            yield return CreateSubProperty(nameof(TableStyle.ShowVerticalHeaderLines), nameof(TableView.Style), tv.Style);
        }

        
        if (View is TabView tabView)
        {
            yield return CreateProperty(nameof(TabView.MaxTabTextWidth));

            yield return CreateSubProperty(nameof(TabStyle.ShowBorder), nameof(TabView.Style), tabView.Style);
            yield return CreateSubProperty(nameof(TabStyle.ShowTopLine), nameof(TabView.Style), tabView.Style);
            yield return CreateSubProperty(nameof(TabStyle.TabsOnBottom), nameof(TabView.Style), tabView.Style);
        }

        if(View is RadioGroup radioGroup)
        {
            yield return CreateProperty(nameof(RadioGroup.RadioLabels));
        }
    }

    private Property CreateSubProperty(string name, string subObjectName, object subObject)
    {
        return new Property(this,subObject.GetType().GetProperty(name)        
            ?? throw new Exception($"Could not find expected Property '{name}' on Sub Object of Type '{subObject.GetType()}'")
            ,subObjectName,subObject);
    }

    private Property CreateProperty(string name)
    {
        return new Property(this,View.GetType().GetProperty(name)
            ?? throw new Exception($"Could not find expected Property '{name}' on View of Type '{View.GetType()}'"));
    }


    /// <summary>
    /// Returns all operations not to do with setting properties.  Often these
    /// are view specific e.g. add/remove column from a <see cref="TableView"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IOperation> GetExtraOperations()
    {
        if (View is TableView)
        {
            yield return new AddColumnOperation(this);
            yield return new RemoveColumnOperation(this);
            yield return new RenameColumnOperation(this);
        }


        if (View is TabView)
        {
            yield return new AddTabOperation(this);
            yield return new RemoveTabOperation(this);
            yield return new RenameTabOperation(this);
        }

    }

    /// <summary>
    /// Returns all designable controls that are in the same container as this
    /// Does not include subcontainer controls etc.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Design> GetSiblings()
    {
        // If there is no parent then we are likely the top rot Design
        // or an orphan.  Either way we have no siblings
        if(View.SuperView == null)
        {
            yield break;
        }

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

    /// <summary>
    /// Gets all Designs in the current scope.  Starting from the root
    /// parent of this on down.  Gets everyone... Everyone? EVERYONE!!!
    /// </summary>
    public IEnumerable<Design> GetAllDesigns()
    {
        var root = GetRootDesign();
        
        // Return the root design
        yield return root;
        
        // And all child designs
        foreach(var d in GetAllChildDesigns(root.View))
        {
            yield return d;
        }
    }

    /// <summary>
    /// Returns the topmost view above this which has an associated
    /// Design or this if there are no Design above this.
    /// <summary/>
    public Design GetRootDesign()
    {
        var toReturn = this;
        var v = View;

        while(v.SuperView != null)
        {
            v = v.SuperView;
            toReturn = v.Data as Design ?? toReturn;
        }

        return toReturn;
    }

    private IEnumerable<Design> GetAllChildDesigns(View view)
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
            toReturn.AddRange(GetAllChildDesigns(subView));
        }

        return toReturn;
    }

    public override string ToString()
    {
        return FieldName;
    }
}
