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
        Assert.IsFalse(d.HasKnownColorScheme());

        d.View.ColorScheme = new ColorScheme();
        
        // we still don't know about this scheme yet
        Assert.IsFalse(d.HasKnownColorScheme());

        ColorSchemeManager.Instance.AddOrUpdateScheme("fff",d.View.ColorScheme);
        // now we know about it
        Assert.IsTrue(d.HasKnownColorScheme());

        ColorSchemeManager.Instance.Clear();

        Application.End(state);
    }

    [Test]
    public void TestTrackingColorSchemes()
    {
        var mgr = ColorSchemeManager.Instance;
        mgr.Clear();

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
        mgr.Clear();
    }

    

    [Test]
    public void TestColorScheme_RoundTrip()
    {

        var mgr = ColorSchemeManager.Instance;
        mgr.Clear();

        mgr.AddOrUpdateScheme("pink",new ColorScheme { 
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black)
        });

        var viewToCode = new ViewToCode();

        var dOut = viewToCode.GenerateNewView(new FileInfo("TestColorScheme_RoundTrip.cs"),"Example",typeof(Dialog),out _);

        // create label with custom color scheme
        var lblOut = new Label{
            ColorScheme = mgr.Schemes.Single().Value
        };

        // add it to view
        OperationManager.Instance.Do(new AddViewOperation(dOut.SourceCode, lblOut,dOut, "mylbl"));

        viewToCode.GenerateDesignerCs(dOut,dOut.SourceCode,typeof(Dialog));

        var codeToView = new CodeToView(dOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblDesignIn = designBackIn.GetAllDesigns().Single(d=>d.View is Label);

        Assert.IsTrue(lblDesignIn.HasKnownColorScheme());

        Assert.AreEqual("pink",mgr.GetNameForColorScheme(lblDesignIn.View.ColorScheme));


        mgr.Clear();
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