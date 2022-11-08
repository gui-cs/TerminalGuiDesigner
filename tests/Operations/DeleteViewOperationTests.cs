using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests.Operations;

internal class DeleteViewOperationTests : Tests
{
    [Test]
    public void TestDeletingObjectWithDependency_IsImpossible()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var factory = new ViewFactory();
        var lbl1 = (Label)factory.Create(typeof(Label));
        var lbl2 = (Label)factory.Create(typeof(Label));

        // add 2 labels
        new AddViewOperation(lbl1, designOut, "lbl1").Do();
        new AddViewOperation(lbl2, designOut, "lbl2").Do();

        // not impossible, we could totally delete either of these
        Assert.IsFalse(new DeleteViewOperation(lbl1).IsImpossible);
        Assert.IsFalse(new DeleteViewOperation(lbl2).IsImpossible);

        // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
        lbl2.X = Pos.Right(lbl1) + 5;

        Assert.IsTrue(new DeleteViewOperation(lbl1).IsImpossible);
    }

    [Test]
    public void TestDeletingObjectWithDependency_IsAllowedIfDeletingBoth()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var factory = new ViewFactory();
        var lbl1 = (Label)factory.Create(typeof(Label));
        var lbl2 = (Label)factory.Create(typeof(Label));

        // add 2 labels
        new AddViewOperation(lbl1, designOut, "lbl1").Do();
        new AddViewOperation(lbl2, designOut, "lbl2").Do();

        // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
        lbl2.X = Pos.Right(lbl1) + 5;

        // Deleting both at once should be possible since there are no hanging references
        Assert.IsFalse(new DeleteViewOperation(lbl1, lbl2).IsImpossible);
        Assert.IsFalse(new DeleteViewOperation(lbl2, lbl1).IsImpossible);

        Assert.AreEqual(3, designOut.GetAllDesigns().Count());
        var cmd = new DeleteViewOperation(lbl2, lbl1);
        Assert.IsTrue(cmd.Do());
        Assert.AreEqual(1, designOut.GetAllDesigns().Count());

        cmd.Undo();
        Assert.AreEqual(3, designOut.GetAllDesigns().Count());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestDeleting_ClearsSelection(bool lockSelection)
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var factory = new ViewFactory();
        var lbl1 = (Label)factory.Create(typeof(Label));

        new AddViewOperation(lbl1, designOut, "lbl1").Do();

        var lbl1Design = (Design)lbl1.Data;

        SelectionManager.Instance.SetSelection(lbl1Design);

        // normally commands are run with locked selection, lets run this test with both cases to be sure
        SelectionManager.Instance.LockSelection = lockSelection;

        Assert.IsFalse(new DeleteViewOperation(lbl1).IsImpossible);

        Assert.AreEqual(2, designOut.GetAllDesigns().Count());
        var cmd = new DeleteViewOperation(lbl1);

        Assert.Contains(lbl1Design, SelectionManager.Instance.Selected.ToArray());

        Assert.IsTrue(cmd.Do());
        Assert.AreEqual(1, designOut.GetAllDesigns().Count());

        Assert.IsEmpty(SelectionManager.Instance.Selected.ToArray(), "Deleting the view should remove it from the active selection");

        cmd.Undo();
        Assert.AreEqual(2, designOut.GetAllDesigns().Count());
        Assert.Contains(lbl1Design, SelectionManager.Instance.Selected.ToArray(), "Undoing a delete operation should restore the previous selection");
    }
}
