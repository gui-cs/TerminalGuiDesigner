﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI;

namespace UnitTests;

internal class Tests
{
    static bool init = false;

    [SetUp]
    public virtual void SetUp()
    {
        if (init)
        {
            throw new InvalidOperationException("After did not run.");
        }

        Application.Init(new FakeDriver(), new FakeMainLoop());
        init = true;

        OperationManager.Instance.ClearUndoRedo();
    }

    [TearDown]
    public virtual void TearDown()
    {
        Application.Shutdown();
        init = false;

        SelectionManager.Instance.LockSelection = false;
        SelectionManager.Instance.Clear();
        ColorSchemeManager.Instance.Clear();
    }

    protected Design Get10By10View()
    {
        // start with blank slate
        OperationManager.Instance.ClearUndoRedo();

        var v = new View(new Rect(0, 0, 10, 10));
        var d = new Design(new SourceCodeFile(new FileInfo("TenByTen.cs")), Design.RootDesignName, v);
        v.Data = d;

        return d;
    }

    protected Design Get100By100<T>([CallerMemberName] string? caller = null)
    {
        // start with blank slate
        OperationManager.Instance.ClearUndoRedo();

        var viewToCode = new ViewToCode();

        var file = new FileInfo($"{caller}.cs");
        var rootDesign = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));
        rootDesign.View.X = 100;
        rootDesign.View.Y = 100;
        
        return rootDesign;
    }
    /// <summary>
    /// Creates a new instance of <typeparamref name="T2"/> using <see cref="ViewFactory"/>.  Then calls the
    /// provided <paramref name="adjust"/> action before writting out and reading back the code.  Returns
    /// the read back in instance of your <typeparamref name="T2"/> so you can compare that it matches expectations
    /// (i.e. nothing was lost during serialization/deserialization).
    /// </summary>
    /// <typeparam name="T1">Root designer View type to create (e.g. <see cref="Window"/>)</typeparam>
    /// <typeparam name="T2">Type of subview to create (e.g. <see cref="Label"/>)</typeparam>
    /// <param name="adjust">Mutator for making pre save changes you want to conform can be read in properly</param>
    /// <param name="viewOut">The view created and passed to <paramref name="adjust"/></param>
    /// <param name="caller"></param>
    /// <returns>The read in object state after round trip (generate code file then read that code back in)</returns>
    protected T2 RoundTrip<T1, T2>(Action<Design, T2> adjust, out T2 viewOut, [CallerMemberName] string? caller = null)
        where T1 : View
        where T2 : View
    {
        const string fieldName = "myViewOut";

        var viewToCode = new ViewToCode();

        var file = new FileInfo(caller + ".cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(T1));

        var factory = new ViewFactory();
        viewOut = (T2)factory.Create(typeof(T2));

        OperationManager.Instance.Do(new AddViewOperation(viewOut, designOut, fieldName));
        adjust((Design)viewOut.Data, viewOut);

        viewToCode.GenerateDesignerCs(designOut, typeof(T1));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        return designBackIn.View.GetActualSubviews().OfType<T2>().Where(v => v.Data is Design d && d.FieldName.Equals(fieldName)).Single();
    }
    /// <summary>
    /// Performs a mouse drag from the first coordinates to the second (in screen space)
    /// </summary>
    /// <param name="root">The root Design.  Make sure you have added it to <see cref="Application.Top"/> and run <see cref="View.LayoutSubviews"/></param>
    /// <param name="x1">X coordinate to start drag at</param>
    /// <param name="y1">Y coordinate to start drag at</param>
    /// <param name="x2">X coordinate to end drag at</param>
    /// <param name="y2">Y coordinate to end drag at</param>
    protected void MouseDrag(Design root, int x1, int y1, int x2, int y2)
    {
        var mm = new MouseManager();

        mm.HandleMouse(
            new MouseEvent
            {
                X = x1,
                Y = y1,
                Flags = MouseFlags.Button1Pressed,
            }, root);

        // press down at 0,0 of the label
        mm.HandleMouse(
            new MouseEvent
            {
                X = x2,
                Y = y2,
                Flags = MouseFlags.Button1Pressed,
            }, root);


        // release in parent
        mm.HandleMouse(
            new MouseEvent
            {
                X = x2,
                Y = y2,
                Flags = MouseFlags.Button1Released,
            }, root);
    }

    [Test]
    public void TesatCastTo()
    {
        var str = "cat";
        object obj = str.CastToReflected(typeof(object));
        ClassicAssert.IsInstanceOf<object>(obj);
    }
}