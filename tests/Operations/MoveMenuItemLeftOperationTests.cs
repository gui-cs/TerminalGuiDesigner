using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations
{
    internal class MoveMenuItemLeftOperationTests : Tests
    {
        [Test]
        public void TestMoveMenuItemLeft_CannotForRootItems()
        {
            RoundTrip<Toplevel, MenuBar>((d,v)=>
            {
                var op = new MoveMenuItemLeftOperation(v.Menus[0].Children[0]);
                Assert.IsTrue(op.IsImpossible, "Expected it to be impossible to move left a menu that is under a root MenuBar Item (e.g. items under File, Edit, View etc)");
                Assert.IsFalse(op.Do());
            }
            ,out _);
        }

        [Test]
        public void TestMoveMenuItemLeft_CannotMoveOrphans()
        {
            // This is not connected to anything!
            var haha = new MenuItem(); 

            var op = new MoveMenuItemLeftOperation(haha);
            Assert.IsTrue(op.IsImpossible, "Expected it to be impossible to move left a menu that is under a root MenuBar Item (e.g. items under File, Edit, View etc)");
            Assert.IsFalse(op.Do());
        }
    }
}
