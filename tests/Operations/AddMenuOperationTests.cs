using NUnit.Framework;
using System;
using System.IO;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

internal class AddMenuOperationTests : Tests
{
    [Test]
    public void TestAddMenu_InvalidViewType()
    {
        var d = Get10By10View();
        var ex = ClassicAssert.Throws<ArgumentException>(() => new AddMenuOperation(d, "haha!"));
        ClassicAssert.AreEqual("Design must wrap a MenuBar to be used with this operation.", ex?.Message);
    }

    [Test]
    public void TestAddingMenu_AtRoot_Do()
    {
        const string expectedTopLevelMenuName = "_File (F9)";

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
            ClassicAssert.AreEqual(1, v.Menus.Length, "Expected 1 placeholder example Menu");
            var first = v.Menus[0];

            var add = new AddMenuOperation(d, "Blarg");
            ClassicAssert.AreEqual(1, v.Menus.Length, "Expected no changes until we actually run the operation");

            add.Do();

            ClassicAssert.AreEqual(2, v.Menus.Length, "Expected a new top level menu to be added");
            ClassicAssert.AreSame(first, v.Menus[0], "Expected new item to be right of the original");
            ClassicAssert.AreEqual("Blarg", v.Menus[1].Title.ToString());

        }, out _);

        ClassicAssert.AreEqual(2, viewIn.Menus.Length);
        ClassicAssert.AreEqual(expectedTopLevelMenuName, viewIn.Menus[0].Title.ToString());
        ClassicAssert.AreEqual("Blarg", viewIn.Menus[1].Title.ToString());
    }

    [Test]
    public void TestAddingMenu_AtRoot_BlankMenuName()
    {
        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(1, v.Menus.Length);
            var add = new AddMenuOperation(d, "   ");
            add.Do();
            ClassicAssert.AreEqual(2, v.Menus.Length);
            ClassicAssert.AreEqual("blank", v.Menus[1].Title);

        }, out _);

        ClassicAssert.AreEqual(2, viewIn.Menus.Length);
        ClassicAssert.AreEqual("blank", viewIn.Menus[1].Title);
    }

    [Test]
    public void TestAddingMenu_AtRoot_Duplicates()
    {
        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(1, v.Menus.Length);
            var add = new AddMenuOperation(d, "Fish");
            add.Do();
            add = new AddMenuOperation(d, "Fish");
            add.Do();
            ClassicAssert.AreEqual(3, v.Menus.Length);
            ClassicAssert.AreEqual("Fish", v.Menus[1].Title.ToString());
            ClassicAssert.AreEqual("Fish2", v.Menus[2].Title.ToString());

        }, out _);

        ClassicAssert.AreEqual(3, viewIn.Menus.Length);
        ClassicAssert.AreEqual("Fish", viewIn.Menus[1].Title.ToString());
        ClassicAssert.AreEqual("Fish2", viewIn.Menus[2].Title.ToString());

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
            ClassicAssert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
            ClassicAssert.AreEqual(1, v.Menus.Length, "Expected 1 placeholder example Menu");
            var first = v.Menus[0];

            var add = new AddMenuOperation(d, "Blarg");
            ClassicAssert.AreEqual(1, v.Menus.Length, "Expected no changes until we actually run the operation");

            add.Do();
            ClassicAssert.AreEqual(2, v.Menus.Length, "Expected a new top level menu to be added");
            ClassicAssert.AreSame(first, v.Menus[0], "Expected new item to be right of the original");
            ClassicAssert.AreEqual("Blarg", v.Menus[1].Title.ToString());

            add.Undo();
            ClassicAssert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString());
            ClassicAssert.AreEqual(1, v.Menus.Length);

            add.Redo();
            ClassicAssert.AreEqual(2, v.Menus.Length);
            ClassicAssert.AreSame(first, v.Menus[0]);
            ClassicAssert.AreEqual("Blarg", v.Menus[1].Title.ToString());

            add.Undo();
            add.Undo();
            add.Undo();
            add.Undo();
            ClassicAssert.AreEqual(expectedTopLevelMenuName, v.Menus[0].Title.ToString());
            ClassicAssert.AreEqual(1, v.Menus.Length);

        }, out _);

        ClassicAssert.AreEqual(1, viewIn.Menus.Length);
        ClassicAssert.AreEqual(expectedTopLevelMenuName, viewIn.Menus[0].Title.ToString());
    }
}
