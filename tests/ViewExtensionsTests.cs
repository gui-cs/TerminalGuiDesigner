using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace tests;

public class ViewExtensionsTests : Tests
{

    // upper left corner tests
    [TestCase(1,1,false,false,false)]
    [TestCase(2,3,true,true,false)]
    [TestCase(2,4,true,true,false)]
    [TestCase(3,3,true,true,false)]
    [TestCase(3,4,true,false,false)]

    // lower left corner tests
    [TestCase(1,6,false,false,false)]
    [TestCase(2,5,true,true,false)]
    [TestCase(2,3,true,true,false)]
    [TestCase(3,5,true,true,false)]
    [TestCase(3,4,true,false,false)]
    public void TestHitTest(int x, int y, bool hit,  bool border, bool lowerRight)
    {
        var v =  new View{
            X=2,
            Y=3,
            Width = 5,
            Height = 3
        };

        Application.Top.Add(v);
        bool isLowerRight;
        bool isBorder;

        var result = v.HitTest(new MouseEvent{
            X = x,
            Y = y
        }, out isBorder, out isLowerRight);

        // click didn't land in anything
        if(hit)
            Assert.AreSame(v,result);
        else
            Assert.IsNull(result);
        Assert.AreEqual(lowerRight, isLowerRight);
        Assert.AreEqual(border,isBorder);
    }
}
