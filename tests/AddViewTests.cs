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
[TestFixture]
internal class AddViewTests : Tests
{
    [Test]
    public void TestAdd_Undo()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAdd_Undo.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var lbl = ViewFactory.Create(typeof(Label));
        var op = new AddViewOperation(lbl, designOut, "label1");

        OperationManager.Instance.Do(op);
        Assert.That( designOut.View.GetActualSubviews( ).OfType<Label>( ).Count( ), Is.EqualTo( 1 ) );

        OperationManager.Instance.Undo();
        Assert.That( designOut.View.GetActualSubviews().OfType<Label>(), Is.Empty );

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        Assert.That( designBackIn.View.GetActualSubviews().OfType<Label>(), Is.Empty );
    }

    [Test]
    public void TestAddUndoRedo_RoundTrip()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAddUndoRedo_RoundTrip.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var lbl = ViewFactory.Create(typeof(Label));
        var op = new AddViewOperation(lbl, designOut, "label1");

        OperationManager.Instance.Do(op);
        OperationManager.Instance.Undo();
        OperationManager.Instance.Redo();

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var lblOut = designOut.View.GetActualSubviews().OfType<Label>().Single();

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        Assert.That( lblIn.Text, Is.EqualTo( lblOut.Text ) );
    }

    /// <summary>
    /// 60 is one of those numbers that can't be modeled exactly in float so ends
    /// up with many decimal places.  This needs to be rounded in the code generated
    /// and has to have the suffix 'f' to ensure that the value is treated as a float
    /// and not a double (which won't compile)
    /// </summary>
    /// <param name="offset"></param>
    [Test]
    public void Test60Percent_RoundTrip([Values]bool? offset)
    {
        var lblIn = this.RoundTrip<Dialog, Label>(
            (d, lbl) =>
        {
            lbl.Width = offset == null ? Dim.Percent(60) : offset.Value ? Dim.Percent(60) + 1 : Dim.Percent(60) - 1;
            lbl.X = offset == null ? Pos.Percent(60) : offset.Value ? Pos.Percent(60) + 1 : Pos.Percent(60) - 1;
        }, out var lblOut);

        Assert.That( lblIn.Text, Is.EqualTo( lblOut.Text ) );

        lblIn.Width.GetDimType(out var outDimType, out var outDimValue, out var outDimOffset);
        lblIn.X.GetPosType(new List<Design>(), out var outPosType, out var outPosValue, out var outPosOffset, out _, out _);

        Assert.Multiple( ( ) =>
        {
            Assert.That( outDimType, Is.EqualTo( DimType.Percent ) );
            Assert.That( 60f - outDimValue, Is.Zero.Within( 0.0001 ) );

            Assert.That( outPosType, Is.EqualTo( PosType.Percent ) );
            Assert.That( 60f - outPosValue, Is.Zero.Within( 0.0001 ) );
        } );
    }
}