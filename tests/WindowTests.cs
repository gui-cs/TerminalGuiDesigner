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

        [Test]
        public void Window_ArrangementIsUserServiceable()
        {
            RoundTrip<View, Window>(static (_, w) =>
            {
                // The Window in the editor should be fixed so that it plays nicely with designer
                Assert.That(w.Arrangement, Is.EqualTo(ViewArrangement.Fixed));
                var prop = w.GetNearestDesign()?.GetDesignableProperty(nameof(View.Arrangement))
                           ?? throw new Exception("Failed to get Arrangement property");

                // But when we write out to the file and read in compiled source we should have preserved the
                // vanilla setting for Arrangement (typically Moveable for Window).
                Assert.That(prop.GetValue(),Is.EqualTo(new Window().Arrangement));

            }, out _);
        }
    }
}
