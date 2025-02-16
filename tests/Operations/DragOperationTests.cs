using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI;

namespace UnitTests.Operations;

/// <summary>
/// Tests for the <see cref="DragOperation"/> class and <see cref="MouseManager"/> areas
/// that are to do with triggering drag operations.
/// </summary>
internal class DragOperationTests : Tests
{
    /// <summary>
    /// Tests dragging a <see cref="Label"/> down in a single root container
    /// using <see cref="DragOperation"/> explicitly (i.e. not mouse)
    /// </summary>
    [Test]
    public void TestSimpleDrag_Down3Rows()
    {
        var d = Get10By10View();

        var lbl = new Label { Text = "Hi there buddy" };
        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        // start drag in center of control
        var drag = new DragOperation(lblDesign, 2, 0, null);

        // drag down 3 lines
        drag.ContinueDrag(new Point(2, 3));
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(3), lbl.Y);

        // finalise the operation
        drag.Do();
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(3), lbl.Y);

        // now test undoing it
        drag.Undo();
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl.Y);
    }

    /// <summary>
    /// Tests dragging a <see cref="Label"/> down in a single root container
    /// using the Mouse
    /// </summary>
    [Test]
    public void TestSimpleDrag_Down3Rows_WithMouse()
    {
        var d = Get10By10View();

        var lbl = new Label
        {
            Text = "Hi there buddy"
        };
        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        Application.Top.Add(d.View);
        Application.Top.LayoutSubviews();

        MouseDrag(d, 2, 0, 2, 3);

        // finalize the operation
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(3), lbl.Y);

        // now test undoing it
        OperationManager.Instance.Undo();
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl.Y);
    }



    [Test]
    public void TestMultiDrag_Down3Rows()
    {
        var d = Get10By10View();

        var lbl1 = new Label{ Text = "Hi there buddy" };
        var lbl2 = new Label{
            X = 1,
            Y = 1,
            Text = "Hi there buddy"
        };

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
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl1.X);
        ClassicAssert.AreEqual(Pos.Absolute(3), lbl1.Y);
        ClassicAssert.AreEqual(Pos.Absolute(1), lbl2.X);
        ClassicAssert.AreEqual(Pos.Absolute(4), lbl2.Y);

        // finalize the operation
        drag.Do();
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl1.X);
        ClassicAssert.AreEqual(Pos.Absolute(3), lbl1.Y);
        ClassicAssert.AreEqual(Pos.Absolute(1), lbl2.X);
        ClassicAssert.AreEqual(Pos.Absolute(4), lbl2.Y);

        // now test undoing it
        drag.Undo();
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl1.X);
        ClassicAssert.AreEqual(Pos.Absolute(0), lbl1.Y);
        ClassicAssert.AreEqual(Pos.Absolute(1), lbl2.X);
        ClassicAssert.AreEqual(Pos.Absolute(1), lbl2.Y);
    }

    [Test]
    public void TestDragCoordinateSystem()
    {
        var d = Get10By10View();
        var container1 = new View
        {
            X = 2,
            Y = 2,
            Width = 8,
            Height = 8,
        };
        d.View.Add(container1);

        var lbl = new Label
        {
            X=1,
            Y=2,
            Text = "Hi there buddy"
        };
        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // Label is at 3,4 on the screen

        // user clicks mouse down at top left of the label
        var drag = new DragOperation(lblDesign, 1, 2, null);
        drag.Do();

        // In client coordinate system of container1 we have not moved the mouse
        // anywhere so the drag will not take the View to anywhere
        ClassicAssert.AreEqual(1, drag.DestinationX);
        ClassicAssert.AreEqual(2, drag.DestinationY);

        drag.ContinueDrag(new Point(2, 3));

        ClassicAssert.AreEqual(2, drag.DestinationX);
        ClassicAssert.AreEqual(3, drag.DestinationY);
        drag.Do();

        ClassicAssert.AreEqual(Pos.Absolute(2), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(3), lbl.Y);
    }

    [Test]
    public void TestSimpleDrag_IntoAnotherView()
    {
        var d = Get10By10View();

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

        var lbl = new Label
        {
            X = 1,
            Y = 2,
            Text = "Hi there buddy"
        };

        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // start drag in center of control
        var drag = new DragOperation(lblDesign, 3, 2, null);

        // drag down to 1,1 of the other box
        drag.ContinueDrag(new Point(6, 7));
        drag.DropInto = container2;

        ClassicAssert.AreEqual(Pos.Absolute(4), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(7), lbl.Y);
        ClassicAssert.Contains(lbl, container1.Subviews.ToArray(), "Did not expect continue drag to move to a new container");

        // finalise the operation
        drag.Do();
        ClassicAssert.IsFalse(container1.Subviews.Contains(lbl));
        ClassicAssert.Contains(lbl, container2.Subviews.ToArray(), "Expected new container to be the one we dropped into");
        ClassicAssert.AreEqual(Pos.Absolute(-1), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(2), lbl.Y);

        // now test undoing it
        drag.Undo();
        ClassicAssert.AreEqual(Pos.Absolute(1), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(2), lbl.Y);
        ClassicAssert.Contains(lbl, container1.Subviews.ToArray(), "Expected undo to return view to its original parent");
    }

    [Test]
    public void TestSimpleDrag_OutOfFrameView_IntoRootWindow()
    {
        var rootDesign = Get100By100<Window>();
        rootDesign.View.X = 0;
        rootDesign.View.Y = 0;

        var screen = rootDesign.View.ContentToScreen(new Point(0, 0));


        // A window is positioned at 0,0 but its client area (to which controls are added) is 1,1 due to border
        ClassicAssert.AreEqual(1, screen.X);
        ClassicAssert.AreEqual(1, screen.Y);

        var frameView = ViewFactory.Create(typeof(FrameView));
        frameView.X = 10;
        frameView.Y = 10;
        var op = new AddViewOperation(frameView, rootDesign, "frame");
        op.Do();

        // Window has an invisible sub-view that forces everything in by 1 to make border
        // for window.  So does FrameView.

        /*Window client area starts at (1,1) + (10,10 X/Y) + (1,1) for border of FrameView*/

        screen = frameView.ContentToScreen(new Point(0, 0));
        ClassicAssert.AreEqual(12, screen.X);
        ClassicAssert.AreEqual(12, screen.Y);

        var lbl = new Label{ X = 1, Y = 2, Text = "Hi there buddy" };
        var lblDesign = new Design(rootDesign.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        frameView.Add(lbl);

        Application.Top.Add(rootDesign.View);
        Application.Top.LayoutSubviews();

        // check screen coordinates are as expected
        screen = lblDesign.View.ContentToScreen(new System.Drawing.Point(0, 0));
        ClassicAssert.AreEqual(13, screen.X, "Expected label X screen to be at its parents 0,0 (11,11) + 1");
        ClassicAssert.AreEqual(14, screen.Y, "Expected label Y screen to be at its parents 0,0 (11,11) + 2");

        // press down at 0,0 of the label
        ClassicAssert.AreEqual(lbl, rootDesign.View.HitTest(new MouseEventArgs { Position = new Point(13, 14) }, out _, out _)
            , "We just asked ViewToScreen for these same coordinates, how can they fail HitTest now?");

        // Drag up 4 so it is no longer in its parents container.
        // Label is at 1,2.  Up 2 brings it to 0 (just inside scroll)
        // Up 3 brings it onto the Frame border
        // Up 4 brings it into the root view
        MouseDrag(rootDesign, 13, 14, 13, 10);


        ClassicAssert.False(frameView.GetActualSubviews().Contains(lbl), "Expected label to no longer be in the scroll view");
        ClassicAssert.Contains(lblDesign.View, rootDesign.View.GetActualSubviews().ToArray(), "Expected label to have moved to the parent Window");
    }

    [Test]
    public void TestSimpleDrag_IntoTabView()
    {
        RoundTrip<View, TabView>((d, v) =>
        {
            // move TabView down a bit
            v.X = 2;
            v.Y = 2;

            // add a Button
            var op = new AddViewOperation(new Button() { Text = "Hello" }, d.GetRootDesign(), "mybtn");
            op.Do();

            Application.Top.Add(d.GetRootDesign().View);
            Application.Top.LayoutSubviews();


            ClassicAssert.AreEqual(0, v.Tabs.ElementAt(0).View.GetActualSubviews().Count, "Expected TabView Tab1 to start off empty");

            // Drag the Button into the TabView
            MouseDrag(d.GetRootDesign(), 0, 0, 3, 3);

            ClassicAssert.AreEqual(1, v.Tabs.ElementAt(0).View.GetActualSubviews().Count, "Expected TabView Tab1 to now contain Button");
            ClassicAssert.IsInstanceOf<Button>(v.Tabs.ElementAt(0).View.GetActualSubviews().Single());

        }, out _);
    }
}
