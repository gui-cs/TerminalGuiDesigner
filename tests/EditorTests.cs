using System;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(Editor))]
[Category("UI")]
[NonParallelizable]
internal class EditorTests : Tests
{
    [Test]
    public void TestHasUnsavedChanges()
    {
        var e = new Editor();
        Assume.That( e, Is.Not.Null.And.InstanceOf<Editor>( ) );

        Assert.That( e.HasUnsavedChanges(), Is.False, "With nothing open there should not be any unsaved changes" );

        OperationManager.Instance.Do(new DummyOperation());

        ClassicAssert.IsTrue(e.HasUnsavedChanges(), "We have performed an operation and not yet saved");

        // fake a save
        var lastOp = OperationManager.Instance.GetLastAppliedOperation() ?? throw new Exception("Expected DummyOperation to be known as the last performed");
        var f = typeof(Editor).GetField("lastSavedOperation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ?? throw new Exception("Missing field");
        f.SetValue(e, lastOp.UniqueIdentifier);

        Assert.That(e.HasUnsavedChanges(), Is.False, "Now that we have saved there should be no unsaved changes");

        OperationManager.Instance.Do(new DummyOperation());
        Assert.That(e.HasUnsavedChanges(), "When we perform an operation after saving we now have changes again");

        OperationManager.Instance.Undo();
        Assert.That( e.HasUnsavedChanges( ), Is.False "Undoing the newly performed operation should mean that we are back where we were when we saved (i.e. no changes)" );
    }

    [Test]
    public void DesignerCsIsAllowed()
    {
        var type = new AllowedType("designer", ".Designer.cs");
        ClassicAssert.IsTrue(type.IsAllowed("MyView.Designer.cs"));
    }

    class DummyOperation : Operation
    {
        protected override bool DoImpl()
        {
            return true;
        }

        public override void Redo()
        {
        }

        public override void Undo()
        {
        }
    }
}
