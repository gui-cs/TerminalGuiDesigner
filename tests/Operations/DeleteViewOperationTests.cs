using Terminal.Gui.ViewBase;

namespace UnitTests.Operations;

internal class DeleteViewOperationTests : Tests
{
    [Test]
    public void TestDeletingObjectWithDependency_IsImpossible()
    {
        var viewToCode = new ViewToCode(App);

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var lbl1 = ViewFactory.Create<Label>( );
        var lbl2 = ViewFactory.Create<Label>( );

        // add 2 labels
        new AddViewOperation(App, lbl1, designOut, "lbl1").Do();
        new AddViewOperation(App, lbl2, designOut, "lbl2").Do();

        // not impossible, we could totally delete either of these
        ClassicAssert.IsFalse(new DeleteViewOperation(App, (Design)lbl1.Data).IsImpossible);
        ClassicAssert.IsFalse(new DeleteViewOperation(App, (Design)lbl2.Data).IsImpossible);

        // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
        lbl2.X = Pos.Right(lbl1) + 5;

        ClassicAssert.IsTrue(new DeleteViewOperation(App, (Design)lbl1.Data).IsImpossible);
    }

    [Test]
    public void TestDeletingObjectWithDependency_IsAllowedIfDeletingBoth()
    {
        var viewToCode = new ViewToCode(App);

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var lbl1 = ViewFactory.Create<Label>( );
        var lbl2 = ViewFactory.Create<Label>( );

        // add 2 labels
        new AddViewOperation(App, lbl1, designOut, "lbl1").Do();
        new AddViewOperation(App, lbl2, designOut, "lbl2").Do();

        // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
        lbl2.X = Pos.Right(lbl1) + 5;

        // Deleting both at once should be possible since there are no hanging references
        ClassicAssert.IsFalse(new DeleteViewOperation(App, (Design)lbl1.Data, (Design)lbl2.Data).IsImpossible);
        ClassicAssert.IsFalse(new DeleteViewOperation(App, (Design)lbl2.Data, (Design)lbl1.Data).IsImpossible);

        ClassicAssert.AreEqual(3, designOut.GetAllDesigns().Count());
        var cmd = new DeleteViewOperation(App, (Design)lbl2.Data, (Design)lbl1.Data);
        ClassicAssert.IsTrue(cmd.Do());
        ClassicAssert.AreEqual(1, designOut.GetAllDesigns().Count());

        cmd.Undo();
        ClassicAssert.AreEqual(3, designOut.GetAllDesigns().Count());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestDeleting_ClearsSelection(bool lockSelection)
    {
        var viewToCode = new ViewToCode(App);

        var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var lbl1 = ViewFactory.Create<Label>( );

        new AddViewOperation(App, lbl1, designOut, "lbl1").Do();

        var lbl1Design = (Design)lbl1.Data;

        SelectionManager.Instance.SetSelection(lbl1Design);

        // normally commands are run with locked selection, lets run this test with both cases to be sure
        SelectionManager.Instance.LockSelection = lockSelection;

        ClassicAssert.IsFalse(new DeleteViewOperation(App, lbl1Design).IsImpossible);

        ClassicAssert.AreEqual(2, designOut.GetAllDesigns().Count());
        var cmd = new DeleteViewOperation(App, lbl1Design);

        ClassicAssert.Contains(lbl1Design, SelectionManager.Instance.Selected.ToArray());

        ClassicAssert.IsTrue(cmd.Do());
        ClassicAssert.AreEqual(1, designOut.GetAllDesigns().Count());

        ClassicAssert.IsEmpty(SelectionManager.Instance.Selected.ToArray(), "Deleting the view should remove it from the active selection");

        cmd.Undo();
        ClassicAssert.AreEqual(2, designOut.GetAllDesigns().Count());
        ClassicAssert.Contains(lbl1Design, SelectionManager.Instance.Selected.ToArray(), "Undoing a delete operation should restore the previous selection");
    }

    [Test]
    public void TestPreventDeleting_PopulatedWhenDependenciesExist()
    {
        var viewToCode = new ViewToCode(App);
        var file = new FileInfo("TestPreventDeleting_PopulatedWhenDependenciesExist.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var lbl1 = ViewFactory.Create<Label>();
        var lbl2 = ViewFactory.Create<Label>();

        // add 2 labels
        new AddViewOperation(App, lbl1, designOut, "lbl1").Do();
        new AddViewOperation(App, lbl2, designOut, "lbl2").Do();

        // Add dependency: lbl2 depends on lbl1
        lbl2.X = Pos.Right(lbl1) + 5;

        var cmd = new DeleteViewOperation(App, (Design)lbl1.Data);

        ClassicAssert.IsTrue(cmd.IsImpossible, "Deleting lbl1 should be impossible because lbl2 depends on it");
        ClassicAssert.AreEqual(1, cmd.PreventDeleting.Length, "PreventDeleting should contain exactly one dependent design");
        ClassicAssert.AreSame(lbl2.Data, cmd.PreventDeleting[0], "PreventDeleting should contain lbl2 because it depends on lbl1");
    }

    [Test]
    public void TestPreventDeleting_EmptyWhenNoDependencies()
    {
        var viewToCode = new ViewToCode(App);
        var file = new FileInfo("TestPreventDeleting_EmptyWhenNoDependencies.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

        var lbl1 = ViewFactory.Create<Label>();
        var lbl2 = ViewFactory.Create<Label>();

        // add 2 labels (no dependencies between them)
        new AddViewOperation(App, lbl1, designOut, "lbl1").Do();
        new AddViewOperation(App, lbl2, designOut, "lbl2").Do();

        var cmd = new DeleteViewOperation(App, (Design)lbl1.Data);

        ClassicAssert.IsFalse(cmd.IsImpossible, "Deleting lbl1 should be possible because nothing depends on it");
        ClassicAssert.IsEmpty(cmd.PreventDeleting, "PreventDeleting should be empty when no dependents exist");
    }

}
