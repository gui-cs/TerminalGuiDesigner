using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations
{
    internal class ConvertMenuItemToSeperatorOperationTests : Tests
    {
        [Test]
        public void TestConvertToSeperator_RoundTrip_Do()
        {
            var mbIn = RoundTrip<Toplevel, MenuBar>((d, v) =>
            {
                Assert.AreEqual(1, v.Menus[0].Children.Length);
                Assert.IsNotNull(v.Menus[0].Children[0]);

                var op = new ConvertMenuItemToSeperatorOperation(v.Menus[0].Children[0]);

                Assert.AreEqual(1, v.Menus[0].Children.Length);
                Assert.IsNotNull(v.Menus[0].Children[0]);
                op.Do();

                Assert.AreEqual(1, v.Menus[0].Children.Length);
                Assert.IsNull(v.Menus[0].Children[0]);

            },out _);


            Assert.AreEqual(1, mbIn.Menus[0].Children.Length);
            Assert.IsNull(mbIn.Menus[0].Children[0]);
        }

        [Test]
        public void TestConvertToSeperator_RoundTrip_UnDo()
        {
            var mbIn = RoundTrip<Toplevel, MenuBar>((d, v) =>
            {
                var orig = v.Menus[0].Children[0];
                var op = new ConvertMenuItemToSeperatorOperation(orig);
                op.Do();
                Assert.AreEqual(1, v.Menus[0].Children.Length);
                Assert.IsNull(v.Menus[0].Children[0]);

                op.Undo();
                Assert.AreEqual(1, v.Menus[0].Children.Length);
                Assert.AreSame(orig, v.Menus[0].Children[0]);

                op.Undo();
                op.Redo();
                Assert.AreEqual(1, v.Menus[0].Children.Length);
                Assert.IsNull(v.Menus[0].Children[0]);


                op.Undo();
                op.Undo();
                op.Undo();
                Assert.AreEqual(1, v.Menus[0].Children.Length);
                Assert.AreSame(orig, v.Menus[0].Children[0]);

            }, out _);

            Assert.AreEqual(1, mbIn.Menus[0].Children.Length);
            Assert.IsNotNull(mbIn.Menus[0].Children[0]);
        }
    }
}
