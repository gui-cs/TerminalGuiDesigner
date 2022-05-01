using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;

class MenuBarTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveMenuItems()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo($"{nameof(TestRoundTrip_PreserveMenuItems)}.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var mbOut = (MenuBar)factory.Create(typeof(MenuBar));

        // 1 visible root menu (e.g. File)
        Assert.AreEqual(1, mbOut.Menus.Length);
        // 1 child menu item (e.g. Open)
        Assert.AreEqual(1,mbOut.Menus[0].Children.Length);

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, mbOut, designOut, "myMenuBar"));

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Dialog));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var mbIn = designBackIn.View.GetActualSubviews().OfType<MenuBar>().Single();

        // 1 visible root menu (e.g. File)
        Assert.AreEqual(1, mbIn.Menus.Length);
        // 1 child menu item (e.g. Open)
        Assert.AreEqual(1,mbIn.Menus[0].Children.Length);
    }
}