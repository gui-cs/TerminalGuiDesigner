using static Terminal.Gui.SpinnerStyle;

namespace UnitTests;

[TestFixture]
[Category( "Core" )]
internal class SpinnerViewTests : Tests
{
    [Test]
    public void NewSpinnerAutoSpins()
    {
        Assume.That( ViewFactory.SupportedViewTypes, Does.Contain( typeof( SpinnerView ) ) );

        Assume.That( Application.MainLoop.Timeouts, Is.Empty );

        using ( SpinnerView s = ViewFactory.Create<SpinnerView>( ) )
        {
            Assert.That( Application.MainLoop.Timeouts, Is.Not.Empty );
        }

        Assert.That( Application.MainLoop.Timeouts, Is.Empty );
    }
    [Test]
    [Category("Code Generation")]
    public void NewSpinnerAutoSpins_AfterRoundTrip()
    {
        Assume.That( ViewFactory.SupportedViewTypes.ToArray(), Does.Contain( typeof(SpinnerView) ) );

        Assume.That( Application.MainLoop.Timeouts, Is.Empty );

        RoundTrip<View, SpinnerView>((d,v) =>
        {

        },out _);

        // Auto-spin original and the one that is read back in
        Assert.That( Application.MainLoop.Timeouts, Has.Count.EqualTo( 2 ) );
    }

    [Test]
    [Category("Code Generation")]
    public void NewSpinnerAutoSpins_ChangeStyle()
    {
        var backIn = RoundTrip<View, SpinnerView>((d, v) =>
        {
            var prop = d.GetDesignableProperty(nameof(SpinnerView.Style))
                       ?? throw new Exception("Property was unexpectedly not designable");

            prop.SetValue(new Triangle());
            Assert.That(v.Style, Is.InstanceOf<Triangle>( ) );
        }, out _);

        // Auto-spin original and the one that is read back in
        Assert.That( backIn.Style, Is.InstanceOf<Triangle>( ) );
    }
}