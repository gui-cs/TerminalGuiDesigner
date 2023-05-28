using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using TerminalGuiDesigner;
using static Terminal.Gui.SpinnerStyle;

namespace UnitTests
{
    internal class SpinnerViewTests : Tests
    {
        [Test]
        public void TestNewSpinnerAutoSpins()
        {
            Assert.Contains(typeof(SpinnerView), ViewFactory.GetSupportedViews().ToArray());

            Assert.IsEmpty(Application.MainLoop.timeouts);

            var factory = new ViewFactory();
            var s = (SpinnerView)factory.Create(typeof(SpinnerView));
            
            Assert.IsNotEmpty(Application.MainLoop.timeouts);
            s.Dispose();

            Assert.IsEmpty(Application.MainLoop.timeouts);
        }
        [Test]
        public void TestNewSpinnerAutoSpins_AfterRoundTrip()
        {
            Assert.Contains(typeof(SpinnerView), ViewFactory.GetSupportedViews().ToArray());

            Assert.IsEmpty(Application.MainLoop.timeouts);

            RoundTrip<View, SpinnerView>((d,v) =>
            {

            },out _);

            // Autospin original and the one that is read back in
            Assert.AreEqual(2, Application.MainLoop.timeouts.Count);
        }

        [Test]
        public void TestNewSpinnerAutoSpins_ChangeStyle()
        {
            var backIn = RoundTrip<View, SpinnerView>((d, v) =>
            {
                var prop = d.GetDesignableProperty(nameof(SpinnerView.Style))
                    ?? throw new Exception("Property was unexpectedly not designable");

                prop.SetValue(typeof(Triangle));
                Assert.IsInstanceOf<Triangle>(v.Style);
            }, out _);

            // Autospin original and the one that is read back in
            Assert.IsInstanceOf<Triangle>(backIn.Style);
        }
    }
}
