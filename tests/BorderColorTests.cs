using NUnit.Framework;
using Terminal.Gui;

namespace UnitTests
{
    internal class BorderColorTests : Tests
    {
        [Test]
        public void TestRoundTrip_BorderColors_NeverSet()
        {
            var result = RoundTrip<Window, FrameView>((d, v) =>
            {
                // Our view should not have any border color to start with
                Assert.AreEqual(-1,(int) v.Border.BorderBrush);
                Assert.AreEqual(-1,(int) v.Border.Background);

            },out _);

            // Since there were no changes we would expect them to stay the same
            // after reload
            Assert.AreEqual(-1, (int)result.Border.BorderBrush);
            Assert.AreEqual(-1, (int)result.Border.Background);
        }
    }
}
