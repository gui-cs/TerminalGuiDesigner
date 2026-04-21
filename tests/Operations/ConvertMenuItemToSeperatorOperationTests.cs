using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

internal class ConvertMenuItemToSeperatorOperationTests : Tests
{
    [Test]
    public void TestConvertToSeperator_RoundTrip_Do()
    {
        var mbIn = RoundTrip<Runnable, MenuBar>((d, v) =>
        {
            var fileMenu = v.SubViews.OfType<MenuBarItem>().First();
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.IsNotNull(fileMenu.GetMenuItems(out _)[0]);

            var op = new ConvertMenuItemToSeperatorOperation(App, fileMenu.GetMenuItems(out _)[0]);

            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.IsNotNull(fileMenu.GetMenuItems(out _)[0]);
            op.Do();

            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreEqual(MenuBarExtensions.SeparatorTitle, fileMenu.GetMenuItems(out _)[0].Title.ToString());

        }, out _);

        var mbInFileMenu = mbIn.SubViews.OfType<MenuBarItem>().First();
        ClassicAssert.AreEqual(1, mbInFileMenu.GetMenuItems(out _).Count);
        ClassicAssert.AreEqual(MenuBarExtensions.SeparatorTitle, mbInFileMenu.GetMenuItems(out _)[0].Title.ToString());
    }

    [Test]
    public void TestConvertToSeperator_RoundTrip_UnDo()
    {
        var mbIn = RoundTrip<Runnable, MenuBar>((d, v) =>
        {
            var fileMenu = v.SubViews.OfType<MenuBarItem>().First();
            var orig = fileMenu.GetMenuItems(out _)[0];
            var origTitle = orig.Title.ToString();
            var op = new ConvertMenuItemToSeperatorOperation(App, orig);
            op.Do();
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreEqual(MenuBarExtensions.SeparatorTitle, fileMenu.GetMenuItems(out _)[0].Title.ToString());

            op.Undo();
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreSame(orig, fileMenu.GetMenuItems(out _)[0]);
            ClassicAssert.AreEqual(origTitle, fileMenu.GetMenuItems(out _)[0].Title.ToString());

            op.Undo();
            op.Redo();
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreEqual(MenuBarExtensions.SeparatorTitle, fileMenu.GetMenuItems(out _)[0].Title.ToString());

            op.Undo();
            op.Undo();
            op.Undo();
            ClassicAssert.AreEqual(1, fileMenu.GetMenuItems(out _).Count);
            ClassicAssert.AreSame(orig, fileMenu.GetMenuItems(out _)[0]);
            ClassicAssert.AreEqual(origTitle, fileMenu.GetMenuItems(out _)[0].Title.ToString());

        }, out _);

        var mbInFileMenu = mbIn.SubViews.OfType<MenuBarItem>().First();
        ClassicAssert.AreEqual(1, mbInFileMenu.GetMenuItems(out _).Count);
        ClassicAssert.IsNotNull(mbInFileMenu.GetMenuItems(out _)[0]);
        ClassicAssert.AreNotEqual(MenuBarExtensions.SeparatorTitle, mbInFileMenu.GetMenuItems(out _)[0].Title.ToString());
    }
}
