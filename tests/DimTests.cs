using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(DimExtensions))]
[Category("Core")]
internal class DimTests
{
    [Test]
    public void TestIsAbsolute()
    {
        ClassicAssert.IsTrue(Dim.Sized(50).IsAbsolute());
        this.AssertIsNotPercent(Dim.Sized(50));
        ClassicAssert.IsFalse(Dim.Sized(50).IsFill());

        ClassicAssert.IsTrue(Dim.Sized(50).IsAbsolute(out int size));
        ClassicAssert.AreEqual(50, size);

        ClassicAssert.IsTrue(Dim.Sized(50).GetDimType(out var type, out var val, out var offset));
        ClassicAssert.AreEqual(DimType.Absolute, type);
        ClassicAssert.AreEqual(50, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsAbsolute_FromInt()
    {
        Dim d = 50;
        ClassicAssert.IsTrue(d.IsAbsolute());
        ClassicAssert.IsFalse(d.IsPercent());
        ClassicAssert.IsFalse(d.IsFill());

        ClassicAssert.IsTrue(d.IsAbsolute(out int size));
        ClassicAssert.AreEqual(50, size);

        ClassicAssert.IsTrue(d.GetDimType(out var type, out var val, out var offset));
        ClassicAssert.AreEqual(DimType.Absolute, type);
        ClassicAssert.AreEqual(50, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsPercent()
    {
        ClassicAssert.IsFalse(Dim.Percent(24).IsAbsolute());
        ClassicAssert.IsTrue(Dim.Percent(24).IsPercent());
        ClassicAssert.IsFalse(Dim.Percent(24).IsFill());

        ClassicAssert.IsTrue(Dim.Percent(24).IsPercent(out var size));
        ClassicAssert.AreEqual(24f, size);

        ClassicAssert.IsTrue(Dim.Percent(24).GetDimType(out var type, out var val, out var offset));
        ClassicAssert.AreEqual(DimType.Percent, type);
        ClassicAssert.AreEqual(24, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestIsFill()
    {
        ClassicAssert.IsFalse(Dim.Fill(2).IsAbsolute());
        ClassicAssert.IsFalse(Dim.Fill(2).IsPercent());
        ClassicAssert.IsTrue(Dim.Fill(2).IsFill());

        ClassicAssert.IsTrue(Dim.Fill(5).IsFill(out var margin));
        ClassicAssert.AreEqual(5, margin);

        ClassicAssert.IsTrue(Dim.Fill(5).GetDimType(out var type, out var val, out var offset));
        ClassicAssert.AreEqual(DimType.Fill, type);
        ClassicAssert.AreEqual(5, val);
        ClassicAssert.AreEqual(0, offset);
    }

    [Test]
    public void TestNullDim()
    {
        var v = new View();

        ClassicAssert.IsNull(v.Width, "As of v1.7.0 a new View started getting null for its Width, if this assert fails it means that behaviour was reverted and this test can be altered or suppressed");

        ClassicAssert.IsTrue(v.Width.IsAbsolute());
        ClassicAssert.IsTrue(v.Width.IsAbsolute(out int n));
        ClassicAssert.AreEqual(0, n);

        ClassicAssert.IsFalse(v.Width.IsFill());
        ClassicAssert.IsFalse(v.Width.IsPercent());
        ClassicAssert.IsFalse(v.Width.IsCombine());

        ClassicAssert.IsTrue(v.Width.GetDimType(out var type, out var val, out _));

        ClassicAssert.AreEqual(DimType.Absolute, type);
        ClassicAssert.AreEqual(0, val);
    }

    [Test]
    public void TestGetDimType_WithOffset()
    {
        var d = Dim.Percent(50) + 2;
        ClassicAssert.True(d.GetDimType(out DimType type, out float value, out int offset), $"Could not figure out DimType for '{d}'");
        ClassicAssert.AreEqual(DimType.Percent, type);
        ClassicAssert.AreEqual(50, value);
        ClassicAssert.AreEqual(2, offset);

        d = Dim.Percent(50) - 2;
        ClassicAssert.True(d.GetDimType(out type, out value, out offset), $"Could not figure out DimType for '{d}'");
        ClassicAssert.AreEqual(DimType.Percent, type);
        ClassicAssert.AreEqual(50, value);
        ClassicAssert.AreEqual(-2, offset);

        d = Dim.Fill(5) + 2;
        ClassicAssert.True(d.GetDimType(out type, out value, out offset), $"Could not figure out DimType for '{d}'");
        ClassicAssert.AreEqual(DimType.Fill, type);
        ClassicAssert.AreEqual(5, value);
        ClassicAssert.AreEqual(2, offset);

        d = Dim.Fill(5) - 2;
        ClassicAssert.True(d.GetDimType(out type, out value, out offset), $"Could not figure out DimType for '{d}'");
        ClassicAssert.AreEqual(DimType.Fill, type);
        ClassicAssert.AreEqual(5, value);
        ClassicAssert.AreEqual(-2, offset);
    }

    [Test]
    public void TestGetCode_WithNoOffset()
    {
        var d = Dim.Percent(50);
        ClassicAssert.AreEqual("Dim.Percent(50f)", d.ToCode());

        d = Dim.Percent(50);
        ClassicAssert.AreEqual("Dim.Percent(50f)", d.ToCode());

        d = Dim.Fill(5);
        ClassicAssert.AreEqual("Dim.Fill(5)", d.ToCode());

        d = Dim.Fill(5);
        ClassicAssert.AreEqual("Dim.Fill(5)", d.ToCode());
    }

    [Test]
    public void TestGetCode_WithOffset()
    {
        var d = Dim.Percent(50) + 2;
        ClassicAssert.AreEqual("Dim.Percent(50f) + 2", d.ToCode());

        d = Dim.Percent(50) - 2;
        ClassicAssert.AreEqual("Dim.Percent(50f) - 2", d.ToCode());

        d = Dim.Fill(5) + 2;
        ClassicAssert.AreEqual("Dim.Fill(5) + 2", d.ToCode());

        d = Dim.Fill(5) - 2;
        ClassicAssert.AreEqual("Dim.Fill(5) - 2", d.ToCode());
    }

    private void AssertIsNotPercent(Dim dim)
    {
        ClassicAssert.IsFalse(dim.IsPercent());
        ClassicAssert.IsFalse(dim.IsPercent(out var percent));
        ClassicAssert.AreEqual(0, percent);

        dim.GetDimType(out DimType type, out _, out _);
        ClassicAssert.AreNotEqual(DimType.Percent, type);
    }
}