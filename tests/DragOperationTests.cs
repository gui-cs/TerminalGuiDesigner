using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests;

public class DragOperationTests : Tests
{
    [Test]
    public void TestSimpleDrag_Down3Rows()
    {
        var d = this.Get10By10View();

        var lbl = new Label(0, 0, "Hi there buddy");
        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        // start drag in center of control
        var drag = new DragOperation(lblDesign, 2, 0, null);

        // drag down 3 lines
        drag.ContinueDrag(new Point(2, 3));
        Assert.AreEqual(Pos.At(0), lbl.X);
        Assert.AreEqual(Pos.At(3), lbl.Y);

        // finalise the operation
        drag.Do();
        Assert.AreEqual(Pos.At(0), lbl.X);
        Assert.AreEqual(Pos.At(3), lbl.Y);

        // now test undoing it
        drag.Undo();
        Assert.AreEqual(Pos.At(0), lbl.X);
        Assert.AreEqual(Pos.At(0), lbl.Y);
    }

    [Test]
    public void TestMultiDrag_Down3Rows()
    {
        var d = this.Get10By10View();

        var lbl1 = new Label(0, 0, "Hi there buddy");
        var lbl2 = new Label(1, 1, "Hi there buddy");

        var lblDesign1 = new Design(d.SourceCode, "mylabel1", lbl1);
        var lblDesign2 = new Design(d.SourceCode, "mylabel2", lbl2);
        lbl1.Data = lblDesign1;
        lbl2.Data = lblDesign2;

        d.View.Add(lbl1);
        d.View.Add(lbl2);

        // start drag in center of first control
        // while both are selected, this multi drags
        // both control down
        var drag = new DragOperation(lblDesign1, 2, 0, new[] { lblDesign2 });

        // drag down 3 lines
        drag.ContinueDrag(new Point(2, 3));
        Assert.AreEqual(Pos.At(0), lbl1.X);
        Assert.AreEqual(Pos.At(3), lbl1.Y);
        Assert.AreEqual(Pos.At(1), lbl2.X);
        Assert.AreEqual(Pos.At(4), lbl2.Y);

        // finalize the operation
        drag.Do();
        Assert.AreEqual(Pos.At(0), lbl1.X);
        Assert.AreEqual(Pos.At(3), lbl1.Y);
        Assert.AreEqual(Pos.At(1), lbl2.X);
        Assert.AreEqual(Pos.At(4), lbl2.Y);

        // now test undoing it
        drag.Undo();
        Assert.AreEqual(Pos.At(0), lbl1.X);
        Assert.AreEqual(Pos.At(0), lbl1.Y);
        Assert.AreEqual(Pos.At(1), lbl2.X);
        Assert.AreEqual(Pos.At(1), lbl2.Y);
    }

    [Test]
    public void TestDragCoordinateSystem()
    {
        var d = this.Get10By10View();
        var container1 = new View
        {
            X = 2,
            Y = 2,
            Width = 8,
            Height = 8,
        };
        d.View.Add(container1);

        var lbl = new Label(1, 2, "Hi there buddy");
        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // Label is at 3,4 on the screen

        // user clicks mouse down at top left of the label
        var drag = new DragOperation(lblDesign, 1, 2, null);
        drag.Do();

        // In client coordinate system of container1 we have not moved the mouse
        // anywhere so the drag will not take the View to anywhere
        Assert.AreEqual(1, drag.DestinationX);
        Assert.AreEqual(2, drag.DestinationY);

        drag.ContinueDrag(new Point(2, 3));

        Assert.AreEqual(2, drag.DestinationX);
        Assert.AreEqual(3, drag.DestinationY);
        drag.Do();

        Assert.AreEqual(Pos.At(2), lbl.X);
        Assert.AreEqual(Pos.At(3), lbl.Y);
    }

    [Test]
    public void TestSimpleDrag_IntoAnotherView()
    {
        var d = this.Get10By10View();

        // setup 2 large subviews at diagonals
        // to one another within the main 10x10 view
        var container1 = new View
        {
            X = 0,
            Y = 1,
            Width = 5,
            Height = 4,
        };
        container1.Data = new Design(d.SourceCode, "v1", container1);

        var container2 = new View
        {
            X = 5,
            Y = 6,
            Width = 5,
            Height = 4,
        };
        container2.Data = new Design(d.SourceCode, "v2", container2);

        d.View.Add(container1);
        d.View.Add(container2);

        var lbl = new Label(1, 2, "Hi there buddy");
        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // start drag in center of control
        var drag = new DragOperation(lblDesign, 3, 2, null);

        // drag down to 1,1 of the other box
        drag.ContinueDrag(new Point(6, 7));
        drag.DropInto = container2;

        Assert.AreEqual(Pos.At(4), lbl.X);
        Assert.AreEqual(Pos.At(7), lbl.Y);
        Assert.Contains(lbl, container1.Subviews.ToArray(), "Did not expect continue drag to move to a new container");

        // finalise the operation
        drag.Do();
        Assert.IsFalse(container1.Subviews.Contains(lbl));
        Assert.Contains(lbl, container2.Subviews.ToArray(), "Expected new container to be the one we dropped into");
        Assert.AreEqual(Pos.At(-1), lbl.X);
        Assert.AreEqual(Pos.At(2), lbl.Y);

        // now test undoing it
        drag.Undo();
        Assert.AreEqual(Pos.At(1), lbl.X);
        Assert.AreEqual(Pos.At(2), lbl.Y);
        Assert.Contains(lbl, container1.Subviews.ToArray(), "Expected undo to return view to its original parent");
    }
}
