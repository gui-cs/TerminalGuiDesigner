using NUnit.Framework;
using Terminal.Gui;

namespace tests;

class ScrollViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveContentSize()
    {
        var scrollViewIn = RoundTrip<ScrollView>((s) =>
                s.ContentSize = new Size(10, 5)
                );

        Assert.AreEqual(10, scrollViewIn.ContentSize.Width);
        Assert.AreEqual(5, scrollViewIn.ContentSize.Height);
    }
}
