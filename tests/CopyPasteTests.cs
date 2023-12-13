using System;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.TabOperations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests;

[TestFixture]
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
        var d = Get10By10View();

        var lbl = new Label("Name:");
        var tb = new TextField();

        bool addLabelOperationSucceeded = false;
        bool addTextFieldOperationSucceeded = false;
        
        Assume.That( ( ) => addLabelOperationSucceeded = new AddViewOperation( lbl, d, "lbl" ).Do( ), Throws.Nothing );
        Assume.That( ( ) => addTextFieldOperationSucceeded = new AddViewOperation( tb, d, "tb" ).Do( ), Throws.Nothing );
        Assume.That( addLabelOperationSucceeded );
        Assume.That( addTextFieldOperationSucceeded );

        var dlbl = d.GetAllDesigns().Single(d => d.FieldName == "lbl");
        var dtb = d.GetAllDesigns().Single(d => d.FieldName == "tb");

        var selected = SelectionManager.Instance;

        ColorScheme green;
        ColorSchemeManager.Instance.AddOrUpdateScheme("green", green = new() { Normal = new(Color.Green, Color.Cyan) }, d);
        dtb.GetDesignableProperty(nameof(ColorScheme))?.SetValue(green);
        d.View.ColorScheme = green;

        ClassicAssert.AreEqual(lbl.ColorScheme, green, "The label should inherit color scheme from the parent");

        ClassicAssert.AreEqual(
            "ColorScheme:(Inherited)",
            dlbl.GetDesignableProperties().OfType<ColorSchemeProperty>().Single().ToString(),
            "Expected ColorScheme to be known to be inherited");

        ClassicAssert.AreEqual(
            "ColorScheme:green",
            dtb.GetDesignableProperties().OfType<ColorSchemeProperty>().Single().ToString(),
            "TextBox inherits but also is explicitly marked as green");

        SelectionManager.Instance.SetSelection(dlbl, dtb);
        new CopyOperation(SelectionManager.Instance.Selected.ToArray()).Do();
        SelectionManager.Instance.SetSelection(dlbl, dtb);

        OperationManager.Instance.Do(new PasteOperation(d));

        // (Root + 2 original + 2 cloned)
        ClassicAssert.AreEqual(5, d.GetAllDesigns().Count());

        var dlbl2 = d.GetAllDesigns().Single(d => d.FieldName == "lbl2");
        var dtb2 = d.GetAllDesigns().Single(d => d.FieldName == "tb2");

        // clear whatever the current selection is (probably the pasted views)
        SelectionManager.Instance.Clear();

        ClassicAssert.AreEqual(dlbl2.View.ColorScheme, green, "The newly pasted label should also inherit color scheme from the parent");

        // but be known to inherit
        ClassicAssert.AreEqual(
            "ColorScheme:(Inherited)",
            dlbl2.GetDesignableProperties().OfType<ColorSchemeProperty>().Single().ToString(),
            "Expected ColorScheme to be known to be inherited");

        ClassicAssert.AreEqual(
            "ColorScheme:green",
            dtb2.GetDesignableProperties().OfType<ColorSchemeProperty>().Single().ToString(),
            "TextBox2 should have its copy pasted explicitly marked green");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestCopyPasteContainer(bool alsoSelectSubElements)
    {
        RoundTrip<Window, FrameView>(
            (d, v) =>
            {
                new AddViewOperation(new Label(), d, "lbl1").Do();
                new AddViewOperation(new Label(), d, "lbl2").Do();
                
                ClassicAssert.AreEqual(2,v.GetActualSubviews().Count(), "Expected the FrameView to have 2 children (lbl1 and lbl2)");

                Design[] toCopy;

                if(alsoSelectSubElements)
                {
                    var lbl1Design = (Design)d.View.GetActualSubviews().First().Data;
                    ClassicAssert.AreEqual("lbl1", lbl1Design.FieldName);

                    toCopy = new Design[] { d, lbl1Design};
                }
                else
                {
                    toCopy = new[] { d };
                }

                // copy the FrameView
                new CopyOperation(toCopy).Do();

                var rootDesign = d.GetRootDesign();

                ClassicAssert.IsTrue(new PasteOperation(rootDesign).Do());

                var rootSubviews = rootDesign.View.GetActualSubviews();

                ClassicAssert.AreEqual(2, rootSubviews.Count, "Expected root to have 2 FrameView now");
                ClassicAssert.IsTrue(rootSubviews.All(v => v is FrameView));


                ClassicAssert.IsTrue(
                    rootSubviews.All(f => f.GetActualSubviews().Count() == 2),
                    "Expected both FrameView (copied and pasted) to have the full contents of 2 Labels");
            }
            , out _
            );
    }

    [Test]
    public void TestCopyPasteContainer_Empty_ScrollView()
    {
        RoundTrip<Window, ScrollView>(
            (d, v) =>
            {
                // copy the ScrollView
                new CopyOperation(d).Do();

                var rootDesign = d.GetRootDesign();

                ClassicAssert.IsTrue(new PasteOperation(rootDesign).Do());

                var rootSubviews = rootDesign.View.GetActualSubviews();

                ClassicAssert.AreEqual(2, rootSubviews.Count, "Expected root to have 2 ScrollView now");
                ClassicAssert.IsTrue(rootSubviews.All(v => v is ScrollView));
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