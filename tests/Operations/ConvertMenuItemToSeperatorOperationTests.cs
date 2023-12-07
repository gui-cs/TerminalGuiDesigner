using Terminal.Gui;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

internal class ConvertMenuItemToSeperatorOperationTests : Tests
{
    [Test]
    public void TestConvertToSeperator_RoundTrip_Do()
    {
        var mbIn = RoundTrip<Toplevel, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.IsNotNull(v.Menus[0].Children[0]);

            var op = new ConvertMenuItemToSeperatorOperation(v.Menus[0].Children[0]);

            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.IsNotNull(v.Menus[0].Children[0]);
            op.Do();

            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.IsNull(v.Menus[0].Children[0]);

        },out _);


        ClassicAssert.AreEqual(1, mbIn.Menus[0].Children.Length);
        ClassicAssert.IsNull(mbIn.Menus[0].Children[0]);
    }

    [Test]
    public void TestConvertToSeperator_RoundTrip_UnDo()
    {
        var mbIn = RoundTrip<Toplevel, MenuBar>((d, v) =>
        {
            var orig = v.Menus[0].Children[0];
            var op = new ConvertMenuItemToSeperatorOperation(orig);
            op.Do();
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.IsNull(v.Menus[0].Children[0]);

            op.Undo();
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.AreSame(orig, v.Menus[0].Children[0]);

            op.Undo();
            op.Redo();
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.IsNull(v.Menus[0].Children[0]);


            op.Undo();
            op.Undo();
            op.Undo();
            ClassicAssert.AreEqual(1, v.Menus[0].Children.Length);
            ClassicAssert.AreSame(orig, v.Menus[0].Children[0]);

        }, out _);

        ClassicAssert.AreEqual(1, mbIn.Menus[0].Children.Length);
        ClassicAssert.IsNotNull(mbIn.Menus[0].Children[0]);
    }
}
