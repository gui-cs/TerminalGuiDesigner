using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;
using NLog;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.MenuOperations;
using TerminalGuiDesigner.Operations.StatusBarOperations;
using TerminalGuiDesigner.Operations.TableViewOperations;
using TerminalGuiDesigner.Operations.TabOperations;
using TerminalGuiDesigner.ToCode;
using static Terminal.Gui.TableView;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner;

/// <summary>
/// Wrapper of a <see cref="View"/> which is being designed by user.
/// </summary>
public class Design
{
    /// <summary>
    /// Name to use for the root <see cref="Design"/> (see <see cref="IsRoot"/>).
    /// </summary>
    public const string RootDesignName = "root";

    private readonly List<Property> designableProperties;
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// View Types for which <see cref="View.Text"/> does not make sense as a user
    /// configurable field (e.g. there is a Title field instead).
    /// </summary>
    private readonly HashSet<Type> excludeTextPropertyFor = new()
    {
        typeof(FrameView),
        typeof(TabView),
        typeof(Window),
        typeof(Toplevel),
        typeof(View),
        typeof(GraphView),
        typeof(HexView),
        typeof(LineView),
        typeof(ListView),
        typeof(MenuBar),
        typeof(TableView),
        typeof(TabView),
        typeof(TreeView),
        typeof(Dialog),
        typeof(NumericUpDown)
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Design"/> class.
    /// </summary>
    /// <param name="sourceCode">Source file that the <see cref="Design"/> will be written to on saving.</param>
    /// <param name="fieldName">The private instance name to use for <paramref name="view"/> when writing it out
    /// to <paramref name="sourceCode"/> or <see cref="RootDesignName"/> if <paramref name="view"/> <see cref="IsRoot"/>.</param>
    /// <param name="view">The view to wrap.</param>
    public Design(SourceCodeFile sourceCode, string fieldName, View view)
    {
        this.View = view;
        this.SourceCode = sourceCode;
        this.FieldName = fieldName;

        this.designableProperties = new List<Property>(this.LoadDesignableProperties());
        this.State = new DesignState(this);
    }

    /// <summary>
    /// Gets the .Designer.cs and .cs files that the <see cref="GetRootDesign"/>
    /// will be saved in and/or was loaded from.
    /// </summary>
    public SourceCodeFile SourceCode { get; }

    /// <summary>
    /// Gets or Sets the name of the instance member field when the <see cref="View"/>
    /// is turned to code in a .Designer.cs file.  For example "label1".
    /// <para>If this <see cref="IsRoot"/> then this will instead be <see cref="RootDesignName"/>
    /// and won't be written out to the <see cref="SourceCode"/> file.</para>
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets a value indicating whether this is the highest level <see cref="View"/>
    /// and therefore the root Type that user is designing (e.g. MyView : Window).
    /// <para><see langword="false"/> if the <see cref="Design"/> wraps a sub-view
    /// (i.e. anything user has added as content).</para>
    /// </summary>
    public bool IsRoot => this.FieldName.Equals(RootDesignName);

    /// <summary>
    /// Gets the record of user configured values of otherwise volatile <see cref="View"/> settings.
    /// <para>For example while <see cref="View.ColorScheme"/> can change based on selection
    /// (see <see cref="SelectionManager.SelectedScheme"/> the <see cref="DesignState.OriginalScheme"/>
    /// will not change.
    /// </para>
    /// </summary>
    public DesignState State { get; }

    /// <summary>
    /// Gets the <see cref="View"/> this <see cref="Design"/> wraps.  Do not use
    /// <see cref="View.Add(Terminal.Gui.View)"/> on this instance.  Instead use
    /// <see cref="AddViewOperation"/> so that new child controls are preserved
    /// for design time changes.
    /// </summary>
    public View View { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="View"/> is a 'container' (i.e. designed
    /// to hold other sub-controls.
    /// </summary>
    public bool IsContainerView => this.View.IsContainerView();

    /// <summary>
    /// Gets a value indicating whether <see cref="View"/> <see cref="IsContainerView"/> and
    /// it has no distinguishable visible border.
    /// </summary>
    public bool IsBorderlessContainerView => this.View.IsBorderlessContainerView();

    /// <summary>
    /// Returns the named <see cref="Property"/> if designing is supported for it
    /// on the <see cref="View"/> Type.
    /// </summary>
    /// <param name="propertyName">Name of the designable <see cref="Property.PropertyInfo"/> you want to find.</param>
    /// <returns>The <see cref="Property"/> if designable or null if not found or not supported.</returns>
    public Property? GetDesignableProperty(string propertyName)
    {
        return this.GetDesignableProperties().SingleOrDefault(p => p.PropertyInfo.Name.Equals(propertyName));
    }

    /// <summary>
    ///   Returns the named <see cref="Property" /> if designing is supported for it
    ///   on the <see cref="View" /> Type.
    /// </summary>
    /// <param name="propertyName">Name of the designable <see cref="Property.PropertyInfo" /> you want to find.</param>
    /// <param name="property">The retrieved property, if true, or null, if false.</param>
    /// <returns>
    ///   <see langword="true" /> if the <see cref="Property" /> exists and is designable or <see langword="false" /> if not found or not
    ///   supported.
    /// </returns>
    public bool TryGetDesignableProperty(string propertyName, [NotNullWhen(true)] out Property? property)
    {
        property = this.GetDesignableProperties( ).SingleOrDefault( p => p.PropertyInfo.Name.Equals( propertyName ) );
        return property is not null;
    }

    /// <summary>
    /// Walk the <see cref="View"/> hierarchy and create <see cref="Design"/> wrappers
    /// for any <see cref="View"/> found that were created by user (ignoring those that
    /// are artifacts of the Terminal.Gui API e.g. ContentView).
    /// </summary>
    public void CreateSubControlDesigns()
    {
        this.CreateSubControlDesigns(this.View);
    }

    /// <summary>
    /// Create <see cref="Design"/> wrappers for <paramref name="subView"/> and any children it
    /// has (recursively) that were created by user (ignoring those that are artifacts of the
    /// Terminal.Gui API e.g. ContentView).
    /// </summary>
    /// <param name="name">The <see cref="Design.FieldName"/> to allocate to the new <see cref="Design"/>.</param>
    /// <param name="subView">The <see cref="View"/> to create a new <see cref="Design"/> wrapper for.</param>
    /// <returns>A new <see cref="Design"/> wrapper wrapping <paramref name="subView"/>.</returns>
    public Design CreateSubControlDesign(string name, View subView)
    {
        // all views can be focused so that they can be edited
        // or deleted etc
        subView.CanFocus = true;

        if (subView is TableView tv && tv.Table != null && tv.GetDataTable().Rows.Count == 0)
        {
            var dt = tv.GetDataTable();

            // add example rows so that it is easier to design the view
            for (int i = 0; i < 100; i++)
            {
                var row = dt.NewRow();
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    row[c] = DBNull.Value;
                }

                dt.Rows.Add(row);
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
            txt.MouseClick += (s, e) => this.SuppressNativeClickEvents(s, e);
            txt.KeyDown += this.SuppressNativeKeyboardEvents;
        }

        if (subView is TextField tf)
        {
            // prevent control from responding to events
            tf.MouseClick += (s,e)=>this.SuppressNativeClickEvents(s,e);
            tf.KeyDown += SuppressNativeKeyboardEvents;
        }

        if (subView.GetType().IsGenericType(typeof(Slider<>)))
        {
            // TODO: Does not seem to work
            subView.MouseEventArgs += (s, e) => SuppressNativeClickEvents(s, e,true);
            subView.MouseClick += (s, e) => SuppressNativeClickEvents(s,e, true);
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

        // Disable click events for sub controls. This allows you to prevent clicking
        // in non designed subcomponents e.g. the bar of a true color picker.
        foreach (var v in subView.GetAllNonDesignableSubviews())
        {
            v.MouseClick += (s,e)=>this.SuppressNativeClickEvents(s,e,true);
            v.MouseEventArgs += (s, e) => this.SuppressNativeClickEvents(s, e, true);
        }
        
        var d = new Design(this.SourceCode, name, subView);
        return d;
    }

    /// <summary>
    /// <para>
    /// Returns true if there is an explicit ColorScheme set
    /// on this Design's View or false if it is inherited from
    /// a View further up the Layout (or a library default scheme).
    /// </para>
    /// <para> If a scheme is found that is not known about by ColorSchemeManager
    /// then false is returned.</para>
    /// </summary>
    /// <returns>True if view explicitly uses one in <see cref="ColorSchemeManager"/>
    /// because user explicitly allocated it to the <see cref="View"/> (rather than inheriting).</returns>
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
    /// uses a different ColorScheme.
    /// </summary>
    /// <param name="scheme">The scheme you want to know if <see cref="View"/> is using.</param>
    /// <returns>True if <see cref="View"/> uses <paramref name="scheme"/> explicitly.</returns>
    public bool UsesColorScheme(ColorScheme scheme)
    {
        // we use this scheme if it is a known scheme
        return this.HasKnownColorScheme() &&
            (this.View.ColorScheme.Equals(scheme) || (this.State.OriginalScheme?.Equals(scheme) ?? false));
    }

    /// <summary>
    /// Gets the designable properties of the hosted <see cref="View"/>.
    /// </summary>
    /// <returns>All designable properties on <see cref="View"/>.</returns>
    public IEnumerable<Property> GetDesignableProperties()
    {
        return this.designableProperties;
    }

    /// <summary>
    /// Returns all operations not to do with setting properties.  Often these
    /// are view specific e.g. add/remove column from a <see cref="TableView"/>.
    /// </summary>
    /// <returns>All view specific <see cref="Operation"/> supported on <see cref="View"/> Type.
    /// Does not return regular <see cref="Property"/> changing operations.</returns>
    public IEnumerable<IOperation> GetExtraOperations()
    {
        return this.GetExtraOperations(Point.Empty);
    }

    /// <summary>
    /// Returns all <see cref="Operation"/> that can be performed on the view at position <paramref name="pos"/>
    /// e.g. 'rename the right clicked column at this point'.
    /// </summary>
    /// <param name="pos">If this was triggered by a click then the position of the click
    /// may inform what operations are returned (e.g. right clicking a specific table view column).  Otherwise
    /// <see cref="Point.Empty"/>.</param>
    /// <returns>All view specific <see cref="IOperation"/> that are supported at the <paramref name="pos"/>.</returns>
    public IEnumerable<IOperation> GetExtraOperations(Point pos)
    {
        // Extra TableView operations
        if (this.View is TableView tv)
        {
            var dt = tv.GetDataTable();

            // if user right clicks a cell then provide options relating to the clicked column
            DataColumn? col = null;
            if (!pos.IsEmpty)
            {
                // See which column the right click lands.
                var cell = tv.ScreenToCell(pos.X, pos.Y, out var colIdx);


                if (cell != null && colIdx == null)
                {
                    col = dt.Columns[cell.Value.X];
                }
            }

            // if no column was right clicked then provide commands for the selected column
            if (col == null && tv.SelectedColumn >= 0)
            {
                col = dt.Columns[tv.SelectedColumn];
            }

            yield return new AddColumnOperation(this, null);

            // no columns are selected so don't offer removal.
            if (col != null)
            {
                yield return new RemoveColumnOperation(this, col);
                yield return new RenameColumnOperation(this, col, null);
                yield return new MoveColumnOperation(this, col, -1);
                yield return new MoveColumnOperation(this, col, 1);
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

        switch ( this.View )
        {
            case TabView tabView:
            {
                yield return new AddTabOperation(this, null);

                if (tabView.SelectedTab != null)
                {
                    yield return new RemoveTabOperation(this, tabView.SelectedTab);
                    yield return new RenameTabOperation(this, tabView.SelectedTab, null);
                    yield return new MoveTabOperation(this, tabView.SelectedTab, -1);
                    yield return new MoveTabOperation(this, tabView.SelectedTab, 1);
                }

                break;
            }
            case MenuBar mb:
            {
                yield return new AddMenuOperation(this, null);

                var menu = pos.IsEmpty ? mb.GetSelectedMenuItem() : mb.ScreenToMenuBarItem(pos.X);

                if (menu != null)
                {
                    yield return new RemoveMenuOperation(this, menu);
                    yield return new RenameMenuOperation(this, menu, null);
                    yield return new MoveMenuOperation(this, menu, -1);
                    yield return new MoveMenuOperation(this, menu, 1);
                }

                break;
            }
            case StatusBar sb:
            {
                yield return new AddStatusItemOperation(this, null);

                var item = sb.ScreenToMenuBarItem(pos.X);

                if (item != null)
                {
                    yield return new RemoveStatusItemOperation(this, item);
                    yield return new RenameStatusItemOperation(this, item, null);
                    yield return new SetShortcutOperation(this, item, null);
                    yield return new MoveStatusItemOperation(this, item, -1);
                    yield return new MoveStatusItemOperation(this, item, 1);
                }

                break;
            }
        }
    }

    /// <summary>
    /// Returns all designable controls that are in the same container as this
    /// Does not include sub container controls etc.
    /// </summary>
    /// <returns>Returns all <see cref="Design"/> in the same parent container  (<see cref="View.SuperView"/>
    /// as this).</returns>
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
    /// parent of this on down.  Gets everyone... Everyone? EVERYONE!!!.
    /// </summary>
    /// <returns>All views in scope.</returns>
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

    /// <summary>
    /// Returns <see cref="FieldName"/>.
    /// </summary>
    /// <returns><see cref="FieldName"/>.</returns>
    public override string ToString()
    {
        return this.FieldName;
    }

    /// <summary>
    /// Returns a new unique name for a view of type <paramref name="viewType"/>.
    /// </summary>
    /// <param name="viewType">The Type of <see cref="View"/> that the name is for.</param>
    /// <returns>A sensible, unique name for a new <see cref="View"/> of <paramref name="viewType"/>.</returns>
    public string GetUniqueFieldName(Type viewType)
    {
        var root = this.GetRootDesign();

        var allDesigns = root.GetAllDesigns();

        var name = CodeDomArgs.MakeValidFieldName($"{viewType.Name}");
        return name.MakeUnique(allDesigns.Select(d => d.FieldName));
    }

    /// <summary>
    /// Returns a new non-null, unique, valid <see cref="Design.FieldName"/> based on
    /// <paramref name="candidate"/>.  Uniqueness is guaranteed for all <see cref="Design"/>
    /// currently in scope (see <see cref="GetAllDesigns"/>).
    /// </summary>
    /// <param name="candidate">The name the user would like.</param>
    /// <returns>The unique valid non null name that will be used.</returns>
    public string GetUniqueFieldName(string? candidate)
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
    /// Returns all <see cref="Design"/> that have dependency on this.
    /// </summary>
    /// <returns>All other <see cref="Design"/> that have a <see cref="Pos"/> or other setting
    /// that relies on this <see cref="Design.View"/> existing (e.g. <see cref="Pos.Left(View)"/>).
    /// </returns>
    public IEnumerable<Design> GetDependantDesigns()
    {
        var everyone = this.GetAllDesigns().ToArray();
        return everyone.Where(o => this.DependsOnUs(o, everyone));
    }

    /// <summary>
    /// Returns all user designable sub-views of <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The parent whose children you want to retrieve.</param>
    /// <returns>All designable children recursively.</returns>
    public IEnumerable<Design> GetAllChildDesigns(View view)
    {
        List<Design> toReturn = new List<Design>();

        foreach (var subView in view.GetActualSubviews().ToArray())
        {
            if (subView.Data is Design d)
            {
                toReturn.Add(d);
            }

            // even if this sub-view isn't designable there might be designable ones further down
            // e.g. a ContentView of a Window
            toReturn.AddRange(this.GetAllChildDesigns(subView));
        }

        return toReturn;
    }

    private void CreateSubControlDesigns(View view)
    {
        foreach (var subView in view.GetActualSubviews().ToArray())
        {
            this.logger.Info($"Found subView of Type '{subView.GetType()}'");

            if (subView.Data is string name)
            {
                subView.Data = this.CreateSubControlDesign(name, subView);
            }

            this.CreateSubControlDesigns(subView);
        }
    }

    private void SuppressNativeClickEvents(object? sender, MouseEventArgs obj, bool alsoSuppressClick = false)
    {
        if (alsoSuppressClick)
        {
            obj.Handled = true;
        }
        else
        {
            // Suppress everything except single click (selection)
            obj.Handled = obj.Flags != MouseFlags.Button1Clicked;
        }
    }

    private void SuppressNativeKeyboardEvents(object? sender, Key e)
    {
        if (sender == null)
        {
            return;
        }

        if (e == Key.Tab || e == Key.Tab.WithShift || e == Key.Esc || e == Application.QuitKey)
        {
            e.Handled = false;
            return;
        }

        e.Handled = true;
    }

    private void RegisterCheckboxDesignTimeChanges(CheckBox cb)
    {
        // prevent space toggling the checkbox
        // (gives better typing experience e.g. "my lovely checkbox")
        cb.KeyBindings.Remove(Key.Space);
        cb.MouseClick += (s, e) =>
        {
            if (e.Flags.HasFlag(MouseFlags.Button1Clicked))
            {
                e.Handled = true;
                cb.SetFocus();
            }
        };
    }

    private IEnumerable<Property> LoadDesignableProperties()
    {
        var viewType = this.View.GetType();
        var isGenericType = viewType.IsGenericType;

        yield return this.CreateProperty(nameof(this.View.Width));
        yield return this.CreateProperty(nameof(this.View.Height));

        yield return this.CreateProperty(nameof(this.View.X));
        yield return this.CreateProperty(nameof(this.View.Y));

        yield return this.CreateSuppressedProperty(nameof(this.View.Visible), true);

        yield return this.CreateSuppressedProperty(nameof(this.View.Arrangement), ViewArrangement.Fixed);

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

        if (isGenericType && viewType.GetGenericTypeDefinition() == typeof(Slider<>))
        {
            yield return this.CreateProperty(nameof(Slider.Options));
            yield return this.CreateProperty(nameof(Slider.Orientation));
            yield return this.CreateProperty(nameof(Slider.RangeAllowSingle));
            yield return this.CreateProperty(nameof(Slider.AllowEmpty));
            yield return this.CreateProperty(nameof(Slider.MinimumInnerSpacing));
            yield return this.CreateProperty(nameof(Slider.LegendsOrientation));
            yield return this.CreateProperty(nameof(Slider.ShowLegends));
            yield return this.CreateProperty(nameof(Slider.ShowEndSpacing));
            yield return this.CreateProperty(nameof(Slider.Type));
        }

        if (this.View is SpinnerView)
        {
            yield return this.CreateProperty(nameof(SpinnerView.AutoSpin));

            yield return new InstanceOfProperty(
                this,
                viewType.GetProperty(nameof(SpinnerView.Style)) ?? throw new Exception($"Could not find expected Property SpinnerView.Style on View of Type '{this.View.GetType()}'"));
        }

        if (this.View is TextView)
        {
            // Do not allow tab at design time so that we don't get stuck in the View (adding more tabs each time!)
            // But let user edit if they want
            yield return this.CreateSuppressedProperty(nameof(TextView.AllowsTab), false);
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

        if (this.ShowTextProperty())
        {
            yield return new Property(this, this.View.GetActualTextProperty());
        }

        /*
        TODO: Borders are changed a lot in v2
            // Border properties - Most views dont have a border so Border is
            if (this.View.Border != null)
            {
                yield return this.CreateSubProperty(nameof(Border.BorderStyle), nameof(this.View.Border), this.View.Border);
                yield return this.CreateSubProperty(nameof(Border.BorderBrush), nameof(this.View.Border), this.View.Border);
                yield return this.CreateSubProperty(nameof(Border.Effect3D), nameof(this.View.Border), this.View.Border);
                yield return this.CreateSubProperty(nameof(Border.Effect3DBrush), nameof(this.View.Border), this.View.Border);
                yield return this.CreateSubProperty(nameof(Border.DrawMarginFrame), nameof(this.View.Border), this.View.Border);
            }
            */

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
            yield return this.CreateProperty(nameof(CheckBox.CheckedState));
        }
        if (this.View is ColorPicker cp)
        {
            yield return this.CreateSubProperty(nameof(ColorPickerStyle.ColorModel),nameof(ColorPicker.Style),cp.Style);
            yield return this.CreateSubProperty(nameof(ColorPickerStyle.ShowColorName), nameof(ColorPicker.Style), cp.Style);
            yield return this.CreateSubProperty(nameof(ColorPickerStyle.ShowTextFields), nameof(ColorPicker.Style), cp.Style);
        }

        if (this.View is ListView lv)
        {
            yield return this.CreateProperty(nameof(ListView.Source));
            yield return this.CreateProperty(nameof(ListView.AllowsMarking));
            yield return this.CreateProperty(nameof(ListView.AllowsMultipleSelection));
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

        if (this.View is ITreeView tree)
        {
            yield return this.CreateSubProperty(nameof(TreeStyle.CollapseableSymbol), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.ColorExpandSymbol), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.ExpandableSymbol), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.InvertExpandSymbolColors), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.LeaveLastRow), nameof(TreeView<ITreeNode>.Style), tree.Style);
            yield return this.CreateSubProperty(nameof(TreeStyle.ShowBranchLines), nameof(TreeView<ITreeNode>.Style), tree.Style);
        }
        
        if (isGenericType && viewType.GetGenericTypeDefinition() == typeof(TreeView<>))
        {
            var prop = this.CreateTreeObjectsProperty(viewType);
            if(((ITreeObjectsProperty)prop).IsSupported())
                yield return prop;
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

        if (viewType.IsGenericType(typeof(NumericUpDown<>)))
        {
            yield return this.CreateProperty(nameof(NumericUpDown.Value));
            yield return this.CreateProperty(nameof(NumericUpDown.Increment));

            // TODO: Probably needs some thought
            // yield return this.CreateProperty(nameof(NumericUpDown.Format));
        }
    }

    private Property CreateTreeObjectsProperty(Type viewType)
    {
        if(viewType.GetGenericTypeDefinition() != typeof(TreeView<>))
        {
            throw new ArgumentException("Method should only be called for TreeView<T>");
        }

        var tType = viewType.GetGenericArguments()[0];
        var propertyType = typeof(TreeObjectsProperty<>).MakeGenericType(tType);

        var instance =
            Activator.CreateInstance(propertyType, new object?[] { this })
            ?? throw new Exception($"Failed to construct {propertyType}");

        return (Property)instance;
    }

    private bool ShowTextProperty()
    {
        // never show Text for root because it's almost certainly a container
        // e.g. derived from Window or Dialog or View (directly)
        if (this.IsRoot)
        {
            return false;
        }

        // Do not let Text be set on Slider or Slider<> implementations as weird stuff happens
        if(this.View.GetType().Name.StartsWith("Slider") || View is RadioGroup || View.GetType().IsGenericType(typeof(NumericUpDown<>)))
        {
            return false;
        }

        return !this.excludeTextPropertyFor.Contains(this.View.GetType());
    }

    private Property CreateSubProperty(string name, string subObjectName, object subObject)
    {
        return new Property(
            this,
            subObject.GetType().GetProperty(name) ?? throw new Exception($"Could not find expected Property '{name}' on Sub Object of Type '{subObject.GetType()}'"),
            subObjectName,
            subObject);
    }

    private Property CreateProperty(string name)
    {
        return new Property(
            this,
            this.View.GetType().GetProperty(name) ?? throw new Exception($"Could not find expected Property '{name}' on View of Type '{this.View.GetType()}'"));
    }

    private Property CreateSuppressedProperty(string name, object? designTimeValue)
    {
        return new SuppressedProperty(
            this,
            this.View.GetType().GetProperty(name) ?? throw new Exception($"Could not find expected Property '{name}' on View of Type '{this.View.GetType()}'"),
            designTimeValue);
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
