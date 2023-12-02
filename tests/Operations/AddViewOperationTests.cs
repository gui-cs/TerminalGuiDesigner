using NUnit.Framework;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations;

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

            ClassicAssert.AreEqual(
                stackSize, d.View.Subviews.Count,
                "Expected the count of views to increase to match the number we have added");

            ClassicAssert.AreSame(
                instance, d.View.Subviews.Last(),
                "Expected the view instance that was added to be the same we passed to operation constructor");
            ClassicAssert.AreEqual(
                "blah" + (stackSize == 1 ? "" : stackSize.ToString()),
                ((Design)d.View.Subviews.Last().Data).FieldName,
                "Expected field name duplicates to be automatically resolved"
                );
        }
    }
    [Test]
    public void TestAddView_RoundTrip()
    {
        int stackSize = 0;
        var factory = new ViewFactory();
        var supportedViews = ViewFactory.GetSupportedViews()
            // Add MenuBar last so order is preserved in Assert check.
            .OrderBy(t=>t == typeof(MenuBar) ? int.MaxValue : 0).ToArray();

        var windowIn = RoundTrip<Toplevel, Window>((d, v) =>
        {
            foreach (var type in supportedViews)
            {
                stackSize++;
                var instance = factory.Create(type);
                var op = new AddViewOperation(instance, d, "blah");
                op.Do();
            }
        }, out _);

        ClassicAssert.AreEqual(stackSize, windowIn.GetActualSubviews().Count);

        for (int i = 0; i < stackSize; i++)
        {
            ClassicAssert.IsInstanceOf(supportedViews[i], windowIn.GetActualSubviews()[i]);
        }
    }

    [Test]
    public void TestAddView_UnDo()
    {
        var d = Get10By10View();
        var factory = new ViewFactory();

        int stackSize = 0;

        foreach (var type in ViewFactory.GetSupportedViews())
        {
            stackSize++;
            var instance = factory.Create(type);
            var op = new AddViewOperation(instance, d, "blah");
            OperationManager.Instance.Do(op);
            ClassicAssert.AreEqual(
                stackSize, d.View.Subviews.Count,
                "Expected the count of views to increase to match the number we have added");
        }

        for (int i = 1; i <= stackSize; i++)
        {
            OperationManager.Instance.Undo();
            ClassicAssert.AreEqual(
                stackSize-i, d.View.Subviews.Count,
                "Expected the count of views to decrease once each time we Undo");
        }

        ClassicAssert.IsEmpty(d.View.Subviews);
    }
}
