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
    [TestCase(true)]
    [TestCase(false)]
    public void TestHasColorScheme(bool whenMultiSelected)
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

        if (whenMultiSelected)
        {
            MultiSelectionManager.Instance.SetSelection(d);
        }

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
    public void TestColorSchemeProperty_ToString()
    {
        // default when creating a new view is to have no explicit 
        // ColorScheme defined and just inherit from parent
        var v = Get10By10View();
        var p = (ColorSchemeProperty)(v.GetDesignableProperty(nameof(View.ColorScheme))?? throw new Exception("Expected this not to be null"));

        Assert.AreEqual("ColorScheme:(Inherited)", p.ToString());

        // Define a new color scheme
        var mgr = ColorSchemeManager.Instance;
        mgr.Clear();

        var pink = new ColorScheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black)
        };

        mgr.AddOrUpdateScheme("pink", pink);

        p.SetValue(pink);
        Assert.AreEqual("ColorScheme:pink", p.ToString());

        // when multiselecting (with a selection box) a bunch of views
        // all the views turn to green.  But we shouldn't loose track
        // of the actual color scheme the user set
        var selection = MultiSelectionManager.Instance;
        selection.SetSelection(p.Design);

        Assert.AreEqual("ColorScheme:pink", p.ToString());
        selection.Clear();

    }


    /// <summary>
    /// <para>
    /// Tests that setting a <see cref="ColorScheme"/> on a view saving and reloading
    /// the .Designer.cs file results in a loaded View with the same ColorScheme as when
    /// saving.
    /// </para>
    /// <para>Multi select changes ColorScheme to a selection color so we also want to test
    /// that that doesn't interfere with things</para>
    /// </summary>
    /// <param name="multiSelectBeforeSaving"></param>
    [TestCase(true)]
    [TestCase(false)]
    public void TestColorScheme_RoundTrip(bool multiSelectBeforeSaving)
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
            ColorScheme = mgr.Schemes.Single().Scheme
        };

        // add it to view
        OperationManager.Instance.Do(new AddViewOperation(dOut.SourceCode, lblOut,dOut, "mylbl"));

        if (multiSelectBeforeSaving)
        {            
            Assert.AreEqual(mgr.Schemes.Single().Scheme, lblOut.ColorScheme);
            MultiSelectionManager.Instance.SetSelection((Design)lblOut.Data);
            Assert.AreNotEqual(mgr.Schemes.Single().Scheme, lblOut.ColorScheme,"Expected multi selecting the view to change its color to the selected color");
        }

        viewToCode.GenerateDesignerCs(dOut,dOut.SourceCode,typeof(Dialog));

        var codeToView = new CodeToView(dOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblDesignIn = designBackIn.GetAllDesigns().Single(d=>d.View is Label);

        Assert.IsTrue(lblDesignIn.HasKnownColorScheme());

        // clear the selection before we do the comparison
        MultiSelectionManager.Instance.Clear();

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