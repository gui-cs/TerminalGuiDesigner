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
            ClassicAssert.Contains(typeof(SpinnerView), ViewFactory.GetSupportedViews().ToArray());

            ClassicAssert.IsEmpty(Application.MainLoop.Timeouts);

            var s = (SpinnerView)ViewFactory.Create(typeof(SpinnerView));
            
            ClassicAssert.IsNotEmpty(Application.MainLoop.Timeouts);
            s.Dispose();

            ClassicAssert.IsEmpty(Application.MainLoop.Timeouts);
        }
        [Test]
        public void TestNewSpinnerAutoSpins_AfterRoundTrip()
        {
            ClassicAssert.Contains(typeof(SpinnerView), ViewFactory.GetSupportedViews().ToArray());

            ClassicAssert.IsEmpty(Application.MainLoop.Timeouts);

            RoundTrip<View, SpinnerView>((d,v) =>
            {

            },out _);

            // Autospin original and the one that is read back in
            ClassicAssert.AreEqual(2, Application.MainLoop.Timeouts.Count);
        }

        [Test]
        public void TestNewSpinnerAutoSpins_ChangeStyle()
        {
            var backIn = RoundTrip<View, SpinnerView>((d, v) =>
            {
                var prop = d.GetDesignableProperty(nameof(SpinnerView.Style))
                    ?? throw new Exception("Property was unexpectedly not designable");

                prop.SetValue(new Triangle());
                ClassicAssert.IsInstanceOf<Triangle>(v.Style);
            }, out _);

            // Autospin original and the one that is read back in
            ClassicAssert.IsInstanceOf<Triangle>(backIn.Style);
        }
    }
}
