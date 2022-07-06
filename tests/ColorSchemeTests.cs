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
}