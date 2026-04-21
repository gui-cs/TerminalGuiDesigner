using Terminal.Gui.ViewBase;
using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

internal class AddMenuOperationTests : Tests
{
    [Test]
    public void TestAddMenu_InvalidViewType()
    {
        var d = Get10By10View();
        var ex = ClassicAssert.Throws<ArgumentException>(() => new AddMenuOperation(App, d, "haha!"));
        ClassicAssert.AreEqual("Design must wrap a MenuBar to be used with this operation.", ex?.Message);
    }

    [Test]
    public void TestAddingMenu_AtRoot_Do()
    {
        const string expectedRunnableMenuName = "_File";

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(expectedRunnableMenuName, v.SubViews.OfType<MenuBarItem>().First().Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
            ClassicAssert.AreEqual(Key.F9, v.SubViews.OfType<MenuBarItem>().First().Key, "Expected a new MenuBar added in Designer to have default of F9");

            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count(), "Expected 1 placeholder example Menu");
            var first = v.SubViews.OfType<MenuBarItem>().First();

            var add = new AddMenuOperation(App, d, "Blarg");
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count(), "Expected no changes until we actually run the operation");

            add.Do();

            ClassicAssert.AreEqual(2, v.SubViews.OfType<MenuBarItem>().Count(), "Expected a new top level menu to be added");
            ClassicAssert.AreSame(first, v.SubViews.OfType<MenuBarItem>().First(), "Expected new item to be right of the original");
            ClassicAssert.AreEqual("Blarg", v.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());

        }, out _);

        ClassicAssert.AreEqual(2, viewIn.SubViews.OfType<MenuBarItem>().Count());
        ClassicAssert.AreEqual(expectedRunnableMenuName, viewIn.SubViews.OfType<MenuBarItem>().First().Title.ToString());
        ClassicAssert.AreEqual(Key.F9, viewIn.SubViews.OfType<MenuBarItem>().First().Key);
        ClassicAssert.AreEqual("Blarg", viewIn.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());
    }

    [Test]
    public void TestAddingMenu_AtRoot_BlankMenuName()
    {
        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count());
            var add = new AddMenuOperation(App, d, "   ");
            add.Do();
            ClassicAssert.AreEqual(2, v.SubViews.OfType<MenuBarItem>().Count());
            ClassicAssert.AreEqual("blank", v.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());

        }, out _);

        ClassicAssert.AreEqual(2, viewIn.SubViews.OfType<MenuBarItem>().Count());
        ClassicAssert.AreEqual("blank", viewIn.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());
    }

    [Test]
    public void TestAddingMenu_AtRoot_Duplicates()
    {
        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count());
            var add = new AddMenuOperation(App, d, "Fish");
            add.Do();
            add = new AddMenuOperation(App, d, "Fish");
            add.Do();
            ClassicAssert.AreEqual(3, v.SubViews.OfType<MenuBarItem>().Count());
            ClassicAssert.AreEqual("Fish", v.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());
            ClassicAssert.AreEqual("Fish2", v.SubViews.OfType<MenuBarItem>().ElementAt(2).Title.ToString());

        }, out _);

        ClassicAssert.AreEqual(3, viewIn.SubViews.OfType<MenuBarItem>().Count());
        ClassicAssert.AreEqual("Fish", viewIn.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());
        ClassicAssert.AreEqual("Fish2", viewIn.SubViews.OfType<MenuBarItem>().ElementAt(2).Title.ToString());

        // Check that the .Designer.cs is producing sensible private field names
        FileAssert.Exists(((Design)viewIn.Data).SourceCode.DesignerFile);
        var code = File.ReadAllText(((Design)viewIn.Data).SourceCode.DesignerFile.FullName);

        StringAssert.Contains("private Terminal.Gui.Views.MenuBarItem fileMenu;", code);
        StringAssert.Contains("private Terminal.Gui.Views.MenuBarItem fishMenu;", code);
        StringAssert.Contains("private Terminal.Gui.Views.MenuBarItem fish2Menu;", code);

        StringAssert.Contains(
            "private Terminal.Gui.Views.MenuItem editMeMenuItem;",
            code,
            "Expected these to be created as template items under the new top level menus");
        StringAssert.Contains("private Terminal.Gui.Views.MenuItem editMeMenuItem2;", code);
        StringAssert.Contains("private Terminal.Gui.Views.MenuItem editMeMenuItem3;", code);
    }
    [Test]
    public void TestAddingMenu_AtRoot_UnDo()
    {
        const string expectedRunnableMenuName = "_File";

        var viewIn = RoundTrip<View, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(expectedRunnableMenuName, v.SubViews.OfType<MenuBarItem>().First().Title.ToString(), "Expected a new MenuBar added in Designer to have a placeholder title");
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count(), "Expected 1 placeholder example Menu");
            var first = v.SubViews.OfType<MenuBarItem>().First();

            var add = new AddMenuOperation(App, d, "Blarg");
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count(), "Expected no changes until we actually run the operation");

            add.Do();
            ClassicAssert.AreEqual(2, v.SubViews.OfType<MenuBarItem>().Count(), "Expected a new top level menu to be added");
            ClassicAssert.AreSame(first, v.SubViews.OfType<MenuBarItem>().First(), "Expected new item to be right of the original");
            ClassicAssert.AreEqual("Blarg", v.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());

            add.Undo();
            ClassicAssert.AreEqual(expectedRunnableMenuName, v.SubViews.OfType<MenuBarItem>().First().Title.ToString());
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count());

            add.Redo();
            ClassicAssert.AreEqual(2, v.SubViews.OfType<MenuBarItem>().Count());
            ClassicAssert.AreSame(first, v.SubViews.OfType<MenuBarItem>().First());
            ClassicAssert.AreEqual("Blarg", v.SubViews.OfType<MenuBarItem>().ElementAt(1).Title.ToString());

            add.Undo();
            add.Undo();
            add.Undo();
            add.Undo();
            ClassicAssert.AreEqual(expectedRunnableMenuName, v.SubViews.OfType<MenuBarItem>().First().Title.ToString());
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count());

        }, out _);

        ClassicAssert.AreEqual(1, viewIn.SubViews.OfType<MenuBarItem>().Count());
        ClassicAssert.AreEqual(expectedRunnableMenuName, viewIn.SubViews.OfType<MenuBarItem>().First().Title.ToString());
    }
}
