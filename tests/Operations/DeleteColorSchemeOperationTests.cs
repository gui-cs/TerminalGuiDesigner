using System;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests.Operations;

internal class DeleteColorSchemeOperationTests : Tests
{
    [TestCase(true)]
    [TestCase(false)]
    public void Test_DeleteColorSchemeOperation_DoThenUndo_RoundTrip(bool withSelected)
    {
        var scheme = new ColorScheme();

        var lblIn = RoundTrip<Dialog, Label>(
            (d, v) =>
        {
            // Clear known default colors
            ColorSchemeManager.Instance.Clear();
            ClassicAssert.IsEmpty(ColorSchemeManager.Instance.Schemes);

            // Add a new color for our Label
            ColorSchemeManager.Instance.AddOrUpdateScheme("yarg", scheme, d.GetRootDesign());
            ClassicAssert.AreEqual(1, ColorSchemeManager.Instance.Schemes.Count);

            // Assign the new color to the view
            var prop = new SetPropertyOperation(d, new ColorSchemeProperty(d), null, scheme);
            prop.Do();
        }, out _);

        var lblInDesign = (Design)lblIn.Data ?? throw new Exception("Expected Design to exist on the label read in");

        ColorSchemeManager.Instance.Clear();
        ColorSchemeManager.Instance.FindDeclaredColorSchemes(lblInDesign.GetRootDesign());
        ClassicAssert.AreEqual(1, ColorSchemeManager.Instance.Schemes.Count, "Reloading the view should find the explicitly declared scheme 'yarg'");

        var rootDesignIn = lblInDesign.GetRootDesign();

        if (withSelected)
        {
            SelectionManager.Instance.ForceSetSelection(lblInDesign);
        }

        // now delete the scheme
        var yarg = ColorSchemeManager.Instance.GetNamedColorScheme("yarg");
        var deleteOp = new DeleteColorSchemeOperation(rootDesignIn, yarg);

        ClassicAssert.IsTrue(deleteOp.Do());

        // after deleting the color scheme nobody should be using it
        ClassicAssert.IsNull(lblIn.GetExplicitColorScheme());
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
            ColorSchemeManager.Instance.GetNameForColorScheme(
            lblIn.GetExplicitColorScheme() ?? throw new Exception("Expected lblIn to have the scheme again")),
            "Expected designer to still know the name of lblIn ColorScheme");

        ClassicAssert.AreEqual(yarg.Scheme, lblIn.GetExplicitColorScheme() ?? throw new Exception("View was unexpected no longer using our color scheme after Redo"));
        ClassicAssert.AreEqual(yarg.Scheme, lblInDesign.State.OriginalScheme ?? throw new Exception("View was unexpected no longer using our color scheme after Redo"));
    }
}
