using System;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI;

namespace UnitTests;

internal class MouseManagerTests : Tests
{
    [Test]
    public void TestDragLabel()
    {
        var d = this.Get10By10View();

        var lbl = new Label(0, 0, "Hi there buddy");
        var lblDesign = new Design(d.SourceCode, "mylabel", lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        var mgr = new MouseManager();

        // we haven't done anything yet
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);
        ClassicAssert.AreEqual(0, lbl.Bounds.Y);

        // user presses down over the control
        var e = new MouseEvent
        {
            X = 1,
            Y = 0,
            Flags = MouseFlags.Button1Pressed,
        };

        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(0, lbl.Bounds.Y);

        // we still haven't committed to anything
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // user pulled view down but still has mouse down
        e = new MouseEvent
        {
            X = 1,
            Y = 1,
            Flags = MouseFlags.Button1Pressed,
        };
        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual((Pos)1, lbl.Y);

        // we still haven't committed to anything
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // user releases mouse (in place)
        e = new MouseEvent
        {
            X = 1,
            Y = 1,
        };
        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual((Pos)1, lbl.Y);

        // we have now committed the drag so could undo
        ClassicAssert.AreEqual(1, OperationManager.Instance.UndoStackSize);
    }

    [TestCase(typeof(Button))]
    [TestCase(typeof(TabView))]
    [TestCase(typeof(TableView))]
    [TestCase(typeof(View))]
    public void TestDragResizeView(Type t)
    {
        var d = this.Get10By10View();

        var view = new ViewFactory().Create(t);
        view.Width = 8;
        view.Height = 1;

        var design = new Design(d.SourceCode, "myView", view);
        view.Data = design;
        d.View.Add(view);

        ClassicAssert.AreEqual(8, view.Bounds.Width);
        var mgr = new MouseManager();

        // we haven't done anything yet
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);
        ClassicAssert.AreEqual(0, view.Bounds.X);
        ClassicAssert.AreEqual(0, view.Bounds.Y);
        ClassicAssert.AreEqual(8, view.Bounds.Width);
        ClassicAssert.AreEqual(1, view.Bounds.Height);

        // user presses down in the lower right of control
        var e = new MouseEvent
        {
            X = 6,
            Y = 0,
            Flags = MouseFlags.Button1Pressed,
        };

        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(0, view.Bounds.Y);

        // we still haven't committed to anything
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // user pulled view size +1 width and +1 height
        e = new MouseEvent
        {
            X = 9,
            Y = 0,
            Flags = MouseFlags.Button1Pressed,
        };
        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(0, view.Bounds.X);
        ClassicAssert.AreEqual(0, view.Bounds.Y);
        ClassicAssert.AreEqual(10, view.Bounds.Width, "Expected resize to increase Width when dragging");
        ClassicAssert.AreEqual(1, view.Bounds.Height, "Expected resize of button to ignore Y component");

        // we still haven't committed to anything
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // user releases mouse (in place)
        e = new MouseEvent
        {
            X = 9,
            Y = 0,
        };
        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(0, view.Bounds.X);
        ClassicAssert.AreEqual(0, view.Bounds.Y);
        ClassicAssert.AreEqual(10, view.Bounds.Width, "Expected resize to increase Width when dragging");
        ClassicAssert.AreEqual(1, view.Bounds.Height);

