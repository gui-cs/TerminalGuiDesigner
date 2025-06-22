using System;
using Terminal.Gui;
using Terminal.Gui.Drawing;
using Terminal.Gui.Views;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests.Operations;

internal class DeleteSchemeOperationTests : Tests
{
    [TestCase(true)]
    [TestCase(false)]
    public void Test_DeleteSchemeOperation_DoThenUndo_RoundTrip(bool withSelected)
    {
        var scheme = new Scheme();

        var lblIn = RoundTrip<Dialog, Label>(
            (d, v) =>
        {
            // Clear known default colors
            SchemeManager.Instance.Clear();
            ClassicAssert.IsEmpty(SchemeManager.Instance.Schemes);

            // Add a new color for our Label
            SchemeManager.Instance.AddOrUpdateScheme("yarg", scheme, d.GetRootDesign());
            ClassicAssert.AreEqual(1, SchemeManager.Instance.Schemes.Count);

            // Assign the new color to the view
            var prop = new SetPropertyOperation(d, new SchemeProperty(d), null, scheme);
            prop.Do();
        }, out _);

        var lblInDesign = (Design)lblIn.Data ?? throw new Exception("Expected Design to exist on the label read in");

        SchemeManager.Instance.Clear();
        SchemeManager.Instance.FindDeclaredSchemes(lblInDesign.GetRootDesign());
        ClassicAssert.AreEqual(1, SchemeManager.Instance.Schemes.Count, "Reloading the view should find the explicitly declared scheme 'yarg'");

        var rootDesignIn = lblInDesign.GetRootDesign();

        if (withSelected)
        {
            SelectionManager.Instance.ForceSetSelection(lblInDesign);
        }

        // now delete the scheme
        var yarg = SchemeManager.Instance.GetNamedScheme("yarg");
        var deleteOp = new DeleteSchemeOperation(rootDesignIn, yarg);

        ClassicAssert.IsTrue(deleteOp.Do());

        // after deleting the color scheme nobody should be using it
        ClassicAssert.IsNull(lblIn.GetExplicitScheme());
        ClassicAssert.IsNull(lblInDesign.State.OriginalScheme);

        // throw a curve ball, all these should do nothing
        deleteOp.Do();
        deleteOp.Do();
        deleteOp.Redo();
        deleteOp.Redo();

        // after redoing the operation we should be back to using it again
        deleteOp.Undo();

        ClassicAssert.AreEqual(
            "yarg",
            SchemeManager.Instance.GetNameForScheme(
            lblIn.GetExplicitScheme() ?? throw new Exception("Expected lblIn to have the scheme again")),
            "Expected designer to still know the name of lblIn Scheme");

        ClassicAssert.AreEqual(yarg.Scheme, lblIn.GetExplicitScheme() ?? throw new Exception("View was unexpected no longer using our color scheme after Redo"));
        ClassicAssert.AreEqual(yarg.Scheme, lblInDesign.State.OriginalScheme ?? throw new Exception("View was unexpected no longer using our color scheme after Redo"));
    }
}
