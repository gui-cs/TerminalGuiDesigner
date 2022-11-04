using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace tests;
public class DimTests
{
    [Test]
    public void TestIsAbsolute()
    {
        Assert.IsTrue(Dim.Sized(50).IsAbsolute());
        Assert.IsFalse(Dim.Sized(50).IsPercent());
        Assert.IsFalse(Dim.Sized(50).IsFill());

        Assert.IsTrue(Dim.Sized(50).IsAbsolute(out int size));
        Assert.AreEqual(50, size);

        Assert.IsTrue(Dim.Sized(50).GetDimType(out var type, out var val, out var offset));
        Assert.AreEqual(DimType.Absolute, type);
        Assert.AreEqual(50, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAbsolute_FromInt()
    {
        Dim d = 50;
        Assert.IsTrue(d.IsAbsolute());
        Assert.IsFalse(d.IsPercent());
        Assert.IsFalse(d.IsFill());

        Assert.IsTrue(d.IsAbsolute(out int size));
        Assert.AreEqual(50, size);

        Assert.IsTrue(d.GetDimType(out var type, out var val, out var offset));
        Assert.AreEqual(DimType.Absolute, type);
        Assert.AreEqual(50, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsPercent()
    {
        Assert.IsFalse(Dim.Percent(24).IsAbsolute());
        Assert.IsTrue(Dim.Percent(24).IsPercent());
        Assert.IsFalse(Dim.Percent(24).IsFill());

        Assert.IsTrue(Dim.Percent(24).IsPercent(out var size));
        Assert.AreEqual(24f, size);

        Assert.IsTrue(Dim.Percent(24).GetDimType(out var type, out var val, out var offset));
        Assert.AreEqual(DimType.Percent, type);
        Assert.AreEqual(24, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsFill()
    {
        Assert.IsFalse(Dim.Fill(2).IsAbsolute());
        Assert.IsFalse(Dim.Fill(2).IsPercent());
        Assert.IsTrue(Dim.Fill(2).IsFill());

        Assert.IsTrue(Dim.Fill(5).IsFill(out var margin));
        Assert.AreEqual(5, margin);

        Assert.IsTrue(Dim.Fill(5).GetDimType(out var type, out var val, out var offset));
        Assert.AreEqual(DimType.Fill, type);
        Assert.AreEqual(5, val);
        Assert.AreEqual(0, offset);
    }

    [Test]
    public void TestNullDim()
    {
        var v = new View();

        Assert.IsNull(v.Width, "As of v1.7.0 a new View started getting null for its Width, if this assert fails it means that behaviour was reverted and this test can be altered or suppressed");

        Assert.IsTrue(v.Width.IsAbsolute());
        Assert.IsTrue(v.Width.IsAbsolute(out int n));
        Assert.AreEqual(0, n);

        Assert.IsFalse(v.Width.IsFill());
        Assert.IsFalse(v.Width.IsPercent());
        Assert.IsFalse(v.Width.IsCombine());

        Assert.IsTrue(v.Width.GetDimType(out var type, out var val, out _));

        Assert.AreEqual(DimType.Absolute, type);
        Assert.AreEqual(0, val);
    }

    [Test]
    public void TestGetDimType_WithOffset()
    {
        var d = Dim.Percent(50) + 2;
        Assert.True(d.GetDimType(out DimType type, out float value, out int offset), $"Could not figure out DimType for '{d}'");
        Assert.AreEqual(DimType.Percent, type);
        Assert.AreEqual(50, value);
        Assert.AreEqual(2, offset);

        d = Dim.Percent(50) - 2;
        Assert.True(d.GetDimType(out type, out value, out offset), $"Could not figure out DimType for '{d}'");
        Assert.AreEqual(DimType.Percent, type);
        Assert.AreEqual(50, value);
        Assert.AreEqual(-2, offset);

        d = Dim.Fill(5) + 2;
        Assert.True(d.GetDimType(out type, out value, out offset), $"Could not figure out DimType for '{d}'");
        Assert.AreEqual(DimType.Fill, type);
        Assert.AreEqual(5, value);
        Assert.AreEqual(2, offset);

        d = Dim.Fill(5) - 2;
        Assert.True(d.GetDimType(out type, out value, out offset), $"Could not figure out DimType for '{d}'");
        Assert.AreEqual(DimType.Fill, type);
        Assert.AreEqual(5, value);
        Assert.AreEqual(-2, offset);
    }

    [Test]
    public void TestGetCode_WithNoOffset()
    {
        var d = Dim.Percent(50);
        Assert.AreEqual("Dim.Percent(50f)", d.ToCode());

        d = Dim.Percent(50);
        Assert.AreEqual("Dim.Percent(50f)", d.ToCode());

        d = Dim.Fill(5);
        Assert.AreEqual("Dim.Fill(5)", d.ToCode());

        d = Dim.Fill(5);
        Assert.AreEqual("Dim.Fill(5)", d.ToCode());
    }

    [Test]
    public void TestGetCode_WithOffset()
    {
        var d = Dim.Percent(50) + 2;
        Assert.AreEqual("Dim.Percent(50f) + 2", d.ToCode());

        d = Dim.Percent(50) - 2;
        Assert.AreEqual("Dim.Percent(50f) - 2", d.ToCode());

        d = Dim.Fill(5) + 2;
        Assert.AreEqual("Dim.Fill(5) + 2", d.ToCode());

        d = Dim.Fill(5) - 2;
        Assert.AreEqual("Dim.Fill(5) - 2", d.ToCode());
    }
}