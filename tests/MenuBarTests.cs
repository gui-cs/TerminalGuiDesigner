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

    [Test]
    public void TestRoundTrip_PreserveMenuItems_EvenSubmenus()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo($"{nameof(TestRoundTrip_PreserveMenuItems_EvenSubmenus)}.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var mbOut = (MenuBar)factory.Create(typeof(MenuBar));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, mbOut, designOut, "myMenuBar"));

        // create some more children in the menu
        new AddMenuItemOperation(mbOut,mbOut,mbOut.Menus[0],mbOut.Menus[0].Children[0]).Do();
        new AddMenuItemOperation(mbOut,mbOut,mbOut.Menus[0],mbOut.Menus[0].Children[0]).Do();
        new AddMenuItemOperation(mbOut,mbOut,mbOut.Menus[0],mbOut.Menus[0].Children[0]).Do();

        // move the last child to 
        new MoveMenuItemRightOperation(mbOut,mbOut,mbOut.Menus[0],mbOut.Menus[0].Children[1]).Do();

        // 1 visible root menu (e.g. File)
        Assert.AreEqual(1, mbOut.Menus.Length);
        // 3 child menu item (original one + 3 we added -1 because we moved it to submenu)
        Assert.AreEqual(3,mbOut.Menus[0].Children.Length);

        // should be 1 submenu item (the one we moved)
        Assert.AreEqual(1,((MenuBarItem)mbOut.Menus[0].Children[0]).Children.Length);

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Dialog));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var mbIn = designBackIn.View.GetActualSubviews().OfType<MenuBar>().Single();

        // 1 visible root menu (e.g. File)
        Assert.AreEqual(1, mbIn.Menus.Length);
        // 3 child menu item (original one + 3 we added -1 because we moved it to submenu)
        Assert.AreEqual(3,mbIn.Menus[0].Children.Length);

        // should be 1 submenu item (the one we moved)
        Assert.AreEqual(1,((MenuBarItem)mbIn.Menus[0].Children[0]).Children.Length);
    }

    [Test]
    public void TestMenuOperations()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo($"{nameof(TestMenuOperations)}.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var mbOut = (MenuBar)factory.Create(typeof(MenuBar));

        // 1 visible root menu (e.g. File)
        Assert.AreEqual(1, mbOut.Menus.Length);
        // 1 child menu item (e.g. Open)
        Assert.AreEqual(1,mbOut.Menus[0].Children.Length);

        var orig = mbOut.Menus[0].Children[0];

        OperationManager.Instance.Do(
            new AddMenuItemOperation(designOut.View,mbOut,mbOut.Menus[0],mbOut.Menus[0].Children[0])
        );

        // Now 2 child menu item
        Assert.AreEqual(2,mbOut.Menus[0].Children.Length);
        Assert.AreSame(orig,mbOut.Menus[0].Children[0]); // original is still at top

        OperationManager.Instance.Undo();

        // Now only 1 child menu item
        Assert.AreEqual(1,mbOut.Menus[0].Children.Length);
        Assert.AreSame(orig,mbOut.Menus[0].Children[0]); // original is still at top

        OperationManager.Instance.Redo();

        // Now 2 child menu item
        Assert.AreEqual(2,mbOut.Menus[0].Children.Length);
        Assert.AreSame(orig,mbOut.Menus[0].Children[0]); // original is still at top

        // Now test moving an item around
        var toMove = mbOut.Menus[0].Children[1];

        // Move second menu item up
        var up = new MoveMenuItemOperation(designOut.View,mbOut,mbOut.Menus[0],toMove,true);
        Assert.IsFalse(up.IsImpossible);
        OperationManager.Instance.Do(up);

        // Original one should now be bottom
        Assert.AreSame(orig,mbOut.Menus[0].Children[1]); 

        // can't move top one up
        Assert.IsTrue(new MoveMenuItemOperation(designOut.View,mbOut,mbOut.Menus[0],toMove,true).IsImpossible);
        // cant move bottom one down
        Assert.IsTrue(new MoveMenuItemOperation(designOut.View,mbOut,mbOut.Menus[0],mbOut.Menus[0].Children[1],false).IsImpossible);

        OperationManager.Instance.Undo();

        // Original one should be back on top
        Assert.AreSame(orig,mbOut.Menus[0].Children[0]);
        
        // test moving the top one down
        var toMove2 = mbOut.Menus[0].Children[1];

        // Move first menu item down
        var down = new MoveMenuItemOperation(designOut.View,mbOut,mbOut.Menus[0],toMove2,true);
        Assert.IsFalse(down.IsImpossible);
        OperationManager.Instance.Do(down);

        // Original one should now be bottom
        Assert.AreSame(orig,mbOut.Menus[0].Children[1]); 
        Assert.AreNotSame(orig,mbOut.Menus[0].Children[0]); 

        OperationManager.Instance.Undo();

        // should be back to how we started now
        Assert.AreSame(orig,mbOut.Menus[0].Children[0]); 
        Assert.AreNotSame(orig,mbOut.Menus[0].Children[1]); 




    }
}