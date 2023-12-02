using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests;

/// <summary>
/// Tests for adding Views to other Views either with <see cref="AddViewOperation"/> or directly.
/// </summary>
internal class AddViewTests : Tests
{
    [Test]
    public void TestAdd_Undo()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAdd_Undo.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        var op = new AddViewOperation(lbl, designOut, "label1");

        OperationManager.Instance.Do(op);
        ClassicAssert.AreEqual(1, designOut.View.GetActualSubviews().OfType<Label>().Count());

        OperationManager.Instance.Undo();
        ClassicAssert.AreEqual(0, designOut.View.GetActualSubviews().OfType<Label>().Count());

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        ClassicAssert.AreEqual(0, designBackIn.View.GetActualSubviews().OfType<Label>().Count());
    }

    [Test]
    public void TestAddUndoRedo_RoundTrip()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAddUndoRedo_RoundTrip.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        var op = new AddViewOperation(lbl, designOut, "label1");

        OperationManager.Instance.Do(op);
        OperationManager.Instance.Undo();
        OperationManager.Instance.Redo();

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var lblOut = designOut.View.GetActualSubviews().OfType<Label>().Single();

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        ClassicAssert.AreEqual(lblOut.Text, lblIn.Text);
    }

    /// <summary>
    /// 60 is one of those numbers that can't be modeled exactly in float so ends
    /// up with many decimal places.  This needs to be rounded in the code generated
    /// and has to have the suffix 'f' to ensure that the value is treated as a float
    /// and not a double (which won't compile)
    /// </summary>
    /// <param name="offset"></param>
    [TestCase(true)]
    [TestCase(null)]
    [TestCase(false)]
    public void Test60Percent_RoundTrip(bool? offset)
    {
        var lblIn = this.RoundTrip<Dialog, Label>(
            (d, lbl) =>
        {
            lbl.Width = offset == null ? Dim.Percent(60) : offset.Value ? Dim.Percent(60) + 1 : Dim.Percent(60) - 1;
            lbl.X = offset == null ? Pos.Percent(60) : offset.Value ? Pos.Percent(60) + 1 : Pos.Percent(60) - 1;
        }, out var lblOut);

        ClassicAssert.AreEqual(lblOut.Text, lblIn.Text);

        lblIn.Width.GetDimType(out var outDimType, out var outDimValue, out var outDimOffset);
        lblIn.X.GetPosType(new List<Design>(), out var outPosType, out var outPosValue, out var outPosOffset, out _, out _);

        ClassicAssert.AreEqual(DimType.Percent, outDimType);
        ClassicAssert.Less(Math.Abs(60f - outDimValue), 0.0001);

        ClassicAssert.AreEqual(PosType.Percent, outPosType);
        ClassicAssert.Less(Math.Abs(60f - outPosValue), 0.0001);
    }
}