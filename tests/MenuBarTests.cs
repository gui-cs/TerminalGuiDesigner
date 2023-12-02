using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.MenuOperations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests;

class MenuBarTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveMenuItems()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo($"{nameof(this.TestRoundTrip_PreserveMenuItems)}.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var mbOut = (MenuBar)factory.Create(typeof(MenuBar));

        // 1 visible root menu (e.g. File)
        ClassicAssert.AreEqual(1, mbOut.Menus.Length);
        // 1 child menu item (e.g. Open)
        ClassicAssert.AreEqual(1, mbOut.Menus[0].Children.Length);

        OperationManager.Instance.Do(new AddViewOperation(mbOut, designOut, "myMenuBar"));

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var mbIn = designBackIn.View.GetActualSubviews().OfType<MenuBar>().Single();

        // 1 visible root menu (e.g. File)
        ClassicAssert.AreEqual(1, mbIn.Menus.Length);
        // 1 child menu item (e.g. Open)
        ClassicAssert.AreEqual(1, mbIn.Menus[0].Children.Length);
    }

    [Test]
    public void TestRoundTrip_PreserveMenuItems_EvenSubmenus()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo($"{nameof(this.TestRoundTrip_PreserveMenuItems_EvenSubmenus)}.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var mbOut = (MenuBar)factory.Create(typeof(MenuBar));

        OperationManager.Instance.Do(new AddViewOperation(mbOut, designOut, "myMenuBar"));

        // create some more children in the menu
        new AddMenuItemOperation(mbOut.Menus[0].Children[0]).Do();
        new AddMenuItemOperation(mbOut.Menus[0].Children[0]).Do();
        new AddMenuItemOperation(mbOut.Menus[0].Children[0]).Do();

        // move the last child to
        new MoveMenuItemRightOperation(mbOut.Menus[0].Children[1]).Do();

        // 1 visible root menu (e.g. File)
        ClassicAssert.AreEqual(1, mbOut.Menus.Length);
        // 3 child menu item (original one + 3 we added -1 because we moved it to submenu)
        ClassicAssert.AreEqual(3, mbOut.Menus[0].Children.Length);

        // should be 1 submenu item (the one we moved)
        ClassicAssert.AreEqual(1, ((MenuBarItem)mbOut.Menus[0].Children[0]).Children.Length);

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var mbIn = designBackIn.View.GetActualSubviews().OfType<MenuBar>().Single();

        // 1 visible root menu (e.g. File)
        ClassicAssert.AreEqual(1, mbIn.Menus.Length);
        // 3 child menu item (original one + 3 we added -1 because we moved it to submenu)
        ClassicAssert.AreEqual(3, mbIn.Menus[0].Children.Length);

        // should be 1 submenu item (the one we moved)
        ClassicAssert.AreEqual(1, ((MenuBarItem)mbIn.Menus[0].Children[0]).Children.Length);
    }

    [Test]
    public void TestMenuOperations()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo($"{nameof(this.TestMenuOperations)}.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var mbOut = (MenuBar)factory.Create(typeof(MenuBar));

        MenuTracker.Instance.Register(mbOut);

        // 1 visible root menu (e.g. File)
        ClassicAssert.AreEqual(1, mbOut.Menus.Length);
        // 1 child menu item (e.g. Open)
        ClassicAssert.AreEqual(1, mbOut.Menus[0].Children.Length);

        var orig = mbOut.Menus[0].Children[0];

        OperationManager.Instance.Do(
            new AddMenuItemOperation(mbOut.Menus[0].Children[0]));

        // Now 2 child menu item
        ClassicAssert.AreEqual(2, mbOut.Menus[0].Children.Length);
        ClassicAssert.AreSame(orig, mbOut.Menus[0].Children[0]); // original is still at top

        OperationManager.Instance.Undo();

        // Now only 1 child menu item
        ClassicAssert.AreEqual(1, mbOut.Menus[0].Children.Length);
        ClassicAssert.AreSame(orig, mbOut.Menus[0].Children[0]); // original is still at top

        OperationManager.Instance.Redo();

        // Now 2 child menu item
        ClassicAssert.AreEqual(2, mbOut.Menus[0].Children.Length);
        ClassicAssert.AreSame(orig, mbOut.Menus[0].Children[0]); // original is still at top

        // Now test moving an item around
        var toMove = mbOut.Menus[0].Children[1];

        // Move second menu item up
        var up = new MoveMenuItemOperation(toMove, true);
        ClassicAssert.IsFalse(up.IsImpossible);
        OperationManager.Instance.Do(up);

        // Original one should now be bottom
        ClassicAssert.AreSame(orig, mbOut.Menus[0].Children[1]);

        // can't move top one up
        ClassicAssert.IsTrue(new MoveMenuItemOperation(toMove, true).IsImpossible);
        // cant move bottom one down
        ClassicAssert.IsTrue(new MoveMenuItemOperation(mbOut.Menus[0].Children[1], false).IsImpossible);

        OperationManager.Instance.Undo();

        // Original one should be back on top
        ClassicAssert.AreSame(orig, mbOut.Menus[0].Children[0]);

        // test moving the top one down
        var toMove2 = mbOut.Menus[0].Children[1];

        // Move first menu item down
        var down = new MoveMenuItemOperation(toMove2, true);
        ClassicAssert.IsFalse(down.IsImpossible);
        OperationManager.Instance.Do(down);

        // Original one should now be bottom
        ClassicAssert.AreSame(orig, mbOut.Menus[0].Children[1]);
        ClassicAssert.AreNotSame(orig, mbOut.Menus[0].Children[0]);

        OperationManager.Instance.Undo();

        // should be back to how we started now
        ClassicAssert.AreSame(orig, mbOut.Menus[0].Children[0]);
        ClassicAssert.AreNotSame(orig, mbOut.Menus[0].Children[1]);
    }

    private MenuBar GetMenuBar()
    {
        return this.GetMenuBar(out _);
    }

    private MenuBar GetMenuBar(out Design root)
    {
        root = this.Get10By10View();

        var bar = (MenuBar)new ViewFactory().Create(typeof(MenuBar));
        var addBarCmd = new AddViewOperation(bar, root, "mb");
        ClassicAssert.IsTrue(addBarCmd.Do());

        // Expect ViewFactory to have created a single
        // placeholder menu item
        ClassicAssert.AreEqual(1, bar.Menus.Length);
        ClassicAssert.AreEqual(1, bar.Menus[0].Children.Length);

        return bar;
    }

    /// <summary>
    /// Tests removing the last menu item (i.e. 'Do Something')
    /// under the only remaining menu header (e.g. 'File F9')
    /// should result in a completely empty menu bar and be undoable
    /// </summary>
    [Test]
    public void TestRemoveFinalMenuItemOnBar()
    {
        var bar = this.GetMenuBar();

        var fileMenu = bar.Menus[0];
        var placeholderMenuItem = fileMenu.Children[0];

        var remove = new RemoveMenuItemOperation(placeholderMenuItem);

        // we are able to remove the last one
        ClassicAssert.IsTrue(remove.Do());
        ClassicAssert.IsEmpty(bar.Menus, "menu bar should now be completely empty");

        remove.Undo();

        // should be back to where we started
        ClassicAssert.AreEqual(1, bar.Menus.Length);
        ClassicAssert.AreEqual(1, bar.Menus[0].Children.Length);
        ClassicAssert.AreSame(placeholderMenuItem, bar.Menus[0].Children[0]);
    }

    /// <summary>
    /// Tests that when there is only one menu item
    /// that it cannot be moved into a submenu
    /// </summary>
    [Test]
    public void TestMoveMenuItemRight_CannotMoveLast()
    {
        var bar = this.GetMenuBar();

        var mi = bar.Menus[0].Children[0];
        var cmd = new MoveMenuItemRightOperation(mi);
        ClassicAssert.IsFalse(cmd.Do());
    }

    [Test]
    public void TestMoveMenuItemRight_CannotMoveElementZero()
    {
        var bar = this.GetMenuBar();

        var mi = bar.Menus[0].Children[0];
        mi.Data = "yarg";
        mi.Shortcut = Key.Y | Key.CtrlMask;
        var addAnother = new AddMenuItemOperation(mi);
        ClassicAssert.True(addAnother.Do());

        // should have added below us
        ClassicAssert.AreSame(mi, bar.Menus[0].Children[0]);
        ClassicAssert.AreNotSame(mi, bar.Menus[0].Children[1]);
        ClassicAssert.AreEqual(2, bar.Menus[0].Children.Length);

        // cannot move element 0
        ClassicAssert.IsFalse(new MoveMenuItemRightOperation(
            bar.Menus[0].Children[0])
        .Do());

        var cmd = new MoveMenuItemRightOperation(
                    bar.Menus[0].Children[1]);

        // can move element 1
        ClassicAssert.IsTrue(cmd.Do());

        // We will have changed from a MenuItem to a MenuBarItem
        // so element 0 will not be us.  In Terminal.Gui there is
        // a different class for a menu item and one with submenus
        ClassicAssert.AreNotSame(mi, bar.Menus[0].Children[0]);
        ClassicAssert.AreEqual(mi.Title, bar.Menus[0].Children[0].Title);
        ClassicAssert.AreEqual(mi.Data, bar.Menus[0].Children[0].Data);
        ClassicAssert.AreEqual(1, bar.Menus[0].Children.Length);

        cmd.Undo();

        ClassicAssert.AreEqual(mi.Title, bar.Menus[0].Children[0].Title);
        ClassicAssert.AreEqual(mi.Data, bar.Menus[0].Children[0].Data);
        ClassicAssert.AreEqual(mi.Shortcut, bar.Menus[0].Children[0].Shortcut);
        ClassicAssert.AreNotSame(mi, bar.Menus[0].Children[1]);
    }

    [Test]
    public void TestMoveMenuItemLeft_CannotMoveRootItems()
    {
        var bar = this.GetMenuBar();

        var mi = bar.Menus[0].Children[0];

        // cannot move a root item
        ClassicAssert.IsFalse(new MoveMenuItemLeftOperation(
            bar.Menus[0].Children[0])
        .Do());
    }

    private MenuBar GetMenuBarWithSubmenuItems(out MenuBarItem head2, out MenuItem topChild)
    {
        var bar = this.GetMenuBar();
        // Set up a menu like:

        /*
           File
            Head1
            Head2 -> Child1
            Head3    Child2
        */

        var mi = bar.Menus[0].Children[0];
        mi.Title = "Head1";

        bar.Menus[0].Children = new[]
        {
            bar.Menus[0].Children[0],
            head2 = new MenuBarItem(new[]
            {
                topChild = new MenuItem("Child1", null, () => { })
                {
                    Data = "Child1",
                    Shortcut = Key.J | Key.CtrlMask,
                },
                new MenuItem("Child2", null, () => { })
                {
                    Data = "Child2",
                    Shortcut = Key.F | Key.CtrlMask,
                },
            })
            {
                Title = "Head2",
            },
            new MenuItem("Head3", null, () => { }),
        };

        return bar;
    }

    [Test]
    public void TestMoveMenuItemLeft_MoveTopChild()
    {
        var bar = this.GetMenuBarWithSubmenuItems(out var head2, out var topChild);

        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(2, head2.Children.Length);
        ClassicAssert.AreSame(topChild, head2.Children[0]);

        var cmd = new MoveMenuItemLeftOperation(topChild);
        ClassicAssert.IsTrue(cmd.Do());

        // move the top child left should pull
        // it out of the submenu and onto the root
        ClassicAssert.AreEqual(4, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(1, head2.Children.Length);

        // it should be pulled out underneath its parent
        // and preserve its (Name) and Shortcuts
        ClassicAssert.AreEqual(topChild.Title, bar.Menus[0].Children[2].Title);
        ClassicAssert.AreEqual(topChild.Data, bar.Menus[0].Children[2].Data);
        ClassicAssert.AreEqual(topChild.Shortcut, bar.Menus[0].Children[2].Shortcut);
        ClassicAssert.AreSame(topChild, bar.Menus[0].Children[2]);

        // undoing command should return us to
        // previous state
        cmd.Undo();

        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(2, head2.Children.Length);

        ClassicAssert.AreEqual(topChild.Title.ToString(), head2.Children[0].Title.ToString());
        ClassicAssert.AreEqual(topChild.Data, head2.Children[0].Data);
        ClassicAssert.AreEqual(topChild.Shortcut, head2.Children[0].Shortcut);
        ClassicAssert.AreSame(topChild, head2.Children[0]);
    }

    [Test]
    public void TestDeletingMenuItemFromSubmenu_TopChild()
    {
        var bar = this.GetMenuBarWithSubmenuItems(out var head2, out var topChild);

        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(2, head2.Children.Length);
        ClassicAssert.AreSame(topChild, head2.Children[0]);

        var cmd = new RemoveMenuItemOperation(topChild);
        ClassicAssert.IsTrue(cmd.Do());

        // Delete the top child should leave only 1 in submenu
        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(1, head2.Children.Length);
        ClassicAssert.AreNotSame(topChild, head2.Children[0]);

        cmd.Undo();

        // should come back now
        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(2, head2.Children.Length);
        ClassicAssert.AreSame(topChild, head2.Children[0]);
    }

    [Test]
    public void TestDeletingMenuItemFromSubmenu_AllSubmenChild()
    {
        var bar = this.GetMenuBarWithSubmenuItems(out var head2, out var topChild);
        var bottomChild = head2.Children[1];

        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(typeof(MenuBarItem), bar.Menus[0].Children[1].GetType());
        ClassicAssert.AreEqual(2, head2.Children.Length);
        ClassicAssert.AreSame(topChild, head2.Children[0]);

        var cmd1 = new RemoveMenuItemOperation(topChild);
        ClassicAssert.IsTrue(cmd1.Do());

        var cmd2 = new RemoveMenuItemOperation(bottomChild);
        ClassicAssert.IsTrue(cmd2.Do());

        // Deleting both children should convert us from
        // a dropdown submenu to just a regular MenuItem
        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(typeof(MenuItem), bar.Menus[0].Children[1].GetType());

        cmd2.Undo();

        // should bring the bottom one back
        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(typeof(MenuBarItem), bar.Menus[0].Children[1].GetType());
        ClassicAssert.AreSame(bottomChild, ((MenuBarItem)bar.Menus[0].Children[1]).Children[0]);

        cmd1.Undo();

        // Both submenu items should now be back
        ClassicAssert.AreEqual(3, bar.Menus[0].Children.Length);
        ClassicAssert.AreEqual(typeof(MenuBarItem), bar.Menus[0].Children[1].GetType());
        ClassicAssert.AreSame(topChild, ((MenuBarItem)bar.Menus[0].Children[1]).Children[0]);
        ClassicAssert.AreSame(bottomChild, ((MenuBarItem)bar.Menus[0].Children[1]).Children[1]);
    }

    [Test]
    public void TestDeletingLastMenuItem_ShouldRemoveWholeBar()
    {
        var bar = this.GetMenuBar(out Design root);

        var mi = bar.Menus[0].Children[0];

        ClassicAssert.Contains(bar, root.View.Subviews.ToArray(),
                "The MenuBar should be on the main view being edited");

        var cmd = new RemoveMenuItemOperation(mi);
        ClassicAssert.IsTrue(cmd.Do());

        ClassicAssert.IsEmpty(bar.Menus, "Expected menu bar header (File) to be removed along with it's last (only) child");

        ClassicAssert.IsFalse(
            root.View.Subviews.Contains(bar),
            "Now that the MenuBar is completely empty it should be automatically removed");

        cmd.Undo();

        ClassicAssert.Contains(bar, root.View.Subviews.ToArray(),
                "Undo should put the MenuBar back on the view again");
    }
}