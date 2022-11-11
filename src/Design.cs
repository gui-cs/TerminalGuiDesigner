using System.Data;
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

    public bool IsRoot => this.FieldName.Equals(RootDesignName);

    public const string RootDesignName = "root";

    private readonly List<Property> designableProperties;

    public DesignState State { get; }

    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    public Property? GetDesignableProperty(string propertyName)
    {
        return this.GetDesignableProperties().SingleOrDefault(p => p.PropertyInfo.Name.Equals(propertyName));
    }

    /// <summary>
    /// The view being designed.  Do not use <see cref="View.Add(Terminal.Gui.View)"/> on
    /// this instance.  Instead use <see cref="AddDesign(string, Terminal.Gui.View)"/> so that
    /// new child controls are preserved for design time changes
    /// </summary>
    public View View { get; }

    public bool IsContainerView => this.View.IsContainerView();

    public bool IsBorderlessContainerView => this.View.IsBorderlessContainerView();

    public Design(SourceCodeFile sourceCode, string fieldName, View view)
    {
        this.View = view;
        this.SourceCode = sourceCode;
        this.FieldName = fieldName;

        this.designableProperties = new List<Property>(this.LoadDesignableProperties());
        this.State = new DesignState(this);
    }

    public void CreateSubControlDesigns()
    {
        // Unlike Window/Dialog the View/TopLevel classes do not have an explicit
        // colors schemes.  When creating a new View or TopLevel we need to use
        // the Colors.Base and fiddle a bit with coloring/clearing to ensure things render correctly

        var baseType = this.View.GetType().BaseType;

        if (baseType == typeof(View) || baseType == typeof(Toplevel))
        {
            if (this.View.ColorScheme == null || this.View.ColorScheme == Colors.TopLevel)
            {
                this.State.OriginalScheme = this.View.ColorScheme = Colors.Base;
            }

            // TODO: Remove this when https://github.com/gui-cs/Terminal.Gui/issues/2094 is fixed
            // HACK

            // View and TopLevel doe not clear their states regularly during drawing
            // we have to do that ourselves
            this.View.DrawContent += (r) =>
            {
                // manually erase stale content
                Application.Driver.SetAttribute(
                    this.State.OriginalScheme?.Normal ??
                    this.View.ColorScheme.Normal);
                this.View.Clear();
            };
        }

        this.CreateSubControlDesigns(this.View);
    }

    private void CreateSubControlDesigns(View view)
    {
        foreach (var subView in view.GetActualSubviews().ToArray())
        {
            this.logger.Info($"Found subView of Type '{subView.GetType()}'");

            if (subView.Data is string name)
            {
                subView.Data = this.CreateSubControlDesign(this.SourceCode, name, subView);
            }

            this.CreateSubControlDesigns(subView);
        }
    }

    public Design CreateSubControlDesign(SourceCodeFile sourceCode, string name, View subView)
    {
        // HACK: if you don't pull the label out first it complains that you cant set Focusable to true
        // on the Label because its super is not focusable :(
        var super = subView.SuperView;
        if (super != null)
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
        if (subView is GraphView gv && gv.Series.Count == 0 && gv.Annotations.Count == 0)
        {
            // We don't have  either so add one
            gv.Annotations.Add(new TextAnnotation
            {
                ScreenPosition = new Point(1, 1),
                Text = string.Empty,
            });
        }

        if (subView is MenuBar mb)
        {
            MenuTracker.Instance.Register(mb);
        }

        if (subView is CheckBox cb)
        {
            this.RegisterCheckboxDesignTimeChanges(cb);
        }

        if (subView is TextView txt)
        {
            // prevent control from responding to events
            txt.MouseClick += this.SuppressNativeClickEvents;
            txt.KeyDown += (s) => s.Handled = true;
        }

        if (subView is TextField tf)
        {
            // prevent control from responding to events
            tf.MouseClick += this.SuppressNativeClickEvents;
            tf.KeyDown += (s) => s.Handled = true;
        }

        if (subView is TreeView tree)
        {
            tree.AddObject(new TreeNode("Example Branch 1")
            {
                Children = new[] { new TreeNode("Child 1") },
            });
            tree.AddObject(new TreeNode("Example Branch 2")
            {
                Children = new[]
                {
                    new TreeNode("Child 1"),
                    new TreeNode("Child 2"),
                },
            });

            for (int l = 0; l < 20; l++)
            {
                tree.AddObject(new TreeNode($"Example Leaf {l}"));
            }
        }

        if (super != null)
        {
            super.Add(subView);
        }

        var d = new Design(sourceCode, name, subView);
        return d;
    }

    private void SuppressNativeClickEvents(View.MouseEventArgs obj)
    {
        // Suppress everything except single click (selection)
        obj.Handled = obj.MouseEvent.Flags != MouseFlags.Button1Clicked;
    }

    private void RegisterCheckboxDesignTimeChanges(CheckBox cb)
    {
        // prevent space toggling the checkbox
        // (gives better typing experience e.g. "my lovely checkbox")
        cb.ClearKeybinding(Key.Space);
        cb.MouseClick += (e) =>
        {
            if (e.MouseEvent.Flags.HasFlag(MouseFlags.Button1Clicked))
            {
                e.Handled = true;
                cb.SetFocus();
            }
        };
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
        var userDefinedColorScheme = this.State.OriginalScheme ?? this.View.GetExplicitColorScheme();

        if (userDefinedColorScheme == null)
        {
            return false;
        }

        // theres a color scheme defined but we aren't tracking it
        // so report it as inherited since it must have got it from
        // the API somehow
        if (Colors.ColorSchemes.Values.Contains(userDefinedColorScheme))
        {
            return false;
        }

        // it has a ColorScheme but not one we are tracking
        if (ColorSchemeManager.Instance.GetNameForColorScheme(userDefinedColorScheme) == null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// True if this view EXPLICITLY states that it uses the scheme
    /// False if its scheme is inherited from a parent or it explicitly
    /// uses a different ColorScheme
    /// </summary>
    public bool UsesColorScheme(ColorScheme scheme)
    {
        // we use this scheme if it is a known scheme
        return this.HasKnownColorScheme() &&
            (this.View.ColorScheme.AreEqual(scheme) || (this.State.OriginalScheme?.AreEqual(scheme) ?? false));
    }

    /// <summary>
    /// Gets the designable properties of the hosted View
    /// </summary>
    public IEnumerable<Property> GetDesignableProperties()
    {
        return this.designableProperties;
    }

    protected virtual IEnumerable<Property> LoadDesignableProperties()
    {
        yield return this.CreateProperty(nameof(this.View.Width));
        yield return this.CreateProperty(nameof(this.View.Height));

        yield return this.CreateProperty(nameof(this.View.X));
        yield return this.CreateProperty(nameof(this.View.Y));

        yield return new ColorSchemeProperty(this);

        // its important that this comes before Text because
        // changing the validator clears the text
        if (this.View is TextValidateField)
        {
            yield return this.CreateProperty(nameof(TextValidateField.Provider));
        }

        if (this.View is TextField)
        {
            yield return this.CreateProperty(nameof(TextField.Secret));
        }

        if (this.View is ScrollView)
        {
            yield return this.CreateProperty(nameof(ScrollView.ContentSize));
        }

        if (this.View is TextView)
        {
            yield return this.CreateProperty(nameof(TextView.AllowsTab));
            yield return this.CreateProperty(nameof(TextView.AllowsReturn));
            yield return this.CreateProperty(nameof(TextView.WordWrap));
        }

        if (this.View is Toplevel)
        {
            yield return this.CreateProperty(nameof(Toplevel.Modal));
        }

        // Allow changing the FieldName on anything but root where
        // such an action would break things badly
        if (!this.IsRoot)
        {
            yield return new NameProperty(this);
        }

        yield return new Property(this, this.View.GetActualTextProperty());

        // Border properties - Most views dont have a border so Border is
        if (this.View.Border != null)
        {
            yield return this.CreateSubProperty(nameof(Border.BorderStyle), nameof(this.View.Border), this.View.Border);
            yield return this.CreateSubProperty(nameof(Border.BorderBrush), nameof(this.View.Border), this.View.Border);
            yield return this.CreateSubProperty(nameof(Border.Effect3D), nameof(this.View.Border), this.View.Border);
            yield return this.CreateSubProperty(nameof(Border.Effect3DBrush), nameof(this.View.Border), this.View.Border);
            yield return this.CreateSubProperty(nameof(Border.DrawMarginFrame), nameof(this.View.Border), this.View.Border);
        }

        yield return this.CreateProperty(nameof(this.View.TextAlignment));

        if (this.View is Button)
        {
            yield return this.CreateProperty(nameof(Button.IsDefault));
        }

        if (this.View is LineView)
        {
            yield return this.CreateProperty(nameof(LineView.LineRune));
            yield return this.CreateProperty(nameof(LineView.Orientation));
        }

        if (this.View is ProgressBar)
        {
            yield return this.CreateProperty(nameof(ProgressBar.Fraction));
            yield return this.CreateProperty(nameof(ProgressBar.BidirectionalMarquee));
            yield return this.CreateProperty(nameof(ProgressBar.ProgressBarStyle));
            yield return this.CreateProperty(nameof(ProgressBar.ProgressBarFormat));
            yield return this.CreateProperty(nameof(ProgressBar.SegmentCharacter));
        }

        if (this.View is CheckBox)
        {
            yield return this.CreateProperty(nameof(CheckBox.Checked));
        }

        if (this.View is ListView lv)
        {
            yield return this.CreateProperty(nameof(ListView.Source));
        }

        if (this.View is GraphView gv)
        {
            yield return this.CreateProperty(nameof(GraphView.GraphColor));
            yield return this.CreateProperty(nameof(GraphView.ScrollOffset));
            yield return this.CreateProperty(nameof(GraphView.MarginLeft));
            yield return this.CreateProperty(nameof(GraphView.MarginBottom));
            yield return this.CreateProperty(nameof(GraphView.CellSize));

            yield return this.CreateSubProperty(nameof(HorizontalAxis.Visible), nameof(GraphView.AxisX), gv.AxisX);
            yield return this.CreateSubProperty(nameof(HorizontalAxis.Increment), nameof(GraphView.AxisX), gv.AxisX);
            yield return this.CreateSubProperty(nameof(HorizontalAxis.ShowLabelsEvery), nameof(GraphView.AxisX), gv.AxisX);
            yield return this.CreateSubProperty(nameof(HorizontalAxis.Minimum), nameof(GraphView.AxisX), gv.AxisX);
            yield return this.CreateSubProperty(nameof(HorizontalAxis.Text), nameof(GraphView.AxisX), gv.AxisX);
            yield return this.CreateSubProperty(nameof(VerticalAxis.Visible), nameof(GraphView.AxisY), gv.AxisY);
            yield return this.CreateSubProperty(nameof(VerticalAxis.Increment), nameof(GraphView.AxisY), gv.AxisY);
            yield return this.CreateSubProperty(nameof(VerticalAxis.ShowLabelsEvery), nameof(GraphView.AxisY), gv.AxisY);
            yield return this.CreateSubProperty(nameof(VerticalAxis.Minimum), nameof(GraphView.AxisY), gv.AxisY);
            yield return this.CreateSubProperty(nameof(VerticalAxis.Text), nameof(GraphView.AxisY), gv.AxisY);
        }

        if (this.View is Window)
        {
            yield return this.CreateProperty(nameof(Window.Title));
        }

        if (this.View is FrameView)
        {
            yield return this.CreateProperty(nameof(FrameView.Title));
        }

        if (this.View is TreeView tree)
        {
            yield return this.CreateSubProperty(nameof(TreeStyle.CollapseableSymbol), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.ColorExpandSymbol), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.ExpandableSymbol), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.InvertExpandSymbolColors), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.LeaveLastRow), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.ShowBranchLines), nameof(TreeView<ITreeNode>.Style), tree.Style);
        }

        if (this.View is TableView tv)
        {
            yield return this.CreateProperty(nameof(TableView.FullRowSelect));

            yield return this.CreateSubProperty(nameof(TableStyle.AlwaysShowHeaders), nameof(TableView.Style), tv.Style);
            yield return this.CreateSubProperty(nameof(TableStyle.ExpandLastColumn), nameof(TableView.Style), tv.Style);
            yield return this.CreateSubProperty(nameof(TableStyle.InvertSelectedCellFirstCharacter), nameof(TableView.Style), tv.Style);
            yield return this.CreateSubProperty(nameof(TableStyle.ShowHorizontalHeaderOverline), nameof(TableView.Style), tv.Style);
            yield return this.CreateSubProperty(nameof(TableStyle.ShowHorizontalHeaderUnderline), nameof(TableView.Style), tv.Style);
            yield return this.CreateSubProperty(nameof(TableStyle.ShowVerticalCellLines), nameof(TableView.Style), tv.Style);
            yield return this.CreateSubProperty(nameof(TableStyle.ShowVerticalHeaderLines), nameof(TableView.Style), tv.Style);
        }

        if (this.View is TabView tabView)
        {
            yield return this.CreateProperty(nameof(TabView.MaxTabTextWidth));

            yield return this.CreateSubProperty(nameof(TabStyle.ShowBorder), nameof(TabView.Style), tabView.Style);
            yield return this.CreateSubProperty(nameof(TabStyle.ShowTopLine), nameof(TabView.Style), tabView.Style);
            yield return this.CreateSubProperty(nameof(TabStyle.TabsOnBottom), nameof(TabView.Style), tabView.Style);
        }

        if (this.View is RadioGroup)
        {
            yield return this.CreateProperty(nameof(RadioGroup.RadioLabels));
        }
    }

    private Property CreateSubProperty(string name, string subObjectName, object subObject)
    {
        return new Property(this, subObject.GetType().GetProperty(name)
            ?? throw new Exception($"Could not find expected Property '{name}' on Sub Object of Type '{subObject.GetType()}'"),
            subObjectName, subObject);
    }

    private Property CreateProperty(string name)
    {
        return new Property(this, this.View.GetType().GetProperty(name)
            ?? throw new Exception($"Could not find expected Property '{name}' on View of Type '{this.View.GetType()}'"));
    }

    /// <summary>
    /// Returns all operations not to do with setting properties.  Often these
    /// are view specific e.g. add/remove column from a <see cref="TableView"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IOperation> GetExtraOperations()
    {
        return this.GetExtraOperations(Point.Empty);
    }

    /// <summary>
    /// Returns one off atomic activities that can be performed on the view e.g. 'add a column'.
    /// </summary>
    /// <param name="pos">If this was triggered by a click then the position of the click
    /// may inform what operations are returned (e.g. right clicking a specific table view column).  Otherwise
    /// <see cref="Point.Empty"/></param>
    /// <returns></returns>
    internal IEnumerable<IOperation> GetExtraOperations(Point pos)
    {
        // Extra TableView operations
        if (this.View is TableView tv)
        {
            // if user right clicks a cell then provide options relating to the clicked column
            DataColumn? col = null;
            if (!pos.IsEmpty)
            {
                // TODO: Currently you have to right click in the row (body) of the table
                // and cannot right click the headers themselves
                var cell = tv.ScreenToCell(pos.X, pos.Y);
                if (cell != null)
                {
                    col = tv.Table.Columns[cell.Value.X];
                }
            }

            // if no column was right clicked then provide commands for the selected column
            if (col == null && tv.SelectedColumn >= 0)
            {
                col = tv.Table.Columns[tv.SelectedColumn];
            }

            yield return new AddColumnOperation(this, null);

            // no columns are selected so don't offer removal.
            if (col != null)
            {
                yield return new RemoveColumnOperation(this, col);
                yield return new RenameColumnOperation(this, col);
            }
        }

        if (this.IsContainerView || this.IsRoot)
        {
            yield return new AddViewOperation(this);
            yield return new PasteOperation(this);
        }
        else
        {
            var nearestContainer = this.View.GetNearestContainerDesign();
            if (nearestContainer != null)
            {
                yield return new AddViewOperation(nearestContainer);
            }
        }

        yield return new DeleteViewOperation(this);

        if (this.View is TabView)
        {
            yield return new AddTabOperation(this,null);
            yield return new RemoveTabOperation(this);
            yield return new RenameTabOperation(this);

            yield return new MoveTabOperation(this, -1);
            yield return new MoveTabOperation(this, 1);
        }

        if (this.View is MenuBar)
        {
            yield return new AddMenuOperation(this, null);
            yield return new RemoveMenuOperation(this);
        }
    }

    /// <summary>
    /// Returns all designable controls that are in the same container as this
    /// Does not include sub container controls etc.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Design> GetSiblings()
    {
        // If there is no parent then we are likely the top rot Design
        // or an orphan.  Either way we have no siblings
        if (this.View.SuperView == null)
        {
            yield break;
        }

        foreach (var v in this.View.SuperView.Subviews)
        {
            if (v == this.View)
            {
                continue;
            }

            if (v.Data is Design d)
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
        var root = this.GetRootDesign();

        // Return the root design
        yield return root;

        // And all child designs
        foreach (var d in this.GetAllChildDesigns(root.View))
        {
            yield return d;
        }
    }

    /// <summary>
    /// Returns the topmost view above this which has an associated
    /// Design or this if there are no Design above this.
    /// </summary>
    /// <returns>The topmost design that is being edited.</returns>
    public Design GetRootDesign()
    {
        var toReturn = this;
        var v = this.View;

        while (v.SuperView != null)
        {
            v = v.SuperView;

            if (v.Data is Design d)
            {
                if (d.IsRoot)
                {
                    return d;
                }

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
            toReturn.AddRange(this.GetAllChildDesigns(subView));
        }

        return toReturn;
    }

    public override string ToString()
    {
        return this.FieldName;
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
        var root = this.GetRootDesign();

        var allDesigns = root.GetAllDesigns();

        var name = CodeDomArgs.MakeValidFieldName($"{viewType.Name.ToLower()}");
        return name.MakeUnique(allDesigns.Select(d => d.FieldName));
    }

    public string GetUniqueFieldName(string candidate)
    {
        var root = this.GetRootDesign();

        // remove problematic characters
        candidate = CodeDomArgs.MakeValidFieldName(candidate);

        var allDesigns = root.GetAllDesigns().ToList();
        allDesigns.Remove(this);

        // what field names are already taken by other objects?
        var usedFieldNames = allDesigns.Select(d => d.FieldName).ToList();
        usedFieldNames.AddRange(ColorSchemeManager.Instance.Schemes.Select(k => k.Name));

        return candidate.MakeUnique(usedFieldNames);
    }

    /// <summary>
    /// Returns all <see cref="Design"/> that have dependency on this
    /// </summary>
    public IEnumerable<Design> GetDependantDesigns()
    {
        var everyone = this.GetAllDesigns().ToArray();
        return everyone.Where(o => this.DependsOnUs(o, everyone));
    }

    private bool DependsOnUs(Design other, Design[] everyone)
    {
        // obviously we cannot depend on ourselves
        if (other == this)
        {
            return false;
        }

        // if their X depends on us
        if (other.View.X.GetPosType(everyone, out _, out _, out var relativeTo, out _, out _))
        {
            if (relativeTo == this)
            {
                return true;
            }
        }

        // if their Y depends on us
        if (other.View.Y.GetPosType(everyone, out _, out _, out relativeTo, out _, out _))
        {
            if (relativeTo == this)
            {
                return true;
            }
        }

        return false;
    }
}
