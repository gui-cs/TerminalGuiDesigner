using System.IO;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI;

namespace tests;

public class MouseManagerTests : Tests
{
    private Design Get10By10View()
    {
        // start with blank slate
        OperationManager.Instance.ClearUndoRedo();

        var v = new View(new Rect(0,0,10,10));
        var d = new Design(new SourceCodeFile(new FileInfo("TenByTen.cs")),Design.RootDesignName,v);
        v.Data = d;

        return d;
    }
    [Test]
    public void TestDragLabel()
    {
        var d = Get10By10View();

        var lbl = new Label(0,0,"Hi there buddy");
        var lblDesign = new Design(d.SourceCode,"mylabel",lbl);
        lbl.Data = lblDesign;
        d.View.Add(lbl);

        var selection = new MultiSelectionManager();
        var mgr = new MouseManager(selection);

        // we haven't done anything yet
        Assert.AreEqual(0,OperationManager.Instance.UndoStackSize);
        Assert.AreEqual(0,lbl.Bounds.Y);

        // user presses down over the control
        var e = new MouseEvent{
            X = 1,
            Y = 0,
            Flags = MouseFlags.Button1Pressed
        };

        mgr.HandleMouse(e,d);

        Assert.AreEqual(0,lbl.Bounds.Y);

        // we still haven't committed to anything
        Assert.AreEqual(0,OperationManager.Instance.UndoStackSize);

        // user pulled view down but still has mouse down
        e = new MouseEvent{
            X = 1,
            Y = 1,
            Flags = MouseFlags.Button1Pressed
        };
        mgr.HandleMouse(e,d);

        Assert.AreEqual((Pos)1,lbl.Y);

        // we still haven't committed to anything
        Assert.AreEqual(0,OperationManager.Instance.UndoStackSize);

        // user releases mouse (in place)
        e = new MouseEvent{
            X = 1,
            Y = 1,
        };
        mgr.HandleMouse(e,d);

        Assert.AreEqual((Pos)1,lbl.Y);

        // we have now committed the drag so could undo
        Assert.AreEqual(1,OperationManager.Instance.UndoStackSize);
    }
}
