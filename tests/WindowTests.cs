using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    internal class WindowTests : Tests
    {
        [Test]
        public void TestViewFactory_HasSensibleDimensions()
        {
            var w = ViewFactory.Create<Window>();
            Assert.That(w.Width,Is.EqualTo((DimAbsolute)10));
            Assert.That(w.Height, Is.EqualTo((DimAbsolute)5));
        }

        [Test]
        public void TestViewFactory_IsNotVanillaDraggable()
        {
            RoundTrip<View, Window>(static (_, w) =>
            {
                // When adding a Window to the editor it should not be 'moveable'
                // This is because the core Terminal.Gui drag will conflict with the TGD dragging.
                Assert.That(w.Arrangement, Is.EqualTo(ViewArrangement.Fixed));

            }, out _);
        }
    }
}
