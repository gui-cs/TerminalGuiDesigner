using System;
using Terminal.Gui;
using TerminalGuiDesigner;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ViewExtensions ) )]
[Category( "Core" )]
[Category( "UI" )]
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
        var v = Get10By10View().View;

        v.X = 2;
        v.Y = 3;
        v.Width = 5;
        v.Height = 3;

        // Hit test does not find things that are not designable
        v.Data = new Design(new SourceCodeFile("MyView.cs"), "myview", v);

        Application.Top.Add(v);
        bool isLowerRight;
        bool isBorder;

        var result = v.HitTest(
            new MouseEventArgs
        {
                Position = new Point(x, y),
        }, out isBorder, out isLowerRight);

        // click didn't land in anything
        if (hit)
        {
            ClassicAssert.AreSame(v, result);
        }
        else
        {
            ClassicAssert.AreSame(Application.Top,result);
        }

        ClassicAssert.AreEqual(lowerRight, isLowerRight);
        ClassicAssert.AreEqual(border, isBorder);
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

        ClassicAssert.AreEqual(expectIsContainerView, inst.IsContainerView());
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

        ClassicAssert.AreEqual(expectResult, inst.IsBorderlessContainerView());
    }

    [Test]
    public void TestHitTest_WindowWithFrameView_InBorder()
    {
        var w = new Window();
        var f = new FrameView()
        {
            Width = 5,
            Height = 5,
        };

        // Hit test does not find things that are not designable
        f.Data = new Design(new SourceCodeFile("MyView.cs"), "myframe", f);

        w.Add(f);
        Application.Begin(w);
        w.LayoutSubViews();

        ClassicAssert.AreSame(w, w.HitTest(new MouseEventArgs {Position = new Point(13, 0) }, out var isBorder, out _),
            "Expected 0,0 to be the window border (its client area should start at 1,1)");
        ClassicAssert.IsTrue(isBorder);

        // 1,1
        ClassicAssert.AreSame(f, w.HitTest(new MouseEventArgs {Position = new Point(1, 1) }, out isBorder, out _),
            "Expected 1,1 to be the Frame border (its client area should start at 1,1)");
        ClassicAssert.IsTrue(isBorder);
    }
}