        // we have now committed the drag so could undo
        ClassicAssert.AreEqual(1, OperationManager.Instance.UndoStackSize);
    }

    [TestCase(typeof(View))]
    public void TestDragResizeView_CannotResize_DimFill(Type t)
    {
        var d = this.Get10By10View();

        var view = new ViewFactory().Create(t);
        view.Width = Dim.Fill();
        view.Height = 1;

        var design = new Design(d.SourceCode, "myView", view);
        view.Data = design;
        d.View.Add(view);

        var mgr = new MouseManager();

        // we haven't done anything yet
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);
        ClassicAssert.AreEqual(0, view.Bounds.X);
        ClassicAssert.AreEqual(0, view.Bounds.Y);
        ClassicAssert.AreEqual(10, view.Bounds.Width);
        ClassicAssert.AreEqual(1, view.Bounds.Height);

        // user presses down in the lower right of control
        var e = new MouseEvent
        {
            X = 9,
            Y = 0,
            Flags = MouseFlags.Button1Pressed,
        };

        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(0, view.Bounds.Y);

        // we still haven't committed to anything
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // user pulled view size down and left
        e = new MouseEvent
        {
            X = 6,
            Y = 3,
            Flags = MouseFlags.Button1Pressed,
        };
        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(0, view.Bounds.X);
        ClassicAssert.AreEqual(0, view.Bounds.Y);
        ClassicAssert.AreEqual(10, view.Bounds.Width, "Expected Width to remain constant because it is Dim.Fill()");
        ClassicAssert.AreEqual(4, view.Bounds.Height, "Expected resize to update Y component");
        ClassicAssert.AreEqual(Dim.Fill(),view.Width);
        ClassicAssert.AreEqual(Dim.Sized(4), view.Height);

        // we still haven't committed to anything
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // user releases mouse (in place)
        e = new MouseEvent
        {
            X = 6,
            Y = 3,
        };
        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(0, view.Bounds.X);
        ClassicAssert.AreEqual(0, view.Bounds.Y);
        ClassicAssert.AreEqual(10, view.Bounds.Width, "Expected Width to remain constant because it is Dim.Fill()");
        ClassicAssert.AreEqual(4, view.Bounds.Height, "Expected resize to update Y component");
        ClassicAssert.AreEqual(Dim.Fill(), view.Width);
        ClassicAssert.AreEqual(Dim.Sized(4), view.Height);

        // we have now committed the drag so could undo
        ClassicAssert.AreEqual(1, OperationManager.Instance.UndoStackSize);

        OperationManager.Instance.Undo();

        // Should reset us to the initial state
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);
        ClassicAssert.AreEqual(0, view.Bounds.X);
        ClassicAssert.AreEqual(0, view.Bounds.Y);
        ClassicAssert.AreEqual(10, view.Bounds.Width);
        ClassicAssert.AreEqual(1, view.Bounds.Height); 
        ClassicAssert.AreEqual(Dim.Fill(), view.Width);
        ClassicAssert.AreEqual(Dim.Sized(1), view.Height);

    }

    [TestCase(0,0)]
    [TestCase(3, 3)]
    public void TestDragResizeView_CannotResize_1By1View(int locationOfViewX, int locationOfViewY)
    {
        var d = this.Get10By10View();

        var view = new ViewFactory().Create(typeof(View));
        view.Width = Dim.Fill();
        view.X = locationOfViewX;
        view.Y = locationOfViewY;
        view.Width = 1;
        view.Height = 1;

        var design = new Design(d.SourceCode, "myView", view);
        view.Data = design;
        d.View.Add(view);
        
        d.View.LayoutSubviews();

        var mgr = new MouseManager();

        // we haven't done anything yet
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);
        ClassicAssert.AreEqual(locationOfViewX, view.Frame.X);
        ClassicAssert.AreEqual(locationOfViewY, view.Frame.Y);
        ClassicAssert.AreEqual(1, view.Bounds.Width);
        ClassicAssert.AreEqual(1, view.Bounds.Height);

        // user presses down in the lower right of control
        var e = new MouseEvent
        {
            X = locationOfViewX,
            Y = locationOfViewY,
            Flags = MouseFlags.Button1Pressed,
        };

        var hit = view.HitTest(e, out var isBorder, out var isLowerRight);
        ClassicAssert.AreSame(view, hit);
        ClassicAssert.IsTrue(isBorder);
        ClassicAssert.IsFalse(isLowerRight,"Upper left should never be considered lower right even if view is 1x1");

        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(locationOfViewY, view.Frame.Y);

        // we still haven't committed to anything
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // user pulled view size down and left
        e = new MouseEvent
        {
            X = 6,
            Y = 3,
            Flags = MouseFlags.Button1Pressed,
        };
        mgr.HandleMouse(e, d);

        ClassicAssert.AreEqual(1, view.Bounds.Width);
        ClassicAssert.AreEqual(1, view.Bounds.Height);
    }

    [TestCase(1, 1, 4, 6, new[] { 0, 2 })] // drag from 1,1 to 4,6 and expect labels 0 and 2 to be selected
    [TestCase(1, 1, 10, 10, new[] { 0, 1, 2 })] // drag over all
    public void TestDragSelectionBox(int xStart, int yStart, int xEnd, int yEnd, int[] expectSelected)
    {
        var d = this.Get10By10View();

        /*
          Hi
            Hi
          Hi
        */

        var lbl1 = new Label(2, 1, "Hi");
        var lbl2 = new Label(4, 2, "Hi");
        var lbl3 = new Label(2, 3, "Hi");

        var lbl1Design = new Design(d.SourceCode, "lbl1", lbl1);
        var lbl2Design = new Design(d.SourceCode, "lbl2", lbl2);
        var lbl3Design = new Design(d.SourceCode, "lbl3", lbl3);

        lbl1.Data = lbl1Design;
        lbl2.Data = lbl2Design;
        lbl3.Data = lbl3Design;

        var labels = new[] { lbl1Design, lbl2Design, lbl3Design };

        d.View.Add(lbl1);
        d.View.Add(lbl2);
        d.View.Add(lbl3);

        var selection = SelectionManager.Instance;
        selection.Clear();
        var mgr = new MouseManager();

        // user presses down
        var e = new MouseEvent
        {
            X = xStart,
            Y = yStart,
            Flags = MouseFlags.Button1Pressed,
        };

        mgr.HandleMouse(e, d);

        // user pulled selection box to destination
        e = new MouseEvent
        {
            X = xEnd,
            Y = yEnd,
            Flags = MouseFlags.Button1Pressed,
        };
        mgr.HandleMouse(e, d);

        // user releases mouse (in place)
        e = new MouseEvent
        {
            X = xEnd,
            Y = yEnd,
        };
        mgr.HandleMouse(e, d);

        // do not expect dragging selection box to change anything
        // or be undoable
        ClassicAssert.AreEqual(0, OperationManager.Instance.UndoStackSize);

        // for each selectable thing
        for (int i = 0; i < labels.Length; i++)
        {
            // its selection status should match the expectation
            // passed in the test case
            ClassicAssert.AreEqual(
                expectSelected.Contains(i),
                selection.Selected.ToList().Contains(labels[i]),
                $"Expectation wrong for label index {i} (indexes are 0 based)");
        }
    }
}
