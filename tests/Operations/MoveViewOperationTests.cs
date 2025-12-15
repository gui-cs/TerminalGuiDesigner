using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using TerminalGuiDesigner;

namespace UnitTests.Operations
{
    internal class MoveViewOperationTests : Tests
    {
        [Test]
        public void TestMoveAbsolutePosition_XYChanges()
        {
            var viewToCode = new ViewToCode(Mock.Of<IApplication>());
            var file = new FileInfo("TestMoveAbsolutePosition_XYChanges.cs");
            var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

            designOut.View.Width = 10;
            designOut.View.Height = 10;

            var lbl = ViewFactory.Create<Label>();
            lbl.X = 1;
            lbl.Y = 2;

            new AddViewOperation(Mock.Of<IApplication>(), lbl, designOut, "lbl").Do();
            var lblDesign = (Design)lbl.Data;

            var move = new MoveViewOperation(Mock.Of<IApplication>(), lblDesign, 3, 4);
            

            ClassicAssert.IsFalse(move.IsImpossible, "Should be possible to move an absolute position");
            ClassicAssert.AreEqual(4, move.DestinationX);
            ClassicAssert.AreEqual(6, move.DestinationY);
            move.Do();


            ClassicAssert.IsTrue(move.Do());
            ClassicAssert.AreEqual(Pos.Absolute(4), lbl.X);
            ClassicAssert.AreEqual(Pos.Absolute(6), lbl.Y);

            move.Undo();
            ClassicAssert.AreEqual(Pos.Absolute(1), lbl.X);
            ClassicAssert.AreEqual(Pos.Absolute(2), lbl.Y);

            move.Redo();
            ClassicAssert.AreEqual(Pos.Absolute(4), lbl.X);
            ClassicAssert.AreEqual(Pos.Absolute(6), lbl.Y);
        }

        [Test]
        public void TestMoveRelativePosition_IsImpossible()
        {
            var viewToCode = new ViewToCode(Mock.Of<IApplication>());
            var file = new FileInfo("TestMoveRelativePosition_IsImpossible.cs");
            var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View));

            var lbl = ViewFactory.Create<Label>();
            lbl.X = Pos.Center();
            lbl.Y = Pos.Center();

            new AddViewOperation(Mock.Of<IApplication>(), lbl, designOut, "lbl").Do();
            var lblDesign = (Design)lbl.Data;

            var move = new MoveViewOperation(Mock.Of<IApplication>(), lblDesign, 5, 5);

            ClassicAssert.IsTrue(move.IsImpossible, "Relative positions should not be moved");
            ClassicAssert.AreEqual(lbl.X, move.OriginX);
            ClassicAssert.AreEqual(lbl.Y, move.OriginY);

            // Do should still be safe to call but not change anything
            move.Do();
            ClassicAssert.AreEqual(Pos.Center(), lbl.X);
            ClassicAssert.AreEqual(Pos.Center(), lbl.Y);
        }

        [Test]
        public void TestMoveBeyondBounds_ClampedToParent()
        {
            var viewToCode = new ViewToCode(Mock.Of<IApplication>());
            var file = new FileInfo("TestMoveBeyondBounds_ClampedToParent.cs");
            var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

            var lbl = ViewFactory.Create<Label>();
            lbl.X = 0;
            lbl.Y = 0;


            designOut.View.Width = 10;
            designOut.View.Height = 10;


            new AddViewOperation(Mock.Of<IApplication>(), lbl, designOut, "lbl").Do();
            var lblDesign = (Design)lbl.Data;

            var parentSize = designOut.View.GetContentSize();

            // Try to move far outside the parent
            var move = new MoveViewOperation(lblDesign, parentSize.Width + 100, parentSize.Height + 100);
            move.Do();

            ClassicAssert.IsFalse(move.IsImpossible, "Operation should still be valid but clamped");
            ClassicAssert.LessOrEqual(move.DestinationX, parentSize.Width - 1);
            ClassicAssert.LessOrEqual(move.DestinationY, parentSize.Height - 1);

            move.Do();
            ClassicAssert.AreEqual(Pos.Absolute(move.DestinationX), lbl.X);
            ClassicAssert.AreEqual(Pos.Absolute(move.DestinationY), lbl.Y);
        }
    }
}
