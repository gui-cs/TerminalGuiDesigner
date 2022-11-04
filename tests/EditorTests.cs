using NUnit.Framework;
using System;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI;

namespace tests
{
    internal class EditorTests : Tests
    {
        [Test]
        public void TestHasUnsavedChanges()
        {
            var e = new Editor();

            Assert.IsFalse(e.HasUnsavedChanges(), "With nothing open there should not be any unsaved changes");

            OperationManager.Instance.Do(new DummyOperation());

            Assert.IsTrue(e.HasUnsavedChanges(), "We have performed an operation and not yet saved");

            // fake a save
            var lastOp = OperationManager.Instance.GetLastAppliedOperation() ?? throw new Exception("Expected DummyOperation to be known as the last performed");
            var f = typeof(Editor).GetField("_lastSavedOperation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ?? throw new Exception("Missing field");
            f.SetValue(e, lastOp.UniqueIdentifier);

            Assert.IsFalse(e.HasUnsavedChanges(), "Now that we have saved there should be no unsaved changes");

            OperationManager.Instance.Do(new DummyOperation());
            Assert.IsTrue(e.HasUnsavedChanges(), "When we perform an operation after saving we now have changes again");

            OperationManager.Instance.Undo();
            Assert.IsFalse(e.HasUnsavedChanges(), "Undoing the newly performed operation should mean that we are back where we were when we saved (i.e. no changes)");
        }

        class DummyOperation : Operation
        {
            public override bool Do()
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
}
