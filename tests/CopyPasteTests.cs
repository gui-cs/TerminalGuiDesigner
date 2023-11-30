using System;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.TabOperations;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;

namespace UnitTests;

internal class CopyPasteTests : Tests
{
    [Test]
    public void CannotCopyRoot()
    {
        var d = this.Get10By10View();

        var top = new Toplevel();
        top.Add(d.View);

        ClassicAssert.IsTrue(d.IsRoot);
        var copy = new CopyOperation(d);

        ClassicAssert.IsTrue(copy.IsImpossible);
    }

    [Test]
    public void CopyPasteTableView()
    {
        var d = this.Get10By10View();

        var tv = (TableView)new ViewFactory().Create(typeof(TableView));

        ClassicAssert.IsTrue(
            new AddViewOperation(tv, d, "mytbl").Do());

        var tvDesign = (Design)tv.Data;

        ClassicAssert.IsFalse(
            tv.Style.InvertSelectedCellFirstCharacter,
            "Expected default state for this flag to be false");

        ClassicAssert.IsFalse(
            tv.FullRowSelect,
            "Expected default state for this flag to be false");

        tv.Table.Rows.Clear();
        tv.Table.Columns.Clear();
        tv.Table.Columns.Add("Yarg", typeof(int));
        tv.Table.Columns.Add("Blerg", typeof(DateTime));

        // flip these flags to so we can check that it is
        // properly cloned
        tv.Style.InvertSelectedCellFirstCharacter = true;
        tv.FullRowSelect = true;

        OperationManager.Instance.ClearUndoRedo();

        var selectionManager = SelectionManager.Instance;

        var copy = new CopyOperation(tvDesign);
        OperationManager.Instance.Do(copy);

        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize,
            "Since you cannot Undo a Copy we expected undo stack to be empty");

        selectionManager.Clear();

        ClassicAssert.IsEmpty(selectionManager.Selected);

        var paste = new PasteOperation(d);
        OperationManager.Instance.Do(paste);

        ClassicAssert.AreEqual(1, OperationManager.Instance.UndoStackSize,
            "Undo stack should contain the paste operation");

        ClassicAssert.IsNotEmpty(
            selectionManager.Selected,
            "After pasting, the new clone should be selected");

        var tv2Design = selectionManager.Selected.Single();
        var tv2 = (TableView)tv2Design.View;

        // The cloned table style/properties should match the copied ones
        ClassicAssert.IsTrue(tv2.Style.InvertSelectedCellFirstCharacter);
        ClassicAssert.IsTrue(tv2.FullRowSelect);

        // The cloned table columns should match the copied ones
        ClassicAssert.AreEqual(2, tv2.Table.Columns.Count);
        ClassicAssert.AreEqual(0, tv2.Table.Rows.Count);
        ClassicAssert.AreEqual("Yarg", tv2.Table.Columns[0].ColumnName);
        ClassicAssert.AreEqual("Blerg", tv2.Table.Columns[1].ColumnName);
        ClassicAssert.AreEqual(typeof(int), tv2.Table.Columns[0].DataType);
        ClassicAssert.AreEqual(typeof(DateTime), tv2.Table.Columns[1].DataType);

        ClassicAssert.AreNotSame(tv.Table, tv2.Table,
            "Cloned table should be a new table not a reference to the old one");
    }

    [Test]
    public void CopyPastePosRelative_Simple()
    {
        var d = this.Get10By10View();

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

        ClassicAssert.IsFalse(cmd.IsImpossible);
        OperationManager.Instance.Do(cmd);

        // (Root + 2 original + 2 cloned)
        ClassicAssert.AreEqual(5, d.GetAllDesigns().Count());

        var cloneLabelDesign = selected.Selected.ElementAt(0);
        var cloneTextFieldDesign = selected.Selected.ElementAt(1);

        ClassicAssert.IsInstanceOf(typeof(Label), cloneLabelDesign.View);
        ClassicAssert.IsInstanceOf(typeof(TextField), cloneTextFieldDesign.View);

        cloneTextFieldDesign.View.X.GetPosType(
            d.GetAllDesigns().ToArray(),
            out var posType, out _, out var relativeTo, out var side, out var offset);

        ClassicAssert.AreEqual(PosType.Relative, posType);
        ClassicAssert.AreEqual(1, offset);
        ClassicAssert.AreEqual(Side.Right, side);

        ClassicAssert.AreSame(cloneLabelDesign, relativeTo, "Pasted clone should be relative to the also pasted label");
    }

    /// <summary>
    /// Tests copying a TextField that has <see cref="PosType.Relative"/> on another View
    /// but that View is not also being copy/pasted.  In this case we should leave the
    /// Pos how it is (pointing at the original View).
    /// </summary>
    [Test]
    public void CopyPastePosRelative_CopyOnlyDepender()
    {
        var d = this.Get10By10View();

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

        ClassicAssert.IsFalse(cmd.IsImpossible);
        OperationManager.Instance.Do(cmd);

        // (Root + 2 original + 1 cloned)
        ClassicAssert.AreEqual(4, d.GetAllDesigns().Count());

        var cloneTextFieldDesign = selected.Selected.ElementAt(0);
        ClassicAssert.IsInstanceOf(typeof(TextField), cloneTextFieldDesign.View);

        cloneTextFieldDesign.View.X.GetPosType(
            d.GetAllDesigns().ToArray(),
            out var posType, out _, out var relativeTo, out var side, out var offset);

        ClassicAssert.AreEqual(PosType.Relative, posType);
        ClassicAssert.AreEqual(1, offset);
        ClassicAssert.AreEqual(Side.Right, side);

        ClassicAssert.AreSame(lbl.Data, relativeTo, "Pasted clone should be relative to the original label");
    }

    [Test]
    public void CopyPasteColorScheme()
    {
        var d = this.Get10By10View();

        var lbl = new Label("Name:");
        var tb = new TextField();

        new AddViewOperation(lbl, d, "lbl").Do();
        new AddViewOperation(tb, d, "tb").Do();

        var dlbl = d.GetAllDesigns().Single(d => d.FieldName == "lbl");
        var dtb = d.GetAllDesigns().Single(d => d.FieldName == "tb");

        var selected = SelectionManager.Instance;

        ColorScheme green;
        ColorSchemeManager.Instance.AddOrUpdateScheme("green", green = new ColorScheme { Normal = new Attribute(Color.Green, Color.Cyan) }, d);
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