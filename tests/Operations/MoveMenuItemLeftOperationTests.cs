using NUnit.Framework;
using System;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations;

internal class MoveMenuItemLeftOperationTests : Tests
{
    [Test]
    public void TestMoveMenuItemLeft_CannotForRootItems()
    {
        RoundTrip<Toplevel, MenuBar>((d,v)=>
        {
            var op = new MoveMenuItemLeftOperation(v.Menus[0].Children[0]);
            Assert.IsTrue(op.IsImpossible, "Expected it to be impossible to move left a menu that is under a root MenuBar Item (e.g. items under File, Edit, View etc)");
            Assert.IsFalse(op.Do());
        }
        ,out _);
    }

    [Test]
    public void TestMoveMenuItemLeft_CannotMoveOrphans()
    {
        // This is not connected to anything!
        var haha = new MenuItem(); 

        var op = new MoveMenuItemLeftOperation(haha);
        Assert.IsTrue(op.IsImpossible, "Expected it to be impossible to move left a menu that is under a root MenuBar Item (e.g. items under File, Edit, View etc)");
        Assert.IsFalse(op.Do());
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void MoveFromSubmenuToRoot(int indexToMove)
    {
        var mb = GetDeepMenu();

        Assert.IsInstanceOf<MenuBarItem>(mb.Menus[0].Children[0]);
        Assert.AreEqual("File", mb.Menus[0].Title.ToString());
        Assert.AreEqual("New", ((MenuBarItem)mb.Menus[0].Children[0]).Title.ToString());
        Assert.AreEqual("Document", ((MenuBarItem)mb.Menus[0].Children[0]).Children[0].Title.ToString());
        Assert.AreEqual("Spreadsheet", ((MenuBarItem)mb.Menus[0].Children[0]).Children[1].Title.ToString());
        Assert.AreEqual("Video Game", ((MenuBarItem)mb.Menus[0].Children[0]).Children[2].Title.ToString());

        var op = new MoveMenuItemLeftOperation(((MenuBarItem)mb.Menus[0].Children[0]).Children[indexToMove]);
        Assert.IsFalse(op.IsImpossible);
        Assert.IsTrue(op.Do());
        Assert.AreEqual(2, ((MenuBarItem)mb.Menus[0].Children[0]).Children.Length);

        Assert.IsInstanceOf<MenuBarItem>(mb.Menus[0].Children[0], "Expected the New sub-menu drop-down to still be there");
        Assert.IsInstanceOf<MenuItem>(mb.Menus[0].Children[1], "Expected our pulled up menu item to now be on root and under sub-menu");


        Assert.AreEqual(
            indexToMove switch
            {
                0 => "Document",
                1 => "Spreadsheet",
                2 => "Video Game",
                _ => throw new System.NotImplementedException(),
            },
            mb.Menus[0].Children[1].Title);

        op.Undo();

        // should now be back to the way we were before applying operation
        Assert.IsInstanceOf<MenuBarItem>(mb.Menus[0].Children[0]);
        Assert.AreEqual("File", mb.Menus[0].Title.ToString());
        Assert.AreEqual("New", ((MenuBarItem)mb.Menus[0].Children[0]).Title.ToString());
        Assert.AreEqual("Document", ((MenuBarItem)mb.Menus[0].Children[0]).Children[0].Title.ToString());
        Assert.AreEqual("Spreadsheet", ((MenuBarItem)mb.Menus[0].Children[0]).Children[1].Title.ToString());
        Assert.AreEqual("Video Game", ((MenuBarItem)mb.Menus[0].Children[0]).Children[2].Title.ToString());
    }
    
    [Test]
    public void MoveAllFromSubmenu()
    {
        var mb = GetDeepMenu();

        Assert.IsInstanceOf<MenuBarItem>(mb.Menus[0].Children[0]);
        Assert.AreEqual("File", mb.Menus[0].Title.ToString());
        Assert.AreEqual("New", ((MenuBarItem)mb.Menus[0].Children[0]).Title.ToString());
        Assert.AreEqual("Document", ((MenuBarItem)mb.Menus[0].Children[0]).Children[0].Title.ToString());
        Assert.AreEqual("Spreadsheet", ((MenuBarItem)mb.Menus[0].Children[0]).Children[1].Title.ToString());
        Assert.AreEqual("Video Game", ((MenuBarItem)mb.Menus[0].Children[0]).Children[2].Title.ToString());

        // Setup operations to remove all 3
        var op1 = new MoveMenuItemLeftOperation(((MenuBarItem)mb.Menus[0].Children[0]).Children[0]);
        var op2 = new MoveMenuItemLeftOperation(((MenuBarItem)mb.Menus[0].Children[0]).Children[1]);
        var op3 = new MoveMenuItemLeftOperation(((MenuBarItem)mb.Menus[0].Children[0]).Children[2]);

        op1.Do();
        Assert.IsInstanceOf<MenuBarItem>(mb.Menus[0].Children[0], "Expected the New sub-menu drop-down to still be there");
        Assert.AreEqual(2,((MenuBarItem)mb.Menus[0].Children[0]).Children.Length);
        op2.Do();
        Assert.IsInstanceOf<MenuBarItem>(mb.Menus[0].Children[0], "Expected the New sub-menu drop-down to still be there");
        Assert.AreEqual(1, ((MenuBarItem)mb.Menus[0].Children[0]).Children.Length);
        op3.Do();
        Assert.IsInstanceOf<MenuItem>(mb.Menus[0].Children[0], "Expected New to convert to a MenuItem (have no drop-down) once last drop-down item was moved out");

        op3.Undo();
        Assert.IsInstanceOf<MenuBarItem>(mb.Menus[0].Children[0], "Expected undo to restore 'New' MenuItem to a drop-down again");
        Assert.AreEqual(1, ((MenuBarItem)mb.Menus[0].Children[0]).Children.Length);
    }
    private MenuBar GetDeepMenu()
    {
        var mb = new MenuBar();
        mb.Menus = new MenuBarItem[]
        {
            new MenuBarItem
            {
                Title = "File",
                Children = new MenuItem[]
                {
                    new MenuBarItem("New",null,()=>{ })
                    {
                        Children = new MenuItem[]
                        {
                            new MenuItem("Document",null,()=>{ }),
                            new MenuItem("Spreadsheet",null,()=>{ }),
                            new MenuItem("Video Game",null,()=>{ }),
                        }
                    }
                }
            }
        };

        MenuTracker.Instance.Register(mb);
        
        return mb;
    }
}
