using NUnit.Framework;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations
{
    internal class AddViewOperationTests : Tests
    {
        [Test]
        public void TestAddView_Do()
        {
            var d = Get10By10View();
            var factory = new ViewFactory();
            
            int stackSize = 0;

            foreach(var type in ViewFactory.GetSupportedViews())
            {
                stackSize++;
                var instance = factory.Create(type);
                var op = new AddViewOperation(instance, d, "blah");
                op.Do();

                Assert.AreEqual(
                    stackSize, d.View.Subviews.Count,
                    "Expected the count of views to increase to match the number we have added");

                Assert.AreSame(
                    instance, d.View.Subviews.Last(),
                    "Expected the view instance that was added to be the same we passed to operation constructor");
                Assert.AreEqual(
                    "blah" + (stackSize == 1 ? "" : stackSize.ToString()),
                    ((Design)d.View.Subviews.Last().Data).FieldName,
                    "Expected field name duplicates to be automatically resolved"
                    );

            }
        }
    }
}
