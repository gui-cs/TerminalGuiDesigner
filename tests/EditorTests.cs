using System;
using System.Reflection;
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
    public void TestHasUnsavedChanges( )
    {
        //TODO: This test should be re-factored to test only HasUnsavedChanges
        Editor e = new Editor( );
        Assume.That( e, Is.Not.Null.And.InstanceOf<Editor>( ) );

        Assert.That( e.HasUnsavedChanges, Is.False, "With nothing open there should not be any unsaved changes" );

        DummyOperation dummyOperation1 = new( );
        Assume.That( dummyOperation1, Is.Not.Null.And.InstanceOf<DummyOperation>( ) );
        Assume.That( dummyOperation1.IsImpossible, Is.False );

        //TODO: This is an inconsistency in the execution model.
        //Unsaved change tracking is dependent on calling Do on the OperationManager and will not work if called on the operation itself.
        //See https://github.com/dodexahedron/TerminalGuiDesigner/issues/34
        bool dummyOperation1Succeeded = false;
        Assert.That( ( ) => dummyOperation1Succeeded = OperationManager.Instance.Do( dummyOperation1 ), Throws.Nothing );
        Assert.That( dummyOperation1Succeeded );

        Assert.That( e.HasUnsavedChanges, "We have performed an operation and not yet saved" );

        // fake a save
        IOperation? saveMock = null;
        Assert.That( ( ) => saveMock = OperationManager.Instance.GetLastAppliedOperation( ), Throws.Nothing );
        Assert.That( saveMock, Is.Not.Null.And.InstanceOf<DummyOperation>( ), "Expected DummyOperation to be known as the last performed" );

        e.LastSavedOperation = saveMock!.UniqueIdentifier;
        Assert.That( e.HasUnsavedChanges, Is.False, "Now that we have saved there should be no unsaved changes" );

        DummyOperation dummyOperation2 = new( );
        Assume.That( dummyOperation2, Is.Not.Null.And.InstanceOf<DummyOperation>( ) );
        Assume.That( dummyOperation2.IsImpossible, Is.False );

        bool dummyOperation2Succeeded = false;
        Assert.That( ( ) => dummyOperation2Succeeded = OperationManager.Instance.Do( dummyOperation2 ), Throws.Nothing );
        Assert.That( dummyOperation2Succeeded );
        Assert.That( e.HasUnsavedChanges, "When we perform an operation after saving we now have changes again" );

        Assert.That( ( ) => OperationManager.Instance.Undo( ), Throws.Nothing );
        Assert.That( e.HasUnsavedChanges, Is.False, "Undoing the newly performed operation should mean that we are back where we were when we saved (i.e. no changes)" );
    }

    [Test]
    public void DesignerCsIsAllowed()
    {
        var type = new AllowedType("designer", ".Designer.cs");
        ClassicAssert.IsTrue(type.IsAllowed("MyView.Designer.cs"));
    }

    private class DummyOperation : Operation
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
