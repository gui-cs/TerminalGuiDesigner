using NUnit.Framework;
using System;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.TabOperations;

namespace UnitTests.Operations;

internal class AddTabOperationTests : Tests
{
    [Test]
    public void TestAddTab_WrongViewType()
    {
        var d = Get10By10View();
        var ex = Assert.Throws<ArgumentException>(() => new AddTabOperation(d, null));
        ClassicAssert.AreEqual("Design must wrap a TabView to be used with this operation.", ex?.Message);
    }

    [Test]
    public void TestAddTab_Do()
    {
        string? tab1Name = null;
        string? tab2Name = null;

        var tabIn = RoundTrip<Toplevel, TabView>(
            (d, v) =>
            {
                ClassicAssert.AreEqual(2, v.Tabs.Count, "Expected ViewFactory TabView to have a couple of example tabs when created");
                tab1Name = v.Tabs.ElementAt(0).Text.ToString();
                tab2Name = v.Tabs.ElementAt(1).Text.ToString();

                var op = new AddTabOperation(d, "Blarg");
                ClassicAssert.AreEqual(2, v.Tabs.Count, "Operation has not been run so why are there new tabs!");
                ClassicAssert.AreEqual(tab1Name, v.Tabs.ElementAt(0).Text);
                ClassicAssert.AreEqual(tab2Name, v.Tabs.ElementAt(1).Text);

                op.Do();
                ClassicAssert.AreEqual(3, v.Tabs.Count);
                ClassicAssert.AreEqual(tab1Name, v.Tabs.ElementAt(0).Text);
                ClassicAssert.AreEqual(tab2Name, v.Tabs.ElementAt(1).Text);
                ClassicAssert.AreEqual("Blarg", v.Tabs.ElementAt(2).Text);
            }, out _);

        ClassicAssert.AreEqual(3, tabIn.Tabs.Count);
        ClassicAssert.AreEqual(tab1Name, tabIn.Tabs.ElementAt(0).Text);
        ClassicAssert.AreEqual(tab2Name, tabIn.Tabs.ElementAt(1).Text);
        ClassicAssert.AreEqual("Blarg", tabIn.Tabs.ElementAt(2).Text);
    }

    [Test]
    public void TestAddTab_BlankName()
    {
        var tabIn = RoundTrip<Toplevel, TabView>(
            (d, v) =>
            {
                var op = new AddTabOperation(d, "  ");
                op.Do();
                ClassicAssert.AreEqual("blank", v.Tabs.ElementAt(2).Text);
            }, out _);

        ClassicAssert.AreEqual(3, tabIn.Tabs.Count);
        ClassicAssert.AreEqual("blank", tabIn.Tabs.ElementAt(2).Text);
    }

    [Test]
    public void TestAddTab_DuplicateNames()
    {
        var tabIn = RoundTrip<Toplevel, TabView>(
            (d, v) =>
            {
                var op = new AddTabOperation(d, "Blah");
                op.Do();
                op = new AddTabOperation(d, "Blah");
                op.Do();
                ClassicAssert.AreEqual(4, v.Tabs.Count);
                ClassicAssert.AreEqual("Blah", v.Tabs.ElementAt(2).Text);
                ClassicAssert.AreEqual("Blah2", v.Tabs.ElementAt(3).Text);
            }, out _);

        ClassicAssert.AreEqual(4, tabIn.Tabs.Count);
        ClassicAssert.AreEqual("Blah", tabIn.Tabs.ElementAt(2).Text);
        ClassicAssert.AreEqual("Blah2", tabIn.Tabs.ElementAt(3).Text);
    }

    [Test]
    public void TestAddTab_UnDo()
    {
        string? tab1Name = null;
        string? tab2Name = null;

        var tabIn = RoundTrip<Toplevel, TabView>(
            (d, v) =>
            {
                ClassicAssert.AreEqual(2, v.Tabs.Count, "Expected ViewFactory TabView to have a couple of example tabs when created");
                tab1Name = v.Tabs.ElementAt(0).Text.ToString();
                tab2Name = v.Tabs.ElementAt(1).Text.ToString();

                var op = new AddTabOperation(d, "Blarg");
                ClassicAssert.AreEqual(2, v.Tabs.Count, "Operation has not been run so why are there new tabs!");
                ClassicAssert.AreEqual(tab1Name, v.Tabs.ElementAt(0).Text);
                ClassicAssert.AreEqual(tab2Name, v.Tabs.ElementAt(1).Text);

                op.Do();
                ClassicAssert.AreEqual(3, v.Tabs.Count);
                ClassicAssert.AreEqual("Blarg", v.Tabs.ElementAt(2).Text);
                ClassicAssert.AreEqual(tab1Name, v.Tabs.ElementAt(0).Text);
                ClassicAssert.AreEqual(tab2Name, v.Tabs.ElementAt(1).Text);

                op.Undo();
                ClassicAssert.AreEqual(tab1Name, v.Tabs.ElementAt(0).Text);
                ClassicAssert.AreEqual(tab2Name, v.Tabs.ElementAt(1).Text);
                ClassicAssert.AreEqual(2, v.Tabs.Count);
            }, out _);

        ClassicAssert.AreEqual(2, tabIn.Tabs.Count);
        ClassicAssert.AreEqual(tab1Name, tabIn.Tabs.ElementAt(0).Text);
        ClassicAssert.AreEqual(tab2Name, tabIn.Tabs.ElementAt(1).Text);
    }
}
