using System.Data;
using System.Text.RegularExpressions;
using NLog;
using Terminal.Gui;
using Terminal.Gui.Graphs;
using Terminal.Gui.Trees;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using static Terminal.Gui.TableView;
using static Terminal.Gui.TabView;

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

    private readonly List<Property> _designableProperties;

    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    public Property? GetDesignableProperty(string propertyName)
    {
        return GetDesignableProperties().SingleOrDefault(p=>p.PropertyInfo.Name.Equals(propertyName));
    }


    /// <summary>
    /// The view being designed.  Do not use <see cref="View.Add(Terminal.Gui.View)"/> on
    /// this instance.  Instead use <see cref="AddDesign(string, Terminal.Gui.View)"/> so that
    /// new child controls are preserved for design time changes
    /// </summary>
    public View View {get;}
    public bool IsContainerView { get
        {
            return View.IsContainerView();
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
                subView.Data = CreateSubControlDesign(SourceCode, name, subView);
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
                    row[c] = DBNull.Value;
                }
                tv.Table.Rows.Add(row);
            }
        }

        // To make the graph render correctly we need some content
        // either a Series or an Annotation
        if(subView is GraphView gv && gv.Series.Count == 0 && gv.Annotations.Count == 0)
        {
            // We don't have  either so add one
            gv.Annotations.Add(new TextAnnotation
            {
                ScreenPosition = new Point(1, 1),
                Text = ""
            });
        }
        if(subView is MenuBar mb)
        {        
            MenuTracker.Instance.Register(mb);
        }

        if(subView is TreeView tree)
        {
            tree.AddObject(new TreeNode("Example Branch 1"){
                Children = new []{new TreeNode("Child 1")}
            });
            tree.AddObject(new TreeNode("Example Branch 2"){
                Children = new []{
                    new TreeNode("Child 1"),
                    new TreeNode("Child 2")}
            });
            
            for (int l = 0; l < 20; l++)
                tree.AddObject(new TreeNode($"Example Leaf {l}"));
        }

        if (super != null)
        {
            super.Add(subView);
        }

        return new Design(sourceCode,name, subView);
    }



    /// <summary>
    /// Returns true if there is an explicit ColorScheme set
    /// on this Design's View or false if it is inherited from
    /// a View further up the Layout (or a library default scheme)
    /// <para> If a scheme is found that is not known about by ColorSchemeManager
    /// then false is returned </para>
    /// </summary>
    public bool HasKnownColorScheme()
    {
        var userDefinedColorScheme = MultiSelectionManager.Instance.GetOriginalExplicitColorScheme(this) ?? View.GetExplicitColorScheme();

        if(userDefinedColorScheme == null)
            return false;

        // theres a color scheme defined but we aren't tracking it
        // so report it as inherited since it must have got it from
        // the API somehow
        if(Colors.ColorSchemes.Values.Contains(userDefinedColorScheme))
            return false;

        // it has a ColorScheme but not one we are tracking
        if(ColorSchemeManager.Instance.GetNameForColorScheme(userDefinedColorScheme) == null)
            return false;

        return true;
    }


    /// <summary>
    /// True if this view EXPLICITLY states that it uses the scheme
    /// False if its scheme is inherited from a parent or it explicitly
    /// uses a different ColorScheme
    /// </summary>
    public bool UsesColorScheme(ColorScheme scheme)
    {
        return HasKnownColorScheme() && View.ColorScheme.AreEqual(scheme);
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
        yield return CreateProperty(nameof(View.Width));

        if(AllowChangingHeight())
            yield return CreateProperty(nameof(View.Height));

        yield return CreateProperty(nameof(View.X));
        yield return CreateProperty(nameof(View.Y));


        yield return new ColorSchemeProperty(this);

        // its important that this comes before Text because
        // changing the validator clears the text
        if(View is TextValidateField)
        {
            yield return CreateProperty(nameof(TextValidateField.Provider));
        }
        if (View is TextField)
        {
            yield return CreateProperty(nameof(TextField.Secret));
        }
        if (View is TextView)
        {
            yield return CreateProperty(nameof(TextView.AllowsTab));
            yield return CreateProperty(nameof(TextView.AllowsReturn));
            yield return CreateProperty(nameof(TextView.WordWrap));
        }

        if(View is Toplevel)
        {
            yield return CreateProperty(nameof(Toplevel.Modal));
        }

        // Allow changing the FieldName on anything but root where 
        // such an action would break things badly
        if(!this.IsRoot)
            yield return new NameProperty(this);

        yield return new Property(this,View.GetActualTextProperty());

        // Border properties - Most views dont have a border so Border is
        if(View.Border != null)
        {
            yield return CreateSubProperty(nameof(Border.DrawMarginFrame), nameof(View.Border), View.Border);
            yield return CreateSubProperty(nameof(Border.BorderStyle),nameof(View.Border),View.Border);
            yield return CreateSubProperty(nameof(Border.Effect3D),nameof(View.Border),View.Border);            
        }
        
        yield return CreateProperty(nameof(View.TextAlignment));

        if (View is Button)
        {
            yield return CreateProperty(nameof(Button.IsDefault));
        }
        if(View is LineView)
        {
            yield return CreateProperty(nameof(LineView.LineRune));
            yield return CreateProperty(nameof(LineView.Orientation));
        }
        if(View is ProgressBar)
        {
            yield return CreateProperty(nameof(ProgressBar.Fraction));
            yield return CreateProperty(nameof(ProgressBar.BidirectionalMarquee));
            yield return CreateProperty(nameof(ProgressBar.ProgressBarStyle));
            yield return CreateProperty(nameof(ProgressBar.ProgressBarFormat));
            yield return CreateProperty(nameof(ProgressBar.SegmentCharacter));
        }
        if (View is CheckBox)
        {
            yield return CreateProperty(nameof(CheckBox.Checked));
        }

        if (View is ListView lv)
        {
            yield return CreateProperty(nameof(ListView.Source));
        }
        if(View is GraphView gv)
        {
            yield return CreateProperty(nameof(GraphView.GraphColor));
            yield return CreateProperty(nameof(GraphView.ScrollOffset));
            yield return CreateProperty(nameof(GraphView.MarginLeft));
            yield return CreateProperty(nameof(GraphView.MarginBottom));
            yield return CreateProperty(nameof(GraphView.CellSize));

            yield return CreateSubProperty(nameof(HorizontalAxis.Visible),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(HorizontalAxis.Increment),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(HorizontalAxis.ShowLabelsEvery),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(HorizontalAxis.Minimum),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(HorizontalAxis.Text),nameof(GraphView.AxisX),gv.AxisX);
            yield return CreateSubProperty(nameof(VerticalAxis.Visible),nameof(GraphView.AxisY),gv.AxisY);
            yield return CreateSubProperty(nameof(VerticalAxis.Increment),nameof(GraphView.AxisY),gv.AxisY);
            yield return CreateSubProperty(nameof(VerticalAxis.ShowLabelsEvery),nameof(GraphView.AxisY),gv.AxisY);
            yield return CreateSubProperty(nameof(VerticalAxis.Minimum),nameof(GraphView.AxisY),gv.AxisY);
            yield return CreateSubProperty(nameof(VerticalAxis.Text),nameof(GraphView.AxisY),gv.AxisY);
        }

        if (View is Window)
        {
            yield return CreateProperty(nameof(Window.Title));
        }

        if(View is TreeView tree)
        {
            yield return CreateSubProperty(nameof(TreeStyle.CollapseableSymbol),nameof(TreeView<ITreeNode>.Style),tree.Style);
            yield return CreateSubProperty(nameof(TreeStyle.ColorExpandSymbol),nameof(TreeView<ITreeNode>.Style),tree.Style);
            yield return CreateSubProperty(nameof(TreeStyle.ExpandableSymbol),nameof(TreeView<ITreeNode>.Style),tree.Style);
            yield return CreateSubProperty(nameof(TreeStyle.InvertExpandSymbolColors),nameof(TreeView<ITreeNode>.Style),tree.Style);
            yield return CreateSubProperty(nameof(TreeStyle.LeaveLastRow),nameof(TreeView<ITreeNode>.Style),tree.Style);
            yield return CreateSubProperty(nameof(TreeStyle.ShowBranchLines),nameof(TreeView<ITreeNode>.Style),tree.Style);
        }
        
        if (View is TableView tv)
        {

            yield return CreateProperty(nameof(TableView.FullRowSelect));

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

        if(View is RadioGroup)
        {
            yield return CreateProperty(nameof(RadioGroup.RadioLabels));
        }
    }

    private bool AllowChangingHeight()
    {
        // don't support multi line buttons
        if(View is Button)
            return false;

        return true; 
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
        return GetExtraOperations(Point.Empty);
    }
    /// <summary>
    /// Returns one off atomic activities that can be performed on the view e.g. 'add a column'.
    /// </summary>
    /// <param name="pos">If this was triggered by a click then the position of the click
    /// may inform what operations are returned (e.g. right clicking a specific table view column).  Otherwise
    /// <see cref="Point.Empty"/></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal IEnumerable<IOperation> GetExtraOperations(Point pos)
    {
        yield return new CopyOperation(this);

        if (View is TableView tv)
        {
            DataColumn? col = null;
            if(!pos.IsEmpty)
            {
                // TODO: Currently you have to right click in the row (body) of the table
                // and cannot right click the headers themselves
                var cell = tv.ScreenToCell(pos.X, pos.Y);
                if (cell != null)
                    col = tv.Table.Columns[cell.Value.X];
            }

            yield return new AddColumnOperation(this);
            yield return new RemoveColumnOperation(this, col);
            yield return new RenameColumnOperation(this, col);
        }

        if(IsContainerView || IsRoot)
        {
            yield return new AddViewOperation(SourceCode,this);
            yield return new PasteOperation(this);
        }
        else
        {
            var nearestContainer = this.View.GetNearestContainerDesign();
            if(nearestContainer != null)
                yield return new AddViewOperation(SourceCode,nearestContainer);
        }

        yield return new DeleteViewOperation(this.View);

        if (View is TabView)
        {
            yield return new AddTabOperation(this);
            yield return new RemoveTabOperation(this);
            yield return new RenameTabOperation(this);

            yield return new MoveTabOperation(this, -1);
            yield return new MoveTabOperation(this, 1);
        }

        if(View is MenuBar)
        {
            yield return new AddMenuOperation(this,null);
            yield return new RemoveMenuOperation(this);
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

            if(v.Data is Design d)
            {
                if(d.IsRoot)
                    return d;

                toReturn = d;
            }
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

    /// <summary>
    /// Returns a new unique name for a view of type <paramref name="viewType"/>
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="viewType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GetUniqueFieldName(Type viewType)
    {
        var root = GetRootDesign();

        var allDesigns = root.GetAllDesigns();

        // consider label1
        int number = 1;
        while (allDesigns.Any(d => d.FieldName.Equals($"{viewType.Name.ToLower()}{number}")))
        {
            // label1 is taken, try label2 etc
            number++;
        }

        // found a unique one
        return $"{viewType.Name.ToLower()}{number}";
    }

    public string GetUniqueFieldName(string candidate)
    {
        var root = GetRootDesign();

        // remove problematic characters
        candidate = CodeDomArgs.MakeValidFieldName(candidate);

        var allDesigns = root.GetAllDesigns().ToList();
        allDesigns.Remove(this);

        // what field names are already taken by other objects?
        var usedFieldNames = allDesigns.Select(d => d.FieldName).ToList();
        usedFieldNames.AddRange(ColorSchemeManager.Instance.Schemes.Select(k=>k.Name));

        // if name is already unique thats great
        if(!usedFieldNames.Contains(candidate))
        {
            return candidate;
        }

        // name collides with something else
        int number = 2;
        while (usedFieldNames.Contains($"{candidate}{number}"))
        {
            // bob is taken, try bob2 etc
            number++;
        }

        // found a unique one
        return $"{candidate}{number}";
    }

    /// <summary>
    /// Returns all <see cref="Design"/> that have dependency on this
    /// </summary>
    public IEnumerable<Design> GetDependantDesigns()
    {
        var everyone = GetAllDesigns().ToArray();
        return everyone.Where(o=>DependsOnUs(o,everyone));
    }

    private bool DependsOnUs(Design other, Design[] everyone)
    {
        // obviously we cannot depend on ourselves
        if (other == this)
            return false;

        // if their X depends on us
        if(other.View.X.GetPosType(everyone,out _,out _, out var relativeTo,out _,out _))
        {
            if (relativeTo == this)
                return true;
        }

        // if their Y depends on us
        if (other.View.Y.GetPosType(everyone, out _, out _, out relativeTo, out _, out _))
        {
            if (relativeTo == this)
                return true;
        }

        return false;
    }
}
