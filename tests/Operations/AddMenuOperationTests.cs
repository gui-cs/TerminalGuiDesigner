using NUnit.Framework;
using System;
using System.IO;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations;

internal class AddMenuOperationTests : Tests
{
    [Test]
    public void TestAddMenu_InvalidViewType()
    {
        var d = Get10By10View();
        var ex = Assert.Throws<ArgumentException>(() => new AddMenuOperation(d, "haha!"));
        Assert.AreEqual("Design must be for a MenuBar to support AddMenuOperation", ex?.Message);
    }

    [Test]
    public void TestAddingMenu_AtRoot_Do()
    {
        const string expectedTopLevelMenuName = "_File (F9)";

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
            Assert.AreEqual(1, v.Menus.Length, "Expected 1 placeholder example Menu");
            var first = v.Menus[0];

            var add = new AddMenuOperation(d, "Blarg");
            Assert.AreEqual(1, v.Menus.Length, "Expected no changes until we actually run the operation");

            add.Do();

            Assert.AreEqual(2, v.Menus.Length, "Expected a new top level menu to be added");
            Assert.AreSame(first, v.Menus[0], "Expected new item to be right of the original");
            Assert.AreEqual("Blarg", v.Menus[1].Title.ToString());

        }, out _);

        Assert.AreEqual(2, viewIn.Menus.Length);
        Assert.AreEqual(expectedTopLevelMenuName, viewIn.Menus[0].Title.ToString());
        Assert.AreEqual("Blarg", viewIn.Menus[1].Title.ToString());
    }

    [Test]
    public void TestAddingMenu_AtRoot_BlankMenuName()
    {
        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            Assert.AreEqual(1, v.Menus.Length);
            var add = new AddMenuOperation(d, "   ");
            add.Do();
            Assert.AreEqual(2, v.Menus.Length);
            Assert.AreEqual("blank", v.Menus[1].Title);

        }, out _);

        Assert.AreEqual(2, viewIn.Menus.Length);
        Assert.AreEqual("blank", viewIn.Menus[1].Title);
    }

    [Test]
    public void TestAddingMenu_AtRoot_Duplicates()
    {
        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            Assert.AreEqual(1, v.Menus.Length);
            var add = new AddMenuOperation(d, "Fish");
            add.Do();
            add = new AddMenuOperation(d, "Fish");
            add.Do();
            Assert.AreEqual(3, v.Menus.Length);
            Assert.AreEqual("Fish", v.Menus[1].Title.ToString());
            Assert.AreEqual("Fish2", v.Menus[2].Title.ToString());

        }, out _);

        Assert.AreEqual(3, viewIn.Menus.Length);
        Assert.AreEqual("Fish", viewIn.Menus[1].Title.ToString());
        Assert.AreEqual("Fish2", viewIn.Menus[2].Title.ToString());

        // Check that the .Designer.cs is producing sensible private field names
        FileAssert.Exists(((Design)viewIn.Data).SourceCode.DesignerFile);
        var code = File.ReadAllText(((Design)viewIn.Data).SourceCode.DesignerFile.FullName);

        StringAssert.Contains("private Terminal.Gui.MenuBarItem fileF9Menu;", code);
        StringAssert.Contains("private Terminal.Gui.MenuBarItem fishMenu;", code);
        StringAssert.Contains("private Terminal.Gui.MenuBarItem fish2Menu;", code);


        StringAssert.Contains(
            "private Terminal.Gui.MenuItem editMeMenuItem;",
            code,
            "Expected these to be created as template items under the new top level menus");
        StringAssert.Contains("private Terminal.Gui.MenuItem editMeMenuItem2;", code);
    }
    [Test]
    public void TestAddingMenu_AtRoot_UnDo()
    {
        const string expectedTopLevelMenuName = "_File (F9)";

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
            Assert.AreEqual(1, v.Menus.Length, "Expected 1 placeholder example Menu");
            var first = v.Menus[0];

            var add = new AddMenuOperation(d, "Blarg");
            Assert.AreEqual(1, v.Menus.Length, "Expected no changes until we actually run the operation");

            add.Do();
            Assert.AreEqual(2, v.Menus.Length, "Expected a new top level menu to be added");
            Assert.AreSame(first, v.Menus[0], "Expected new item to be right of the original");
            Assert.AreEqual("Blarg", v.Menus[1].Title.ToString());

            add.Undo();
            Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString());
            Assert.AreEqual(1, v.Menus.Length);

            add.Redo();
            Assert.AreEqual(2, v.Menus.Length);
            Assert.AreSame(first, v.Menus[0]);
            Assert.AreEqual("Blarg", v.Menus[1].Title.ToString());

            add.Undo();
            add.Undo();
            add.Undo();
            add.Undo();
            Assert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString());
            Assert.AreEqual(1, v.Menus.Length);

        }, out _);

        Assert.AreEqual(1, viewIn.Menus.Length);
        Assert.AreEqual(expectedTopLevelMenuName, viewIn.Menus[0].Title.ToString());
    }
}
