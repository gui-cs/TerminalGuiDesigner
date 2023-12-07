using System.IO;
using System.Linq;
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

        var lbl1 = ViewFactory.Create<Label>( );
        var lbl2 = ViewFactory.Create<Label>( );

        // add 2 labels
        new AddViewOperation(lbl1, designOut, "lbl1").Do();
        new AddViewOperation(lbl2, designOut, "lbl2").Do();

        // not impossible, we could totally delete either of these
        ClassicAssert.IsFalse(new DeleteViewOperation((Design)lbl1.Data).IsImpossible);
        ClassicAssert.IsFalse(new DeleteViewOperation((Design)lbl2.Data).IsImpossible);

        // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
        lbl2.X = Pos.Right(lbl1) + 5;

        ClassicAssert.IsTrue(new DeleteViewOperation((Design)lbl1.Data).IsImpossible);
    }

    [Test]
    public void TestDeletingObjectWithDependency_IsAllowedIfDeletingBoth()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var lbl1 = ViewFactory.Create<Label>( );
        var lbl2 = ViewFactory.Create<Label>( );

        // add 2 labels
        new AddViewOperation(lbl1, designOut, "lbl1").Do();
        new AddViewOperation(lbl2, designOut, "lbl2").Do();

        // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
        lbl2.X = Pos.Right(lbl1) + 5;

        // Deleting both at once should be possible since there are no hanging references
        ClassicAssert.IsFalse(new DeleteViewOperation((Design)lbl1.Data, (Design)lbl2.Data).IsImpossible);
        ClassicAssert.IsFalse(new DeleteViewOperation((Design)lbl2.Data, (Design)lbl1.Data).IsImpossible);

        ClassicAssert.AreEqual(3, designOut.GetAllDesigns().Count());
        var cmd = new DeleteViewOperation((Design)lbl2.Data, (Design)lbl1.Data);
        ClassicAssert.IsTrue(cmd.Do());
        ClassicAssert.AreEqual(1, designOut.GetAllDesigns().Count());

        cmd.Undo();
        ClassicAssert.AreEqual(3, designOut.GetAllDesigns().Count());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestDeleting_ClearsSelection(bool lockSelection)
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var lbl1 = ViewFactory.Create<Label>( );

        new AddViewOperation(lbl1, designOut, "lbl1").Do();

        var lbl1Design = (Design)lbl1.Data;

        SelectionManager.Instance.SetSelection(lbl1Design);

        // normally commands are run with locked selection, lets run this test with both cases to be sure
        SelectionManager.Instance.LockSelection = lockSelection;

        ClassicAssert.IsFalse(new DeleteViewOperation(lbl1Design).IsImpossible);

        ClassicAssert.AreEqual(2, designOut.GetAllDesigns().Count());
        var cmd = new DeleteViewOperation(lbl1Design);

        ClassicAssert.Contains(lbl1Design, SelectionManager.Instance.Selected.ToArray());

        ClassicAssert.IsTrue(cmd.Do());
        ClassicAssert.AreEqual(1, designOut.GetAllDesigns().Count());

        ClassicAssert.IsEmpty(SelectionManager.Instance.Selected.ToArray(), "Deleting the view should remove it from the active selection");

        cmd.Undo();
        ClassicAssert.AreEqual(2, designOut.GetAllDesigns().Count());
        ClassicAssert.Contains(lbl1Design, SelectionManager.Instance.Selected.ToArray(), "Undoing a delete operation should restore the previous selection");
    }
}
