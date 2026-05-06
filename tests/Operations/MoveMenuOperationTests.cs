using TerminalGuiDesigner.Operations.MenuOperations;

namespace UnitTests.Operations;

internal class MoveMenuOperationTests : Tests
{
    [Test]
    public void TestMoveMenu_WrongViewType_Throws()
    {
        // Label is not a MenuBar so should get Exception
        RoundTrip<Window, Label>((d, v) =>
        {
            ClassicAssert.Throws<ArgumentException>(() => new MoveMenuOperation(App, d, new MenuBarItem(), 1));
        }, out _);
    }

    [Test]
    public void TestMoveMenu_MenuBarItemDoesNotBelongToMenuBar_Throws()
    {
        RoundTrip<Window, MenuBar>((d, v) =>
        {
            // we passed a new MenuBarItem which will not belong to d and therefore should be an Exception
            ClassicAssert.Throws<ArgumentException>(() => new MoveMenuOperation(App, d, new MenuBarItem(), 1));
        }, out _);
    }

    [Test]
    public void TestMoveMenu_OnlyOneMenu_IsImpossible()
    {
        RoundTrip<Window, MenuBar>((d, v) =>
        {
            ClassicAssert.AreEqual(1, v.SubViews.OfType<MenuBarItem>().Count(), $"Expected {nameof(ViewFactory)} to create a {nameof(MenuBar)} with 1 example placeholder Menu");
            ClassicAssert.IsTrue(new MoveMenuOperation(App, d, v.SubViews.OfType<MenuBarItem>().First(), 1).IsImpossible, "Should be impossible to move Menu when there is only one of them");
        }, out _);
    }

    [TestCase(3,-8,0,true)] // move left lots
    [TestCase(3, 55, 5, true)] // move right lots
    [TestCase(0, 0, 0, false)] // move 0 nowhere
    [TestCase(0, 1, 1, true)] // move 0 right 1
    [TestCase(1, -1, 0, true)] // move 1 left 1
    public void TestMoveMenu_DoUndo(int idxToMove, int adjustment, int expectedNewIndex, bool expectPossible)
    {
        RoundTrip<Window, MenuBar>((d, v) =>
        {
            new AddMenuOperation(App, d, "NewMenu").Do();
            new AddMenuOperation(App, d, "NewMenu").Do();
            new AddMenuOperation(App, d, "NewMenu").Do();
            new AddMenuOperation(App, d, "NewMenu").Do();
            new AddMenuOperation(App, d, "NewMenu").Do();

            var menus = v.SubViews.OfType<MenuBarItem>().ToList();
            var toMove = menus[idxToMove];
            var originalIndex = menus.IndexOf(toMove);
            var op = new MoveMenuOperation(App, d, toMove, adjustment);

            if(expectPossible)
            {
                ClassicAssert.IsFalse(op.IsImpossible);
                ClassicAssert.IsTrue(op.Do());
            }
            else
            {
                ClassicAssert.IsTrue(op.IsImpossible);
                ClassicAssert.False(op.Do());
            }

            ClassicAssert.AreEqual(expectedNewIndex, v.SubViews.OfType<MenuBarItem>().ToList().IndexOf(toMove));

            op.Undo();
            ClassicAssert.AreEqual(originalIndex, v.SubViews.OfType<MenuBarItem>().ToList().IndexOf(toMove));

            op.Redo();
            ClassicAssert.AreEqual(expectedNewIndex, v.SubViews.OfType<MenuBarItem>().ToList().IndexOf(toMove));

        }, out _);
    }
}
