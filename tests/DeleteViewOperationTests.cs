using NUnit.Framework;
using System.IO;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests
{
    internal class DeleteViewOperationTests : Tests
    {
        [Test]
        public void TestDeletingObjectWithDependency_IsImpossible()
        {
            var viewToCode = new ViewToCode();

            var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
            var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View), out var sourceCode);

            var factory = new ViewFactory();
            var lbl1 = (Label)factory.Create(typeof(Label));
            var lbl2 = (Label)factory.Create(typeof(Label));

            // add 2 labels
            new AddViewOperation(sourceCode,lbl1,designOut,"lbl1").Do();
            new AddViewOperation(sourceCode, lbl2, designOut, "lbl2").Do();

            // not impossible, we could totalyy delete either of these
            Assert.IsFalse(new DeleteViewOperation(lbl1).IsImpossible);
            Assert.IsFalse(new DeleteViewOperation(lbl2).IsImpossible);

            // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
            lbl2.X = Pos.Right(lbl1) + 5;

            Assert.IsTrue(new DeleteViewOperation(lbl1).IsImpossible);
        }
    }
}
