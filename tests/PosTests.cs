using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace tests;

public class PosTests
{
    [Test]
    public void TestIsAbsolute()
    {
        Assert.IsTrue(Pos.At(50).IsAbsolute());
        Assert.IsFalse(Pos.At(50).IsPercent());
        Assert.IsFalse(Pos.At(50).IsRelative(out _));

        Assert.IsTrue(Pos.At(50).IsAbsolute(out int size));
        Assert.AreEqual(50,size);

        Assert.IsTrue(Pos.At(50).GetPosType(new List<Design>(),out var type, out var val,out var design, out var side, out var offset));
        Assert.AreEqual(PosType.Absolute,type);
        Assert.AreEqual(50,val);
        Assert.AreEqual(0,offset);
    }

    [Test]
    public void TestIsAbsolute_FromInt()
    {
        Pos p = 50;
        Assert.IsTrue(p.IsAbsolute());
        Assert.IsFalse(p.IsPercent());
        Assert.IsFalse(p.IsRelative(out _));

        Assert.IsTrue(p.IsAbsolute(out int size));
        Assert.AreEqual(50,size);

        Assert.IsTrue(p.GetPosType(new List<Design>(), out var type, out var val,out var design, out var side, out var offset));
        Assert.AreEqual(PosType.Absolute,type);
        Assert.AreEqual(50,val);
        Assert.AreEqual(0,offset);
    }

    [Test]
    public void TestIsPercent()
    {
        Assert.IsFalse(Pos.Percent(24).IsAbsolute());
        Assert.IsTrue(Pos.Percent(24).IsPercent());
        Assert.IsFalse(Pos.Percent(24).IsRelative(out _));

        Assert.IsTrue(Pos.Percent(24).IsPercent(out var size));
        Assert.AreEqual(24f,size);

        Assert.IsTrue(Pos.Percent(24).GetPosType(new List<Design>(),out var type, out var val,out var design, out var side, out var offset));
        Assert.AreEqual(PosType.Percent,type);
        Assert.AreEqual(24,val);
        Assert.AreEqual(0,offset);
    }


    [Test]
    public void TestIsRelativeTo()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")),"myView",v);

        Assert.IsFalse(Pos.Top(v).IsAbsolute());
        Assert.IsFalse(Pos.Top(v).IsPercent());
        Assert.IsTrue(Pos.Top(v).IsRelative(out _));

        Assert.IsTrue(Pos.Top(v).IsRelative(new List<Design>{d},out var relativeTo, out var side));
        Assert.AreSame(d,relativeTo);
        Assert.AreEqual(Side.Top,side);

        Assert.IsTrue(Pos.Top(v).GetPosType(new List<Design>{d},out var type, out var val,out relativeTo, out side, out var offset));
        Assert.AreEqual(PosType.Relative,type);
        Assert.AreSame(d,relativeTo);
        Assert.AreEqual(Side.Top,side);
    }

    [Test]
    public void TestGetPosType_WithOffset()
    {
        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")),"myView",v);

        var p = Pos.Percent(50) + 2;
        Assert.True(p.GetPosType(new List<Design>{d},out PosType type,out float value,out var relativeTo,out var side, out int offset),$"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Percent,type);
        Assert.AreEqual(50,value);
        Assert.AreEqual(2,offset);

        p = Pos.Percent(50) - 2;
        Assert.True(p.GetPosType(new List<Design>{d},out type,out value,out relativeTo,out side, out offset),$"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Percent,type);
        Assert.AreEqual(50,value);
        Assert.AreEqual(-2,offset);

        p = Pos.Top(v) + 2;
        Assert.True(p.GetPosType(new List<Design>{d},out type,out value,out relativeTo,out side, out offset),$"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Relative,type);
        Assert.AreSame(d,relativeTo);
        Assert.AreEqual(Side.Top,side);
        Assert.AreEqual(2,offset);

        p = Pos.Top(v) - 2;
        Assert.True(p.GetPosType(new List<Design>{d},out type,out value,out relativeTo,out side, out offset),$"Could not figure out PosType for '{p}'");
        Assert.AreEqual(PosType.Relative,type);
        Assert.AreSame(d,relativeTo);
        Assert.AreEqual(Side.Top,side);
        Assert.AreEqual(-2,offset);
    }

    [Test]
    public void TestGetCode_WithNoOffset()
    {

        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")),"myView",v);

        var p = Pos.Percent(50);
        Assert.AreEqual("Pos.Percent(50)",p.ToCode(new List<Design>{d}));

        p = Pos.Left(v);
        Assert.AreEqual("Pos.Left(myView)",p.ToCode(new List<Design>{d}));
        p = Pos.Right(v);
        Assert.AreEqual("Pos.Right(myView)",p.ToCode(new List<Design>{d}));
        p = Pos.Bottom(v);
        Assert.AreEqual("Pos.Bottom(myView)",p.ToCode(new List<Design>{d}));
        p = Pos.Top(v);
        Assert.AreEqual("Pos.Top(myView)",p.ToCode(new List<Design>{d}));

    }


    [Test]
    public void TestGetCode_WithOffset()
    {

        View v = new View();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")),"myView",v);

        var p = Pos.Percent(50) + 2;
        Assert.AreEqual("Pos.Percent(50) + 2",p.ToCode(new List<Design>{d}));

        p = Pos.Percent(50) - 2;
        Assert.AreEqual("Pos.Percent(50) - 2",p.ToCode(new List<Design>{d}));

        p = Pos.Right(v) + 2;
        Assert.AreEqual("Pos.Right(myView) + 2",p.ToCode(new List<Design>{d}));

        p = Pos.Right(v) - 2;
        Assert.AreEqual("Pos.Right(myView) - 2",p.ToCode(new List<Design>{d}));
    }
}