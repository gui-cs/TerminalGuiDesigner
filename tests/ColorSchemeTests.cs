using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;

namespace tests;

public class ColorSchemeTests : Tests
{
    [Test]
    public void TestHasColorScheme()
    {
        var window = new Window();
        var d = new Design(new SourceCodeFile(new FileInfo("TenByTen.cs")), Design.RootDesignName, window);
        window.Data = d;

        var state = Application.Begin(window);

        Assert.AreSame(Colors.Base,d.View.ColorScheme);
        Assert.IsNotNull(d.View.ColorScheme);
        Assert.IsFalse(d.HasColorScheme());

        d.View.ColorScheme = new ColorScheme();
        Assert.IsTrue(d.HasColorScheme());

        Application.End(state);
    }

    [Test]
    public void TestTrackingColorSchemes()
    {
        var mgr = new ColorSchemeManager();
        var view = new TestClass();

        var d = new Design(new SourceCodeFile(new FileInfo("TestTrackingColorSchemes.cs")), Design.RootDesignName, view);

        Assert.AreEqual(0, mgr.Schemes.Count);
        mgr.FindDeclaredColorSchemes(d);
        Assert.AreEqual(2, mgr.Schemes.Count);

        var found = mgr.GetNameForColorScheme(new ColorScheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black)
        });

        Assert.IsNotNull(found);
        Assert.AreEqual("aaa", found);        
    }

    class TestClass : View
    {
        private ColorScheme aaa = new ColorScheme { 
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black)
        };
        private ColorScheme bbb = new ColorScheme {Normal = new Attribute(Color.Green,Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black)
        };
        private int ccc;
    }
}