using Terminal.Gui.ViewBase;

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
        var lblDesign = new Design(App, d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        // start drag in center of control
        var drag = new DragOperation(App, lblDesign, 2, 0, null);

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
        var lblDesign = new Design(App, d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        App.TopRunnableView.Add(d.View);
        App.TopRunnableView.LayoutSubViews();

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

        var lblDesign1 = new Design(App, d.SourceCode, "mylabel1", lbl1);
        var lblDesign2 = new Design(App, d.SourceCode, "mylabel2", lbl2);
        lbl1.Data = lblDesign1;
        lbl2.Data = lblDesign2;

        d.View.Add(lbl1);
        d.View.Add(lbl2);

        // start drag in center of first control
        // while both are selected, this multi drags
        // both control down
        var drag = new DragOperation(App, lblDesign1, 2, 0, new[] { lblDesign2 });

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
        var lblDesign = new Design(App, d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // Label is at 3,4 on the screen

        // user clicks mouse down at top left of the label
        var drag = new DragOperation(App, lblDesign, 1, 2, null);
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

        // setup 2 large SubViews at diagonals
        // to one another within the main 10x10 view
        var container1 = new View
        {
            X = 0,
            Y = 1,
            Width = 5,
            Height = 4,
        };
        container1.Data = new Design(App, d.SourceCode, "v1", container1);

        var container2 = new View
        {
            X = 5,
            Y = 6,
            Width = 5,
            Height = 4,
        };
        container2.Data = new Design(App, d.SourceCode, "v2", container2);

        d.View.Add(container1);
        d.View.Add(container2);

        var lbl = new Label
        {
            X = 1,
            Y = 2,
            Text = "Hi there buddy"
        };

        var lblDesign = new Design(App, d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // start drag in center of control
        var drag = new DragOperation(App, lblDesign, 3, 2, null);

        // drag down to 1,1 of the other box
        drag.ContinueDrag(new Point(6, 7));
        drag.DropInto = container2;

        ClassicAssert.AreEqual(Pos.Absolute(4), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(7), lbl.Y);
        ClassicAssert.Contains(lbl, container1.SubViews.ToArray(), "Did not expect continue drag to move to a new container");

        // finalise the operation
        drag.Do();
        ClassicAssert.IsFalse(container1.SubViews.Contains(lbl));
        ClassicAssert.Contains(lbl, container2.SubViews.ToArray(), "Expected new container to be the one we dropped into");
        ClassicAssert.AreEqual(Pos.Absolute(-1), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(2), lbl.Y);

        // now test undoing it
        drag.Undo();
        ClassicAssert.AreEqual(Pos.Absolute(1), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(2), lbl.Y);
        ClassicAssert.Contains(lbl, container1.SubViews.ToArray(), "Expected undo to return view to its original parent");
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
        var op = new AddViewOperation(App, frameView, rootDesign, "frame");
        op.Do();

        // Window has an invisible sub-view that forces everything in by 1 to make border
        // for window.  So does FrameView.

        /*Window client area starts at (1,1) + (10,10 X/Y) + (1,1) for border of FrameView*/

        screen = frameView.ContentToScreen(new Point(0, 0));
        ClassicAssert.AreEqual(12, screen.X);
        ClassicAssert.AreEqual(12, screen.Y);

        var lbl = new Label{ X = 1, Y = 2, Text = "Hi there buddy" };
        var lblDesign = new Design(App, rootDesign.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        frameView.Add(lbl);

        App.TopRunnableView.Add(rootDesign.View);
        App.TopRunnableView.LayoutSubViews();

        // check screen coordinates are as expected
        screen = lblDesign.View.ContentToScreen(new System.Drawing.Point(0, 0));
        ClassicAssert.AreEqual(13, screen.X, "Expected label X screen to be at its parents 0,0 (11,11) + 1");
        ClassicAssert.AreEqual(14, screen.Y, "Expected label Y screen to be at its parents 0,0 (11,11) + 2");

        // press down at 0,0 of the label
        ClassicAssert.AreEqual(lbl, rootDesign.View.HitTest(App, new Mouse { Position = new Point(13, 14) }, out _, out _)
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
    public void TestDropInto_SelfIgnored()
    {
        var d = Get10By10View();
        var lbl = new Label { X = 1, Y = 1, Text = "Hello" };
        var lblDesign = new Design(App, d.SourceCode, "lbl", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        var drag = new DragOperation(App, lblDesign, 1, 1, null);

        // Try to set DropInto to itself
        drag.DropInto = lbl;

        ClassicAssert.IsNull(drag.DropInto, "Should ignore attempts to drop a view into itself");
    }

    [Test]
    public void TestDropInto_NonContainerIgnored()
    {
        var d = Get10By10View();

        var lbl = new Label { X = 1, Y = 1, Text = "Hello" };
        var lblDesign = new Design(App, d.SourceCode, "lbl", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        var btn = new Button { Text = "Not a container" };
        d.View.Add(btn);

        var drag = new DragOperation(App, lblDesign, 1, 1, null);

        // Try to drop into a non-container view
        drag.DropInto = btn;

        ClassicAssert.IsNull(drag.DropInto, "Should ignore attempts to drop into non-container views");
    }

    [Test]
    public void TestDropInto_DependentViews_MakesImpossible()
    {
        var d = Get10By10View();

        // Parent container
        var container1 = new View { Width = 10, Height = 10 };
        container1.Data = new Design(App, d.SourceCode, "c1", container1);
        d.View.Add(container1);

        // Another container to drop into
        var container2 = new View { Width = 10, Height = 10 };
        container2.Data = new Design(App, d.SourceCode, "c2", container2);
        d.View.Add(container2);

        // Label inside container1
        var lbl = new Label { X = 1, Y = 1, Text = "Hello" };
        var lblDesign = new Design(App, d.SourceCode, "lbl", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // Another label that depends on lbl for positioning
        var lbl2 = new Label { X = Pos.Right(lbl) + 1, Y = 1, Text = "World" };
        var lblDesign2 = new Design(App, d.SourceCode, "lbl2", lbl2);
        lbl2.Data = lblDesign2;
        container1.Add(lbl2);

        var drag = new DragOperation(App, lblDesign, 1, 1, null);

        // Try to move lbl into container2
        drag.DropInto = container2;

        ClassicAssert.IsTrue(drag.IsImpossible, "Should be impossible to drop when dependent views exist that are not part of the drag");
        ClassicAssert.Contains(lbl2, container1.SubViews.ToArray(), "Dependent view should remain in original container");
    }

    [Test]
    public void TestDropInto_AllDependantsDragged_Allowed()
    {
        var d = Get10By10View();

        var container1 = new View { Width = 10, Height = 10 };
        container1.Data = new Design(App, d.SourceCode, "c1", container1);
        d.View.Add(container1);

        var container2 = new View { Width = 10, Height = 10 };
        container2.Data = new Design(App, d.SourceCode, "c2", container2);
        d.View.Add(container2);

        // First label
        var lbl1 = new Label { X = 1, Y = 1, Text = "One" };
        var lblDesign1 = new Design(App, d.SourceCode, "lbl1", lbl1);
        lbl1.Data = lblDesign1;
        container1.Add(lbl1);

        // Second label depends on lbl1
        var lbl2 = new Label { X = Pos.Right(lbl1) + 1, Y = 1, Text = "Two" };
        var lblDesign2 = new Design(App, d.SourceCode, "lbl2", lbl2);
        lbl2.Data = lblDesign2;
        container1.Add(lbl2);

        // Drag lbl1 and lbl2 together
        var drag = new DragOperation(App, lblDesign1, 1, 1, new[] { lblDesign2 });

        // Try to drop into container2
        drag.DropInto = container2;

        ClassicAssert.IsFalse(drag.IsImpossible, "Dragging both dependants together should be allowed");
        drag.Do();

        ClassicAssert.Contains(lbl1, container2.SubViews.ToArray());
        ClassicAssert.Contains(lbl2, container2.SubViews.ToArray());
    }
    [Test]
    public void TestAbandon_RestoresOriginalPosition()
    {
        var d = Get10By10View();

        var container1 = new View { Width = 10, Height = 10 };
        container1.Data = new Design(App, d.SourceCode, "c1", container1);
        d.View.Add(container1);

        var container2 = new View { Width = 10, Height = 10 };
        container2.Data = new Design(App, d.SourceCode, "c2", container2);
        d.View.Add(container2);

        // Label in container1
        var lbl = new Label { X = 1, Y = 2, Text = "Hello" };
        var lblDesign = new Design(App, d.SourceCode, "lbl", lbl);
        lbl.Data = lblDesign;
        container1.Add(lbl);

        // Another label depending on the first
        var lbl2 = new Label { X = Pos.Right(lbl) + 1, Y = 2, Text = "World" };
        var lblDesign2 = new Design(App, d.SourceCode, "lbl2", lbl2);
        lbl2.Data = lblDesign2;
        container1.Add(lbl2);

        var drag = new DragOperation(App, lblDesign, 1, 2, null);

        // Move to a legal new position
        drag.ContinueDrag(new Point(3, 4));
        ClassicAssert.AreEqual(Pos.Absolute(3), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(4), lbl.Y);

        // Now try to drop into container2 (impossible, because lbl2 depends on lbl)
        drag.DropInto = container2;
        ClassicAssert.IsTrue(drag.IsImpossible);

        // Call Abandon � should snap lbl back to original
        drag.Abandon();

        ClassicAssert.AreEqual(Pos.Absolute(1), lbl.X);
        ClassicAssert.AreEqual(Pos.Absolute(2), lbl.Y);
        ClassicAssert.Contains(lbl, container1.SubViews.ToArray(), "Expected abandon to restore original container");
    }


}
