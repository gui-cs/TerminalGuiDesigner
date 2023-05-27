using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using TerminalGuiDesigner;

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
    }
}
