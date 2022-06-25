using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace tests;

public class DragOperationTests : Tests
{
    [Test]
    public void TestSimpleDrag_Down3Rows()
    {
        var d = Get10By10View();

        var lbl = new Label(0,0,"Hi there buddy");
        var lblDesign = new Design(d.SourceCode,"mylabel",lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        // start drag in center of control
        var drag = new DragOperation(lblDesign,2,0,null);
        
        // drag down 3 lines
        drag.ContinueDrag(new Point(2,3));
        Assert.AreEqual(Pos.At(0),lbl.X);
        Assert.AreEqual(Pos.At(3),lbl.Y);
        
        // finalise the operation
        drag.Do();;
        Assert.AreEqual(Pos.At(0),lbl.X);
        Assert.AreEqual(Pos.At(3),lbl.Y);

        // now test undoing it
        drag.Undo();
        Assert.AreEqual(Pos.At(0),lbl.X);
        Assert.AreEqual(Pos.At(0),lbl.Y);

    }

    [Test]
    public void TestMultiDrag_Down3Rows()
    {
        var d = Get10By10View();

        var lbl1 = new Label(0,0,"Hi there buddy");
        var lbl2 = new Label(1,1,"Hi there buddy");

        var lblDesign1 = new Design(d.SourceCode,"mylabel1",lbl1);
        var lblDesign2 = new Design(d.SourceCode,"mylabel2",lbl2);
        lbl1.Data = lblDesign1;
        lbl2.Data = lblDesign2;

        d.View.Add(lbl1);
        d.View.Add(lbl2);

        // start drag in center of first control
        // while both are selected, this multi drags
        // both control down
        var drag = new DragOperation(lblDesign1,2,0,new []{lblDesign2});
        
        // drag down 3 lines
        drag.ContinueDrag(new Point(2,3));
        Assert.AreEqual(Pos.At(0),lbl1.X);
        Assert.AreEqual(Pos.At(3),lbl1.Y);
        Assert.AreEqual(Pos.At(1),lbl2.X);
        Assert.AreEqual(Pos.At(4),lbl2.Y);
        
        // finalise the operation
        drag.Do();;
        Assert.AreEqual(Pos.At(0),lbl1.X);
        Assert.AreEqual(Pos.At(3),lbl1.Y);
        Assert.AreEqual(Pos.At(1),lbl2.X);
        Assert.AreEqual(Pos.At(4),lbl2.Y);

        // now test undoing it
        drag.Undo();
        Assert.AreEqual(Pos.At(0),lbl1.X);
        Assert.AreEqual(Pos.At(0),lbl1.Y);
        Assert.AreEqual(Pos.At(1),lbl2.X);
        Assert.AreEqual(Pos.At(1),lbl2.Y);

    }

    //TODO: Drag into another view tests
}
