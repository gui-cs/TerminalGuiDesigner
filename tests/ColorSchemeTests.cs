using System;
using System.IO;
using System.Linq;
using NLog.Layouts;
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

        var scheme = new ColorScheme();
        var prop = new SetPropertyOperation(d , d.GetDesignableProperty(nameof(View.ColorScheme))
            ?? throw new Exception("Expected Property did not exist or was not designable"),null,scheme);

        prop.Do();

        // we still don't know about this scheme yet
        Assert.IsFalse(d.HasKnownColorScheme());

        ColorSchemeManager.Instance.AddOrUpdateScheme("fff", scheme);

        if (whenMultiSelected)
        {
            SelectionManager.Instance.SetSelection(d);
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

    [TestCase(true)]
    [TestCase(false)]
    public void TestColorSchemeProperty_ToString(bool testMultiSelectingSeveralTimes)
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
        var selection = SelectionManager.Instance;

        if(testMultiSelectingSeveralTimes)
        {
            selection.SetSelection(p.Design);
            selection.Clear();
            selection.SetSelection(p.Design);
            selection.SetSelection(p.Design);
            selection.SetSelection(p.Design);
            selection.Clear();

            Assert.AreEqual(pink,p.Design.View.ColorScheme);
        }

        selection.SetSelection(p.Design);
        Assert.AreNotEqual(pink,p.Design.View.ColorScheme, "Expected view to be selected so be green not pink");
        Assert.AreEqual("ColorScheme:pink", p.ToString(), "Expected us to know it was pink under the hood even while selected");
        selection.Clear();

        Assert.AreEqual(pink,p.Design.View.ColorScheme);

    }
    
    [Test]
    public void TestColorSchemeProperty_ToString_SelectThenSetScheme()
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

        // select it first to make it green
        SelectionManager.Instance.SetSelection(p.Design);

        p.SetValue(pink);
        Assert.AreEqual("ColorScheme:pink", p.ToString());

        SelectionManager.Instance.Clear();
        Assert.AreEqual("ColorScheme:pink", p.ToString(), "Expected clearing selection not to reset an old scheme");

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

        var lblIn = RoundTrip<Dialog, Label>((d, l) =>
        {
            //unselect it so it is rendered with correct scheme
            SelectionManager.Instance.Clear();
            l.ColorScheme = mgr.Schemes.Single().Scheme;

            if (multiSelectBeforeSaving)
            {
                Assert.AreEqual(mgr.Schemes.Single().Scheme, l.ColorScheme);
                SelectionManager.Instance.SetSelection((Design)l.Data);
                Assert.AreNotEqual(mgr.Schemes.Single().Scheme, l.ColorScheme, "Expected multi selecting the view to change its color to the selected color");
            }
        }, out _);

        var lblDesignIn = (Design)lblIn.Data;
        Assert.IsTrue(lblDesignIn.HasKnownColorScheme());

        // clear the selection before we do the comparison
        SelectionManager.Instance.Clear();

        Assert.AreEqual("pink",mgr.GetNameForColorScheme(lblDesignIn.View.ColorScheme));

        mgr.Clear();
    }

    [Test]
    public void TestDefaultColors()
    {
        var d = new DefaultColorSchemes();

        Assert.Contains(d.GreenOnBlack, d.GetDefaultSchemes().ToArray());
        Assert.Contains(d.RedOnBlack, d.GetDefaultSchemes().ToArray());
        Assert.Contains(d.BlueOnBlack, d.GetDefaultSchemes().ToArray());
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