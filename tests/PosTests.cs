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
internal class PosTests : Tests
{
    [Test]
    public void TestIsAbsolute()
    {
        ClassicAssert.IsTrue(Pos.At(50).IsAbsolute());
        ClassicAssert.IsFalse(Pos.At(50).IsPercent());
        ClassicAssert.IsFalse(Pos.At(50).IsRelative());
        ClassicAssert.IsFalse(Pos.At(50).IsAnchorEnd(out _));

        ClassicAssert.IsTrue(Pos.At(50).IsAbsolute(out int size));
        ClassicAssert.AreEqual(50, size);

        ClassicAssert.IsTrue(Pos.At(50).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        ClassicAssert.AreEqual(PosType.Absolute, type);
        ClassicAssert.AreEqual(50, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAbsolute_FromInt()
    {
        Pos p = 50;
        ClassicAssert.IsTrue(p.IsAbsolute());
        ClassicAssert.IsFalse(p.IsPercent());
        ClassicAssert.IsFalse(p.IsRelative());
        ClassicAssert.IsFalse(p.IsAnchorEnd(out _));

        ClassicAssert.IsTrue(p.IsAbsolute(out int size));
        ClassicAssert.AreEqual(50, size);

        ClassicAssert.IsTrue(p.GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        ClassicAssert.AreEqual(PosType.Absolute, type);
        ClassicAssert.AreEqual(50, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsPercent()
    {
        ClassicAssert.IsFalse(Pos.Percent(24).IsAbsolute());
        ClassicAssert.IsTrue(Pos.Percent(24).IsPercent());
        ClassicAssert.IsFalse(Pos.Percent(24).IsRelative());
        ClassicAssert.IsFalse(Pos.Percent(24).IsAnchorEnd(out _));

        ClassicAssert.IsTrue(Pos.Percent(24).IsPercent(out var size));
        ClassicAssert.AreEqual(24f, size);

        ClassicAssert.IsTrue(Pos.Percent(24).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        ClassicAssert.AreEqual(PosType.Percent, type);
        ClassicAssert.AreEqual(24, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsRelativeTo()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        ClassicAssert.IsFalse(Pos.Top(v).IsAbsolute());
        ClassicAssert.IsFalse(Pos.Top(v).IsPercent());
        ClassicAssert.IsTrue(Pos.Top(v).IsRelative());
        ClassicAssert.IsFalse(Pos.Top(v).IsAnchorEnd(out _));

        ClassicAssert.IsTrue(Pos.Top(v).IsRelative(new List<Design> { d }, out var relativeTo, out var side));
        ClassicAssert.AreSame(d, relativeTo);
        ClassicAssert.AreEqual(Side.Top, side);

        ClassicAssert.IsTrue(Pos.Top(v).GetPosType(new List<Design> { d }, out var type, out var val, out relativeTo, out side, out var offset));
        ClassicAssert.AreEqual(PosType.Relative, type);
        ClassicAssert.AreSame(d, relativeTo);
        ClassicAssert.AreEqual(Side.Top, side);
    }

    [Test]
    public void TestIsAnchorEnd()
    {
        ClassicAssert.IsFalse(Pos.AnchorEnd().IsAbsolute());
        ClassicAssert.IsFalse(Pos.AnchorEnd().IsPercent());
        ClassicAssert.IsFalse(Pos.AnchorEnd().IsRelative());
        ClassicAssert.IsTrue(Pos.AnchorEnd().IsAnchorEnd(out _));

        ClassicAssert.IsTrue(Pos.AnchorEnd().IsAnchorEnd(out var margin));
        ClassicAssert.AreEqual(0, margin);

        ClassicAssert.IsTrue(Pos.AnchorEnd().GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        ClassicAssert.AreEqual(PosType.AnchorEnd, type);
        ClassicAssert.AreEqual(0, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAnchorEnd_WithMargin()
    {
        ClassicAssert.IsFalse(Pos.AnchorEnd(2).IsAbsolute());
        ClassicAssert.IsFalse(Pos.AnchorEnd(2).IsPercent());
        ClassicAssert.IsFalse(Pos.AnchorEnd(2).IsRelative());
        ClassicAssert.IsTrue(Pos.AnchorEnd(2).IsAnchorEnd(out _));

        ClassicAssert.IsTrue(Pos.AnchorEnd(2).IsAnchorEnd(out var margin));
        ClassicAssert.AreEqual(2, margin);

        ClassicAssert.IsTrue(Pos.AnchorEnd(2).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        ClassicAssert.AreEqual(PosType.AnchorEnd, type);
        ClassicAssert.AreEqual(2, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAnchorEnd_WithOffset()
    {
        ClassicAssert.IsTrue((Pos.AnchorEnd(1) + 2).GetPosType(new List<Design>(), out var type, out var val, out var design, out var side, out var offset));
        ClassicAssert.AreEqual(PosType.AnchorEnd, type);
        ClassicAssert.AreEqual(1, val);
        ClassicAssert.AreEqual(2, offset);

        ClassicAssert.IsTrue((Pos.AnchorEnd(1) - 2).GetPosType(new List<Design>(), out type, out val, out design, out side, out offset));
        ClassicAssert.AreEqual(PosType.AnchorEnd, type);
        ClassicAssert.AreEqual(1, val);
        ClassicAssert.AreEqual(-2, offset);
    }

    [Test]
    public void TestGetPosType_WithOffset()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        var p = Pos.Percent(50) + 2;
        ClassicAssert.True(p.GetPosType(new List<Design> { d }, out PosType type, out float value, out var relativeTo, out var side, out int offset), $"Could not figure out PosType for '{p}'");
        ClassicAssert.AreEqual(PosType.Percent, type);
        ClassicAssert.AreEqual(50, value);
        ClassicAssert.AreEqual(2, offset);

        p = Pos.Percent(50) - 2;
        ClassicAssert.True(p.GetPosType(new List<Design> { d }, out type, out value, out relativeTo, out side, out offset), $"Could not figure out PosType for '{p}'");
        ClassicAssert.AreEqual(PosType.Percent, type);
        ClassicAssert.AreEqual(50, value);
        ClassicAssert.AreEqual(-2, offset);

        p = Pos.Top(v) + 2;
        ClassicAssert.True(p.GetPosType(new List<Design> { d }, out type, out value, out relativeTo, out side, out offset), $"Could not figure out PosType for '{p}'");
        ClassicAssert.AreEqual(PosType.Relative, type);
        ClassicAssert.AreSame(d, relativeTo);
        ClassicAssert.AreEqual(Side.Top, side);
        ClassicAssert.AreEqual(2, offset);

        p = Pos.Top(v) - 2;
        ClassicAssert.True(p.GetPosType(new List<Design> { d }, out type, out value, out relativeTo, out side, out offset), $"Could not figure out PosType for '{p}'");
        ClassicAssert.AreEqual(PosType.Relative, type);
        ClassicAssert.AreSame(d, relativeTo);
        ClassicAssert.AreEqual(Side.Top, side);
        ClassicAssert.AreEqual(-2, offset);
    }

    [Test]
    public void TestNullPos()
    {
        var v = new View();

        ClassicAssert.IsNull(v.X, "As of v1.7.0 a new View started getting null for its X, if this assert fails it means that behaviour was reverted and this test can be altered or suppressed");

        ClassicAssert.IsTrue(v.X.IsAbsolute());
        ClassicAssert.IsTrue(v.X.IsAbsolute(out int n));
        ClassicAssert.AreEqual(0, n);

        ClassicAssert.IsFalse(v.X.IsPercent());
        ClassicAssert.IsFalse(v.X.IsCombine());
        ClassicAssert.IsFalse(v.X.IsCenter());

        ClassicAssert.IsFalse(v.X.IsRelative());
        ClassicAssert.IsFalse(v.X.IsRelative(new List<Design>(), out _, out _));

        ClassicAssert.IsTrue(v.X.GetPosType(new List<Design>(), out var type, out var val, out _, out _, out _));

        ClassicAssert.AreEqual(PosType.Absolute, type);
        ClassicAssert.AreEqual(0, val);
    }

    [Test]
    public void TestGetCode_WithNoOffset()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        var p = Pos.Percent(50);
        ClassicAssert.AreEqual("Pos.Percent(50f)", p.ToCode(new List<Design> { d }));

        p = Pos.Left(v);
        ClassicAssert.AreEqual("Pos.Left(myView)", p.ToCode(new List<Design> { d }));
        p = Pos.Right(v);
        ClassicAssert.AreEqual("Pos.Right(myView)", p.ToCode(new List<Design> { d }));
        p = Pos.Bottom(v);
        ClassicAssert.AreEqual("Pos.Bottom(myView)", p.ToCode(new List<Design> { d }));
        p = Pos.Top(v);
        ClassicAssert.AreEqual("Pos.Top(myView)", p.ToCode(new List<Design> { d }));
    }

    [Test]
    public void TestGetCode_WithOffset()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "myView", v);

        var p = Pos.Percent(50) + 2;
        ClassicAssert.AreEqual("Pos.Percent(50f) + 2", p.ToCode(new List<Design> { d }));

        p = Pos.Percent(50) - 2;
        ClassicAssert.AreEqual("Pos.Percent(50f) - 2", p.ToCode(new List<Design> { d }));

        p = Pos.Right(v) + 2;
        ClassicAssert.AreEqual("Pos.Right(myView) + 2", p.ToCode(new List<Design> { d }));

        p = Pos.Right(v) - 2;
        ClassicAssert.AreEqual("Pos.Right(myView) - 2", p.ToCode(new List<Design> { d }));
    }

    [TestCase(Side.Left, -2, "X")]
    [TestCase(Side.Right, 1, "X")]
    [TestCase(Side.Top, -2, "Y")]
    [TestCase(Side.Bottom, 5, "Y")]
    public void TestRoundTrip_PosRelative(Side side, int offset, string property)
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PosRelative.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        lbl.X = 50;
        lbl.Y = 50;

        var btn = factory.Create(typeof(Button));

        new AddViewOperation(lbl, designOut, "label1").Do();
        new AddViewOperation(btn, designOut, "btn").Do();

        if (property == "X")
        {
            btn.X = PosExtensions.CreatePosRelative((Design)lbl.Data, side, offset);
        }
        else if (property == "Y")
        {
            btn.Y = PosExtensions.CreatePosRelative((Design)lbl.Data, side, offset);
        }
        else
        {
            throw new ArgumentException($"Unknown property for test '{property}'");
        }

        viewToCode.GenerateDesignerCs(designOut, typeof(Window));

        var codeToView = new CodeToView(designOut.SourceCode);
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

        ClassicAssert.AreEqual(side, backInSide);
        ClassicAssert.AreEqual(PosType.Relative, backInType);
        ClassicAssert.AreEqual(offset, backInOffset);
        ClassicAssert.IsNotNull(backInRelativeTo);
        ClassicAssert.IsInstanceOf<Label>(backInRelativeTo?.View);
    }

    [Test]
    public void TestRoundTrip_PosAnchorEnd()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PosAnchorEnd.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        lbl.X = Pos.AnchorEnd(1);
        lbl.Y = Pos.AnchorEnd(4); // length of "Heya"

        new AddViewOperation(lbl, designOut, "label1").Do();

        viewToCode.GenerateDesignerCs(designOut, typeof(Window));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        lblIn.X.GetPosType(designBackIn.GetAllDesigns().ToList(), out var backInType, out var backInValue, out _, out _, out var backInOffset);
        ClassicAssert.AreEqual(0, backInOffset);
        ClassicAssert.AreEqual(PosType.AnchorEnd, backInType);
        ClassicAssert.AreEqual(1, backInValue);

        lblIn.Y.GetPosType(designBackIn.GetAllDesigns().ToList(), out backInType, out backInValue, out _, out _, out backInOffset);
        ClassicAssert.AreEqual(0, backInOffset);
        ClassicAssert.AreEqual(PosType.AnchorEnd, backInType);
        ClassicAssert.AreEqual(4, backInValue);
    }

    [Test]
    public void TestRoundTrip_PosAnchorEnd_WithOffset()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PosAnchorEnd.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

        designOut.View.Width = 100;
        designOut.View.Height = 100;

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        lbl.X = Pos.AnchorEnd(1) + 5;
        lbl.Y = Pos.AnchorEnd(4) - 3; // length of "Heya"

        new AddViewOperation(lbl, designOut, "label1").Do();

        viewToCode.GenerateDesignerCs(designOut, typeof(Window));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        lblIn.X.GetPosType(designBackIn.GetAllDesigns().ToList(), out var backInType, out var backInValue, out _, out _, out var backInOffset);
        ClassicAssert.AreEqual(5, backInOffset);
        ClassicAssert.AreEqual(PosType.AnchorEnd, backInType);
        ClassicAssert.AreEqual(1, backInValue);

        lblIn.Y.GetPosType(designBackIn.GetAllDesigns().ToList(), out backInType, out backInValue, out _, out _, out backInOffset);
        ClassicAssert.AreEqual(-3, backInOffset);
        ClassicAssert.AreEqual(PosType.AnchorEnd, backInType);
        ClassicAssert.AreEqual(4, backInValue);
    }
}