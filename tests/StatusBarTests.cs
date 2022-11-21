using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests
{
    internal class StatusBarTests : Tests
    {
        [Test]
        public void TestItemsArePreserved()
        {
            Key shortcutBefore = Key.Null;

            var statusBarIn = RoundTrip<Toplevel, StatusBar>((d, v) =>
            {
                Assert.AreEqual(1, v.Items.Length, $"Expected {nameof(ViewFactory)} to create a placeholder status item in new StatusBars it creates");
                shortcutBefore = v.Items[0].Shortcut;

            }, out _);

            Assert.AreEqual(1, statusBarIn.Items.Length, $"Expected reloading StatusBar to create the same number of StatusItems");
            Assert.AreEqual(shortcutBefore, statusBarIn.Items[0].Shortcut);
        }
    }
}
