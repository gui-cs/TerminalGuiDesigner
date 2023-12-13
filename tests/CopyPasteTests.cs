using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.TabOperations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( CopyOperation ) )]
[TestOf( typeof( PasteOperation ) )]
[Category( "UI Operations" )]
internal class CopyPasteTests : Tests
{
    [Test]
    public void CannotCopyRoot()
    {
        var d = Get10By10View();

        var top = new Toplevel();
        top.Add(d.View);

        Assert.That(d.IsRoot);
        var copy = new CopyOperation(d);

        Assert.That(copy.IsImpossible);
    }

    [Test]
    public void CopyPasteTableView()
    {
        var d = Get10By10View();

        var tv = ViewFactory.Create<TableView>( );

        Assert.That( new AddViewOperation(tv, d, "mytbl").Do() );

        var tvDesign = (Design)tv.Data;

        Assert.Multiple( ( ) =>
        {
            Assert.That(
                tv.Style.InvertSelectedCellFirstCharacter, Is.False,
                "Expected default state for this flag to be false");

            Assert.That(
                tv.FullRowSelect, Is.False,
                "Expected default state for this flag to be false");
        } );

        var dt = tv.GetDataTable();

        dt.Rows.Clear();
        dt.Columns.Clear();

        Assume.That( dt.Rows, Is.Empty );
        Assume.That( dt.Columns, Is.Empty );

        const string columnName1 = "Yarg";
        const string columnName2 = "Blerg";
        dt.Columns.Add(columnName1, typeof(int));
        dt.Columns.Add(columnName2, typeof(DateTime));

        // flip these flags, so we can check that it is properly cloned
        tv.Style.InvertSelectedCellFirstCharacter = true;
        tv.FullRowSelect = true;

        OperationManager.Instance.ClearUndoRedo();

        var selectionManager = SelectionManager.Instance;

        var copy = new CopyOperation(tvDesign);
        OperationManager.Instance.Do(copy);

        // TODO: Remove this comment once addressed
        // I think it would be a good idea to make the undoStack internal and then
        // expose internals to the test project namespace, so we can actually inspect
        // the collection itself.
        // Using this proxy property only gives the test limited validity.
        Assert.That( OperationManager.Instance.UndoStackSize,
                     Is.Zero,
                     "Since you cannot Undo a Copy we expected undo stack to be empty" );

        selectionManager.Clear();

        Assert.That( selectionManager.Selected, Is.Null.Or.Empty );

        var paste = new PasteOperation(d);
        OperationManager.Instance.Do(paste);

        Assert.Multiple( ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize,
                         Is.EqualTo( 1 ),
                         "Undo stack should contain the paste operation" );

            Assert.That( selectionManager.Selected,
                         Is.Not.Empty,
                         "After pasting, the new clone should be selected" );
        } );

        var tv2Design = selectionManager.Selected.Single();
        var tv2 = (TableView)tv2Design.View;

        // The cloned table style/properties should match the copied ones
        Assert.Multiple( ( ) =>
        {
            Assert.That(tv2.Style.InvertSelectedCellFirstCharacter);
            Assert.That(tv2.FullRowSelect);
        } );

        // The cloned table columns should match the copied ones
        var tableSource = tv2.Table;
        Assume.That( tableSource, Is.Not.Null );
        Assert.Multiple( ( ) =>
        {
            Assert.That( tableSource.Columns, Is.EqualTo( 2 ) );
            Assert.That( tableSource.Rows, Is.Zero );
            Assert.That( tableSource.ColumnNames[0], Is.EqualTo( columnName1 ) );
            Assert.That( tableSource.ColumnNames[1], Is.EqualTo( columnName2 ) );
        } );

        var dataTableColumns = tv2.GetDataTable().Columns;
        Assert.Multiple( ( ) =>
        {
            Assert.That( dataTableColumns[ 0 ].DataType, Is.EqualTo( typeof( int ) ) );
            Assert.That( dataTableColumns[ 1 ].DataType, Is.EqualTo( typeof( DateTime ) ) );

            Assert.That( tableSource, Is.Not.SameAs( tv.Table ) );
        } );
    }

    [Test]
    public void CopyPastePosRelative_Simple()
    {
        var d = Get10By10View();

        var lbl = new Label("Name:");
        var tb = new TextField
        {
            Width = 10,
            X = Pos.Right(lbl) + 1,
        };

        new AddViewOperation(lbl, d, "lbl").Do();
        new AddViewOperation(tb, d, "tb").Do();

        var selected = SelectionManager.Instance;
        selected.Clear();
        selected.SetSelection((Design)lbl.Data, (Design)tb.Data);

        new CopyOperation(SelectionManager.Instance.Selected.ToArray()).Do();
        var cmd = new PasteOperation(d);

        Assert.That( cmd.IsImpossible, Is.False );
        OperationManager.Instance.Do(cmd);

        // (Root + 2 original + 2 cloned)
        Assert.That( d.GetAllDesigns().Count(), Is.EqualTo( 5 ) );

        var cloneLabelDesign = selected.Selected.ElementAt(0);
        var cloneTextFieldDesign = selected.Selected.ElementAt(1);

        Assert.That( cloneLabelDesign.View, Is.InstanceOf<Label>( ) );
        Assert.That( cloneTextFieldDesign.View, Is.InstanceOf<TextField>( ) );

        cloneTextFieldDesign.View.X.GetPosType(
            d.GetAllDesigns().ToArray(),
            out var posType, out _, out var relativeTo, out var side, out var offset);

        Assert.That( posType, Is.EqualTo( PosType.Relative ) );
        Assert.That( offset, Is.EqualTo( 1 ) );
        Assert.That( side, Is.EqualTo( Side.Right ) );

        Assert.That( relativeTo, Is.SameAs( cloneLabelDesign ), "Pasted clone should be relative to the also pasted label");
    }

    /// <summary>
    /// Tests copying a TextField that has <see cref="PosType.Relative"/> on another View
    /// but that View is not also being copy/pasted.  In this case we should leave the
    /// Pos how it is (pointing at the original View).
    /// </summary>
    [Test]
    public void CopyPastePosRelative_CopyOnlyDependent()
    {
        var d = Get10By10View();

        var lbl = new Label("Name:");
        var tb = new TextField
        {
            Width = 10,
            X = Pos.Right(lbl) + 1,
        };

        new AddViewOperation(lbl, d, "lbl").Do();
        new AddViewOperation(tb, d, "tb").Do();

        var selected = SelectionManager.Instance;

        // Copy only the TextField and not the View it's Pos points to
        new CopyOperation((Design)tb.Data).Do();

        var cmd = new PasteOperation(d);

        Assert.That( cmd.IsImpossible, Is.False );
        OperationManager.Instance.Do(cmd);

        // (Root + 2 original + 1 cloned)
        Assert.That( d.GetAllDesigns().Count(), Is.EqualTo( 4 ) );

        var cloneTextFieldDesign = selected.Selected.ElementAt(0);
        Assert.That( cloneTextFieldDesign.View, Is.InstanceOf<TextField>( ) );

        cloneTextFieldDesign.View.X.GetPosType(
            d.GetAllDesigns().ToArray(),
            out var posType, out _, out var relativeTo, out var side, out var offset);

        Assert.That( posType, Is.EqualTo( PosType.Relative ) );
        Assert.That( offset, Is.EqualTo( 1 ) );
        Assert.That( side, Is.EqualTo( Side.Right ) );

        Assert.That( relativeTo, Is.SameAs( lbl.Data ), "Pasted clone should be relative to the original label");
    }

    [Test]
    public void CopyPasteColorScheme()
    {
        Design? rootDesign = null;
        Label? lbl = null;
        TextField? tb = null;
        
        Assume.That( ( ) => rootDesign = Get10By10View( ), Throws.Nothing );
        Assume.That( ( ) => lbl = ViewFactory.Create<Label>( null, null, "Name:" ), Throws.Nothing );
        Assume.That( ( ) =>  tb = ViewFactory.Create<TextField>( ), Throws.Nothing );
        
        Assume.That( rootDesign, Is.Not.Null.And.InstanceOf<Design>( ) );
        Assume.That( lbl, Is.Not.Null.And.InstanceOf<Label>( ) );
        Assume.That( tb, Is.Not.Null.And.InstanceOf<TextField>( ) );

        bool addLabelOperationSucceeded = false;
        bool addTextFieldOperationSucceeded = false;
        
        Assume.That( ( ) => addLabelOperationSucceeded = new AddViewOperation( lbl!, rootDesign!, "lbl" ).Do( ), Throws.Nothing );
        Assume.That( ( ) => addTextFieldOperationSucceeded = new AddViewOperation( tb!, rootDesign!, "tb" ).Do( ), Throws.Nothing );
        Assume.That( addLabelOperationSucceeded );
        Assume.That( addTextFieldOperationSucceeded );

        Design? labelDesign = null;
        Design? textFieldDesign = null;
        Assume.That( ( ) => labelDesign = rootDesign!.GetAllDesigns().SingleOrDefault(d => d.FieldName == "lbl"), Throws.Nothing );
        Assume.That( ( ) => textFieldDesign = rootDesign!.GetAllDesigns().SingleOrDefault(d => d.FieldName == "tb"), Throws.Nothing );
        Assume.That( labelDesign, Is.Not.Null.And.InstanceOf<Design>( ) );
        Assume.That( textFieldDesign, Is.Not.Null.And.InstanceOf<Design>( ) );

        SelectionManager selected = SelectionManager.Instance;

        ColorScheme green = new() { Normal = new(Color.Green, Color.Cyan) };
        Assume.That( green, Is.Not.Null.And.InstanceOf<ColorScheme>( ) );

        Assume.That( labelDesign!.GetDesignableProperties( ).OfType<ColorSchemeProperty>( ).ToArray( ),
                     Has.Length.EqualTo( 1 ),
                     "Should only be one ColorScheme property" );
        
        ColorScheme addedScheme = ColorSchemeManager.Instance.AddOrUpdateScheme("green", green, rootDesign!);
        Assume.That( addedScheme, Is.SameAs( green ) );

        Assume.That( textFieldDesign!.TryGetDesignableProperty( nameof( ColorScheme ), out Property? colorSchemeProperty ) );
        Assume.That( colorSchemeProperty, Is.Not.Null.And.InstanceOf<Property>( ) );
        Assume.That( ( ) => colorSchemeProperty.SetValue( green ), Throws.Nothing );
        
        rootDesign!.View.ColorScheme = green;

        Assume.That( lbl!.ColorScheme, Is.SameAs( green ), "The label should inherit color scheme from the parent" );

        Assume.That( labelDesign!.GetDesignableProperties( ).OfType<ColorSchemeProperty>( ).Single( ).ToString( ),
                     Is.EqualTo( "ColorScheme:(Inherited)" ),
                     "Expected ColorScheme to be known to be inherited" );

        Assume.That( textFieldDesign.GetDesignableProperties( ).OfType<ColorSchemeProperty>( ).Single( ).ToString( ),
                     Is.EqualTo( "ColorScheme:green" ),
                     "TextBox inherits but also is explicitly marked as green" );

        SelectionManager.Instance.SetSelection(labelDesign, textFieldDesign);
        Assume.That( SelectionManager.Instance.Selected, Does.Contain( labelDesign ).And.Contains( textFieldDesign ) );

        CopyOperation copyOperation = new( SelectionManager.Instance.Selected.ToArray( ) );
        Assert.That( copyOperation, Is.Not.Null.And.InstanceOf<CopyOperation>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( copyOperation.IsImpossible, Is.False );
            Assert.That( copyOperation.SupportsUndo, Is.False, "What would it even mean to undo a copy?" );
        } );

        bool copyOperationSucceeded = false;
        Assert.That( ( ) => copyOperationSucceeded = copyOperation.Do( ), Throws.Nothing );
        Assert.That( copyOperationSucceeded );

        SelectionManager.Instance.SetSelection(labelDesign, textFieldDesign);
        Assume.That( SelectionManager.Instance.Selected, Does.Contain( labelDesign ).And.Contains( textFieldDesign ) );

        PasteOperation pasteOperation = new (rootDesign);
        Assume.That( pasteOperation, Is.Not.Null.And.InstanceOf<PasteOperation>( ) );

        Assert.That(pasteOperation.IsImpossible, Is.False );
        Assert.That(pasteOperation.SupportsUndo );
        
        bool pasteOperationSucceeded = false;
        Assert.That( ( ) => pasteOperationSucceeded = pasteOperation.Do( ), Throws.Nothing );
        Assert.That( pasteOperationSucceeded );

        // (Root + 2 original + 2 cloned)
        Design[] allDesigns = rootDesign.GetAllDesigns( ).ToArray( );
        Assert.That( allDesigns, Has.Length.EqualTo( 5 ) );
        
        // Reference equality should be maintained through the copy/paste operation
        Assert.Multiple( ( ) =>
        {
            Assert.That( allDesigns, Has.One.SameAs( labelDesign ) );
            Assert.That( allDesigns, Has.One.SameAs( textFieldDesign ) );
        } );
        
        // clear whatever the current selection is (probably the pasted views)
        SelectionManager.Instance.Clear();
    }

    [Test]
    public void CopyPasteContainer( [Values] bool alsoSelectSubElements )
    {
        RoundTrip<Window, FrameView>(
            ( d, v ) =>
            {
                Label lbl1 = ViewFactory.Create<Label>( );
                Label lbl2 = ViewFactory.Create<Label>( );
                Assume.That( ( ) => new AddViewOperation( lbl1, d, "lbl1" ).Do( ), Throws.Nothing );
                Assume.That( ( ) => new AddViewOperation( lbl2, d, "lbl2" ).Do( ), Throws.Nothing );

                View[] actualSubviews = v.GetActualSubviews( ).ToArray( );
                Assume.That( actualSubviews, Has.Length.EqualTo( 2 ) );
                Assume.That( actualSubviews, Has.One.SameAs( lbl1 ) );
                Assume.That( actualSubviews, Has.One.SameAs( lbl2 ) );

                Design[]? toCopy;

                if ( alsoSelectSubElements )
                {
                    Design? lbl1Design = d.View.GetActualSubviews( ).First( ).Data as Design;
                    Assume.That( lbl1Design, Is.Not.Null.And.InstanceOf<Design>( ) );
                    Assume.That( lbl1Design!.FieldName, Is.EqualTo( "lbl1" ) );

                    toCopy = new[] { d, lbl1Design };
                }
                else
                {
                    toCopy = new[] { d };
                }

                CopyOperation copyOperation = new( toCopy );
                Assume.That( copyOperation, Is.Not.Null.And.InstanceOf<CopyOperation>( ) );
                Assume.That( copyOperation.SupportsUndo, Is.False );
                Assume.That( copyOperation.IsImpossible, Is.False );
                Assume.That( copyOperation.TimesDone, Is.Zero );

                bool copyOperationSucceeded = false;
                Assert.That( ( ) => copyOperationSucceeded = copyOperation.Do( ), Throws.Nothing );
                Assert.That( copyOperationSucceeded );
                Assert.That( copyOperation.TimesDone, Is.EqualTo( 1 ) );

                var rootDesign = d.GetRootDesign( );

                PasteOperation pasteOperation = new( rootDesign );
                Assume.That( pasteOperation, Is.Not.Null.And.InstanceOf<PasteOperation>( ) );
                Assume.That( pasteOperation.SupportsUndo );
                Assume.That( pasteOperation.IsImpossible, Is.False );
                Assume.That( pasteOperation.TimesDone, Is.Zero );

                bool pasteOperationSucceeded = false;
                Assert.That( ( ) => pasteOperationSucceeded = pasteOperation.Do( ), Throws.Nothing );
                Assert.That( pasteOperationSucceeded );
                Assert.That( pasteOperation.TimesDone, Is.EqualTo( 1 ) );

                View[] rootSubviews = rootDesign.View.GetActualSubviews( ).ToArray( );
                Assert.That( rootSubviews.Length, Is.EqualTo( 2 ) );
                Assert.That( rootSubviews, Has.All.InstanceOf<FrameView>( ) );
            }
            , out _
        );
    }

    [Test]
    public void CopyPasteContainer_Empty_ScrollView()
    {
        RoundTrip<Window, ScrollView>(
            ( d, v ) =>
            {
                Assume.That( d, Is.Not.Null.And.InstanceOf<Design>( ) );
                Assume.That( v, Is.Not.Null.And.InstanceOf<ScrollView>( ) );
                Assume.That( v.GetActualSubviews( ), Is.Empty );

                // copy the ScrollView
                CopyOperation copyOperation = new( d );
                Assume.That( copyOperation, Is.Not.Null.And.InstanceOf<CopyOperation>( ) );
                Assume.That( copyOperation.SupportsUndo, Is.False );
                Assume.That( copyOperation.IsImpossible, Is.False );
                Assume.That( copyOperation.TimesDone, Is.Zero );

                bool copyOperationSucceeded = false;
                Assert.That( ( ) => copyOperationSucceeded = copyOperation.Do( ), Throws.Nothing );
                Assert.That( copyOperationSucceeded );
                Assert.That( copyOperation.TimesDone, Is.EqualTo( 1 ) );

                var rootDesign = d.GetRootDesign( );

                PasteOperation pasteOperation = new( rootDesign );
                Assume.That( pasteOperation, Is.Not.Null.And.InstanceOf<PasteOperation>( ) );
                Assume.That( pasteOperation.SupportsUndo );
                Assume.That( pasteOperation.IsImpossible, Is.False );
                Assume.That( pasteOperation.TimesDone, Is.Zero );

                bool pasteOperationSucceeded = false;
                Assert.That( ( ) => pasteOperationSucceeded = pasteOperation.Do( ), Throws.Nothing );
                Assert.That( pasteOperationSucceeded );
                Assert.That( pasteOperation.TimesDone, Is.EqualTo( 1 ) );

                var rootSubviews = rootDesign.View.GetActualSubviews( );
                Assert.That( rootSubviews, Has.Count.EqualTo( 2 ) );
                Assert.That( rootSubviews, Has.All.InstanceOf<ScrollView>( ) );
            }
            , out _
        );
    }
    [Test]
    public void TestCopyPasteContainer_EmptyScrollView_IntoItself_InsteadPastesToRoot()
    {
        RoundTrip<Window, ScrollView>(
            (d, v) =>
            {
                // copy the ScrollView
                new CopyOperation(d).Do();

                var rootDesign = d.GetRootDesign();

                ClassicAssert.IsTrue(new PasteOperation(d).Do());

                var rootSubviews = rootDesign.View.GetActualSubviews();

                ClassicAssert.AreEqual(2, rootSubviews.Count, "Expected root to have 2 ScrollView now");
                ClassicAssert.IsTrue(rootSubviews.All(v => v is ScrollView));
            }
            , out _
            );
    }

    [Test]
    public void TestCopyPasteContainer_TabView()
    {
        RoundTrip<Window, TabView>(
            (d, v) =>
            {
                // Setup a TabView with 3 tabs each of which has 2 labels
                v.SelectedTab = v.Tabs.ElementAt(0);
                new AddViewOperation(new Label("lbl1"),d,"lbl1").Do();
                new AddViewOperation(new Label("lbl2"), d, "lbl2").Do();
                v.SelectedTab = v.Tabs.ElementAt(1);
                new AddViewOperation(new Label("lbl3"), d, "lbl3").Do();
                new AddViewOperation(new Label("lbl4"), d, "lbl4").Do();
                
                new AddTabOperation(d, "newTab").Do();
                v.SelectedTab = v.Tabs.ElementAt(2);
                new AddViewOperation(new Label("lbl5"), d, "lbl5").Do();
                new AddViewOperation(new Label("lbl6"), d, "lbl6").Do();

                ClassicAssert.AreEqual(3, v.Tabs.Count);
                ClassicAssert.AreEqual(2, v.Tabs.ElementAt(0).View.GetActualSubviews().Count);
                ClassicAssert.AreEqual(2, v.Tabs.ElementAt(1).View.GetActualSubviews().Count);
                ClassicAssert.AreEqual(2, v.Tabs.ElementAt(2).View.GetActualSubviews().Count);

                // copy the TabView
                new CopyOperation(d).Do();

                var rootDesign = d.GetRootDesign();
                ClassicAssert.IsTrue(new PasteOperation(rootDesign).Do());

                var rootSubviews = rootDesign.View.GetActualSubviews();

                ClassicAssert.AreEqual(2, rootSubviews.Count, "Expected root to have 2 TabView now");
                ClassicAssert.IsTrue(rootSubviews.All(v => v is TabView));

                var orig = (TabView)rootSubviews[0];
                var pasted = (TabView)rootSubviews[1];

                ClassicAssert.AreEqual(3, orig.Tabs.Count);
                ClassicAssert.AreEqual(2, orig.Tabs.ElementAt(0).View.GetActualSubviews().Count);
                ClassicAssert.AreEqual(2, orig.Tabs.ElementAt(1).View.GetActualSubviews().Count);
                ClassicAssert.AreEqual(2, orig.Tabs.ElementAt(2).View.GetActualSubviews().Count);

                ClassicAssert.AreEqual(3, pasted.Tabs.Count);
                ClassicAssert.AreEqual(2, pasted.Tabs.ElementAt(0).View.GetActualSubviews().Count);
                ClassicAssert.AreEqual(2, pasted.Tabs.ElementAt(1).View.GetActualSubviews().Count);
                ClassicAssert.AreEqual(2, pasted.Tabs.ElementAt(2).View.GetActualSubviews().Count);
            }
            , out _
            );
    }
}