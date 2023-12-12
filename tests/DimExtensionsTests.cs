using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(DimExtensions))]
[Category("Core")]
internal class DimExtensionsTests
{
    [Test]
    public void IsAbsolute_ReturnsTrue_IfSized([Range(-15,15,5)] int size )
    {
        Assert.That( Dim.Sized( size ).IsAbsolute );
    }

    [Test]
    public void IsFill_ReturnsFalse_IfSized( [Range( -15, 15, 5 )] int size )
    {
        Assert.That( Dim.Sized( size ).IsFill( ), Is.False );
    }

    [Test]
    public void IsPercent_ReturnsFalse_IfSized( [Range( -15, 15, 5 )] int size )
    {
        Assert.That( Dim.Sized( size ).IsPercent( ), Is.False );
    }

    [Test]
    public void IsPercent_ReturnsExpectedValue_IfSized( [Range( -15, 15, 5 )] int size )
    {
        Assert.Multiple( ( ) =>
        {
            Assert.That( Dim.Sized( size ).IsPercent( out float percent ), Is.False );
            Assert.That( percent, Is.Zero );
            Assert.That( Dim.Sized( size ).IsAbsolute( out int outSize ) );
            Assert.That( outSize, Is.EqualTo( size ) );
        } );
    }

    [Test]
    public void GetDimType_IsNotPercent_IfSized( [Range( -15, 15, 5 )] int size )
    {
        Dim.Sized( size ).GetDimType( out DimType type, out _, out _ );
        Assert.That( type, Is.Not.EqualTo( DimType.Percent ) );
    }

    [Test]
    public void GetDimType_ReturnsExpectedValues_IfSized( [Range( -15, 15, 5 )] int size )
    {
        Assert.That( Dim.Sized( size ).GetDimType( out DimType type, out float val, out int offset ) );
        Assert.That( type, Is.EqualTo( DimType.Absolute ) );
        Assert.That( val, Is.EqualTo( size ) );
        Assert.That( offset, Is.Zero );
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
}