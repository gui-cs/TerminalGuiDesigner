using static Terminal.Gui.SpinnerStyle;

namespace UnitTests;

[TestFixture]
[Category("Core")]
internal class ColorPickerTests : Tests
{
    [Test]
    public void ColorPickerHeightStaysAuto()
    {
        Assume.That(ViewFactory.SupportedViewTypes, Does.Contain(typeof(ColorPicker)));
        
        using (var cp = ViewFactory.Create<ColorPicker>())
        {
            Assert.That(cp.Height?.IsAuto(out _,out _, out _),Is.True);
        }
    }


    [Test]
    [Category("Code Generation")]
    public void ColorPickerCanSerializeStyle()
    {
        using var backIn = RoundTrip<View, ColorPicker>(static (d, v) =>
        {
            Assume.That(v.Style.ColorModel,Is.Not.EqualTo(ColorModel.RGB));

            var prop = d.GetDesignableProperty(nameof(ColorPickerStyle.ColorModel))
                       ?? throw new("Property was unexpectedly not designable");

            // We change to RGB
            prop.SetValue(ColorModel.RGB);

            Assert.That(v.Style.ColorModel, Is.EqualTo(ColorModel.RGB));
        }, out _);

        // Reloaded code from .Designer.cs should match
        Assert.That(backIn.Style.ColorModel, Is.EqualTo(ColorModel.RGB));
    }
}