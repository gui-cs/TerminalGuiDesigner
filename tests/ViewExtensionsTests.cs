using System;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace UnitTests;

internal class ViewExtensionsTests : Tests
{
    // upper left corner tests
    [TestCase(1, 1, false, false, false)]
    [TestCase(2, 3, true, true, false)]
    [TestCase(2, 4, true, true, false)]
    [TestCase(3, 3, true, true, false)]
    [TestCase(3, 4, true, false, false)]

    // lower left corner tests
    [TestCase(1, 6, false, false, false)]
    [TestCase(2, 5, true, true, false)]
    [TestCase(2, 3, true, true, false)]
    [TestCase(3, 5, true, true, false)]
    [TestCase(3, 4, true, false, false)]
    public void TestHitTest(int x, int y, bool hit, bool border, bool lowerRight)
    {
        var v = this.Get10By10View().View;

        v.X = 2;
        v.Y = 3;
        v.Width = 5;
        v.Height = 3;

        Application.Top.Add(v);
        bool isLowerRight;
        bool isBorder;

        var result = v.HitTest(
            new MouseEvent
        {
            X = x,
            Y = y,
        }, out isBorder, out isLowerRight);

        // click didn't land in anything
        if (hit)
        {
            Assert.AreSame(v, result);
        }
        else
        {
            Assert.IsNull(result);
        }

        Assert.AreEqual(lowerRight, isLowerRight);
        Assert.AreEqual(border, isBorder);
    }

    [TestCase(typeof(Label), false)]
    [TestCase(typeof(TableView), false)]
    [TestCase(typeof(TabView), true)]
    [TestCase(typeof(View), true)]
    [TestCase(typeof(Window), true)]
    public void TestIsContainerView(Type viewType, bool expectIsContainerView)
    {
        var inst = (View?)Activator.CreateInstance(viewType)
            ?? throw new Exception("CreateInstance returned null!");

        Assert.AreEqual(expectIsContainerView, inst.IsContainerView());
    }

    [TestCase(typeof(Label), false)]
    [TestCase(typeof(TableView), false)]
    [TestCase(typeof(TabView), false)]
    [TestCase(typeof(Window), false)]
    [TestCase(typeof(View), true)]
    public void TestOutOfBox_IsBorderlessContainerView(Type viewType, bool expectResult)
    {
        var inst = (View?)Activator.CreateInstance(viewType)
            ?? throw new Exception("CreateInstance returned null!");

        Assert.AreEqual(expectResult, inst.IsBorderlessContainerView());
    }
}
