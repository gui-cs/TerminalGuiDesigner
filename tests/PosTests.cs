using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;
public class PosTests : Tests
{
    [Test]
    public void TestIsAbsolute()
    {
        Assert.IsTrue(Pos.At(50).IsAbsolute());
        Assert.IsFalse(Pos.At(50).IsPercent());
        Assert.IsFalse(Pos.At(50).IsRelative(out _));
        Assert.IsFalse(Pos.At(50).IsAnchorEnd(out _));

        Assert.IsTrue(Pos.At(50).IsAbsolute(out int size));
        Assert.AreEqual(50, size);

        Assert.IsTrue(Pos.At(50).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        Assert.AreEqual(PosType.Absolute, type);
        Assert.AreEqual(50, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAbsolute_FromInt()
    {
        Pos p = 50;
        Assert.IsTrue(p.IsAbsolute());
        Assert.IsFalse(p.IsPercent());
        Assert.IsFalse(p.IsRelative(out _));
        Assert.IsFalse(p.IsAnchorEnd(out _));

        Assert.IsTrue(p.IsAbsolute(out int size));
        Assert.AreEqual(50, size);

        Assert.IsTrue(p.GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        Assert.AreEqual(PosType.Absolute, type);
        Assert.AreEqual(50, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsPercent()
    {
        Assert.IsFalse(Pos.Percent(24).IsAbsolute());
        Assert.IsTrue(Pos.Percent(24).IsPercent());
        Assert.IsFalse(Pos.Percent(24).IsRelative(out _));
        Assert.IsFalse(Pos.Percent(24).IsAnchorEnd(out _));

        Assert.IsTrue(Pos.Percent(24).IsPercent(out var size));
        Assert.AreEqual(24f, size);

        Assert.IsTrue(Pos.Percent(24).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        Assert.AreEqual(PosType.Percent, type);
        Assert.AreEqual(24, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsRelativeTo()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        Assert.IsFalse(Pos.Top(v).IsAbsolute());
        Assert.IsFalse(Pos.Top(v).IsPercent());
        Assert.IsTrue(Pos.Top(v).IsRelative(out _));
        Assert.IsFalse(Pos.Top(v).IsAnchorEnd(out _));

        Assert.IsTrue(Pos.Top(v).IsRelative(new List<Design> { d }, out var relativeTo, out var side));
        Assert.AreSame(d, relativeTo);
        Assert.AreEqual(Side.Top, side);

        Assert.IsTrue(Pos.Top(v).GetPosType(new List<Design> { d }, out var type, out var val, out relativeTo, out side, out var offset));
        Assert.AreEqual(PosType.Relative, type);
        Assert.AreSame(d, relativeTo);
        Assert.AreEqual(Side.Top, side);
    }

    [Test]
    public void TestIsAnchorEnd()
    {
        Assert.IsFalse(Pos.AnchorEnd().IsAbsolute());
        Assert.IsFalse(Pos.AnchorEnd().IsPercent());
        Assert.IsFalse(Pos.AnchorEnd().IsRelative(out _));
        Assert.IsTrue(Pos.AnchorEnd().IsAnchorEnd(out _));

        Assert.IsTrue(Pos.AnchorEnd().IsAnchorEnd(out var margin));
        Assert.AreEqual(0, margin);

        Assert.IsTrue(Pos.AnchorEnd().GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        Assert.AreEqual(PosType.AnchorEnd, type);
        Assert.AreEqual(0, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAnchorEnd_WithMargin()
    {
        Assert.IsFalse(Pos.AnchorEnd(2).IsAbsolute());
        Assert.IsFalse(Pos.AnchorEnd(2).IsPercent());
        Assert.IsFalse(Pos.AnchorEnd(2).IsRelative(out _));
        Assert.IsTrue(Pos.AnchorEnd(2).IsAnchorEnd(out _));

        Assert.IsTrue(Pos.AnchorEnd(2).IsAnchorEnd(out var margin));
        Assert.AreEqual(2, margin);

        Assert.IsTrue(Pos.AnchorEnd(2).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        Assert.AreEqual(PosType.AnchorEnd, type);
        Assert.AreEqual(2, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAnchorEnd_WithOffset()
    {
        Assert.IsTrue((Pos.AnchorEnd(1) + 2).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        Assert.AreEqual(PosType.AnchorEnd, type);
        Assert.AreEqual(1, val);
        Assert.AreEqual(2, offset);

        Assert.IsTrue((Pos.AnchorEnd(1) - 2).GetPosType(new List<Design>(), out type, out val, out design, out side, out offset));
        Assert.AreEqual(PosType.AnchorEnd, type);
        Assert.AreEqual(1, val);
        Assert.AreEqual(-2, offset);
    }

    [Test]
    public void TestGetPosType_WithOffset()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        var p = Pos.Percent(50) + 2;
        Assert.True(p.GetPosType(new List<Design> { d }, out PosType type, out float value, out var relativeTo, out var side, out int offset), $"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Percent, type);
        Assert.AreEqual(50, value);
        Assert.AreEqual(2, offset);

        p = Pos.Percent(50) - 2;
        Assert.True(p.GetPosType(new List<Design> { d }, out type, out value, out relativeTo, out side, out offset), $"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Percent, type);
        Assert.AreEqual(50, value);
        Assert.AreEqual(-2, offset);

        p = Pos.Top(v) + 2;
        Assert.True(p.GetPosType(new List<Design> { d }, out type, out value, out relativeTo, out side, out offset), $"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Relative, type);
        Assert.AreSame(d, relativeTo);
        Assert.AreEqual(Side.Top, side);
        Assert.AreEqual(2, offset);

        p = Pos.Top(v) - 2;
        Assert.True(p.GetPosType(new List<Design> { d }, out type, out value, out relativeTo, out side, out offset), $"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Relative, type);
        Assert.AreSame(d, relativeTo);
        Assert.AreEqual(Side.Top, side);
        Assert.AreEqual(-2, offset);
    }

    [Test]
    public void TestNullPos()
    {
        var v = new View();

        Assert.IsNull(v.X, "As of v1.7.0 a new View started getting null for its X, if this assert fails it means that behaviour was reverted and this test can be altered or suppressed");

        Assert.IsTrue(v.X.IsAbsolute());
        Assert.IsTrue(v.X.IsAbsolute(out int n));
        Assert.AreEqual(0, n);

        Assert.IsFalse(v.X.IsPercent());
        Assert.IsFalse(v.X.IsCombine());
        Assert.IsFalse(v.X.IsCenter());

        Assert.IsFalse(v.X.IsRelative(out var _));
        Assert.IsFalse(v.X.IsRelative(new List<Design>(), out _, out _));

        Assert.IsTrue(v.X.GetPosType(new List<Design>(), out var type, out var val, out _, out _, out _));

        Assert.AreEqual(PosType.Absolute, type);
        Assert.AreEqual(0, val);
    }

    [Test]
    public void TestGetCode_WithNoOffset()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        var p = Pos.Percent(50);
        Assert.AreEqual("Pos.Percent(50f)", p.ToCode(new List<Design> { d }));

        p = Pos.Left(v);
        Assert.AreEqual("Pos.Left(myView)", p.ToCode(new List<Design> { d }));
        p = Pos.Right(v);
        Assert.AreEqual("Pos.Right(myView)", p.ToCode(new List<Design> { d }));
        p = Pos.Bottom(v);
        Assert.AreEqual("Pos.Bottom(myView)", p.ToCode(new List<Design> { d }));
        p = Pos.Top(v);
        Assert.AreEqual("Pos.Top(myView)", p.ToCode(new List<Design> { d }));
    }

    [Test]
    public void TestGetCode_WithOffset()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        var p = Pos.Percent(50) + 2;
        Assert.AreEqual("Pos.Percent(50f) + 2", p.ToCode(new List<Design> { d }));

        p = Pos.Percent(50) - 2;
        Assert.AreEqual("Pos.Percent(50f) - 2", p.ToCode(new List<Design> { d }));

        p = Pos.Right(v) + 2;
        Assert.AreEqual("Pos.Right(myView) + 2", p.ToCode(new List<Design> { d }));

        p = Pos.Right(v) - 2;
        Assert.AreEqual("Pos.Right(myView) - 2", p.ToCode(new List<Design> { d }));
    }

    [TestCase(Side.Left, -2, "X")]
    [TestCase(Side.Right, 1, "X")]
    [TestCase(Side.Top, -2, "Y")]
    [TestCase(Side.Bottom, 5, "Y")]
    public void TestRoundTrip_PosRelative(Side side, int offset, string property)
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PosRelative.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window), out var sourceCode);

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        lbl.X = 50;
        lbl.Y = 50;

        var btn = factory.Create(typeof(Button));

        new AddViewOperation(sourceCode, lbl, designOut, "label1").Do();
        new AddViewOperation(sourceCode, btn, designOut, "btn").Do();

        if (property == "X")
        {
            btn.X = PosExtensions.CreatePosRelative((Design)lbl.Data, side, offset);
        }
        else if (property == "Y")
        {
            btn.Y = PosExtensions.CreatePosRelative((Design)lbl.Data, side, offset);
        }
        else
            throw new ArgumentException($"Unknown property for test '{property}'");

        viewToCode.GenerateDesignerCs(designOut, designOut.SourceCode, typeof(Window));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var btnIn = designBackIn.View.GetActualSubviews().OfType<Button>().Single();

        PosType backInType;
        Design? backInRelativeTo;
        Side backInSide;
        int backInOffset;

        if (property == "X")
        {
            btnIn.X.GetPosType(designBackIn.GetAllDesigns().ToList(), out backInType, out _, out backInRelativeTo, out backInSide, out backInOffset);
        }
        else
        {
            btnIn.Y.GetPosType(designBackIn.GetAllDesigns().ToList(), out backInType, out _, out backInRelativeTo, out backInSide, out backInOffset);
        }

        Assert.AreEqual(side, backInSide);
        Assert.AreEqual(PosType.Relative, backInType);
        Assert.AreEqual(offset, backInOffset);
        Assert.IsNotNull(backInRelativeTo);
        Assert.IsInstanceOf<Label>(backInRelativeTo?.View);
    }

    [Test]
    public void TestRoundTrip_PosAnchorEnd()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PosAnchorEnd.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window), out var sourceCode);

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        lbl.X = Pos.AnchorEnd(1);
        lbl.Y = Pos.AnchorEnd(4); // length of "Heya"

        new AddViewOperation(sourceCode, lbl, designOut, "label1").Do();

        viewToCode.GenerateDesignerCs(designOut, designOut.SourceCode, typeof(Window));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        lblIn.X.GetPosType(designBackIn.GetAllDesigns().ToList(), out var backInType, out var backInValue, out _, out _, out var backInOffset);
        Assert.AreEqual(0, backInOffset);
        Assert.AreEqual(PosType.AnchorEnd, backInType);
        Assert.AreEqual(1, backInValue);

        lblIn.Y.GetPosType(designBackIn.GetAllDesigns().ToList(), out backInType, out backInValue, out _, out _, out backInOffset);
        Assert.AreEqual(0, backInOffset);
        Assert.AreEqual(PosType.AnchorEnd, backInType);
        Assert.AreEqual(4, backInValue);
    }

    [Test]
    public void TestRoundTrip_PosAnchorEnd_WithOffset()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PosAnchorEnd.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window), out var sourceCode);

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        lbl.X = Pos.AnchorEnd(1) + 5;
        lbl.Y = Pos.AnchorEnd(4) - 3; // length of "Heya"

        new AddViewOperation(sourceCode, lbl, designOut, "label1").Do();

        viewToCode.GenerateDesignerCs(designOut, designOut.SourceCode, typeof(Window));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        lblIn.X.GetPosType(designBackIn.GetAllDesigns().ToList(), out var backInType, out var backInValue, out _, out _, out var backInOffset);
        Assert.AreEqual(5, backInOffset);
        Assert.AreEqual(PosType.AnchorEnd, backInType);
        Assert.AreEqual(1, backInValue);

        lblIn.Y.GetPosType(designBackIn.GetAllDesigns().ToList(), out backInType, out backInValue, out _, out _, out backInOffset);
        Assert.AreEqual(-3, backInOffset);
        Assert.AreEqual(PosType.AnchorEnd, backInType);
        Assert.AreEqual(4, backInValue);
    }
}