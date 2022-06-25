using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace tests;

public class DragOperationTests : Tests
{
    [Test]
    public void TestSimpleDrag_Down3()
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

    //TODO: Drag into another view tests
}
