using System.IO;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.ToCode;

namespace tests;

public class DesignMenuTests
{
    [Test]
    public void TestCreatingAMenu()
    {
        var viewToCode = new ViewToCode();
        var d = viewToCode.GenerateNewView(new FileInfo("TestCreatingAMenu.cs"),"Yarg",typeof(Toplevel), out _);

        // Start with the menu, view and statusbar
        Assert.AreEqual(3,d.View.GetActualSubviews().Count);
    }
}