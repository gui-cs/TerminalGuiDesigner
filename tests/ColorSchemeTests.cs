using System.IO;
using System.Linq;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(ColorSchemeManager))]
[Category("Core")]
internal class ColorSchemeTests : Tests
{
    [Test]
    public void RenameScheme( )
    {
        var window = new Window( );
        var d = new Design( new SourceCodeFile( new FileInfo( "TenByTen.cs" ) ), Design.RootDesignName, window );
        window.Data = d;

        var state = Application.Begin( window );

        Assume.That( d.View.ColorScheme, Is.Not.Null.And.SameAs( Colors.ColorSchemes["Base"] ) );
        Assume.That( d.HasKnownColorScheme( ), Is.False );

        var scheme = new ColorScheme( );
        var prop = new SetPropertyOperation( d, d.GetDesignableProperty( nameof( View.ColorScheme ) )
                                                ?? throw new Exception( "Expected Property did not exist or was not designable" ), null, scheme );

        prop.Do( );

        // we still don't know about this scheme yet
        Assume.That( d.HasKnownColorScheme( ), Is.False );

        const string oldName = "fff";
        ColorSchemeManager.Instance.AddOrUpdateScheme( oldName, scheme, d );
        var originalNamedColorScheme = ColorSchemeManager.Instance.GetNamedColorScheme( oldName );
        Assume.That( d.HasKnownColorScheme );

        // Now rename it and verify

        const string newName = "FancyNewName";
        ColorSchemeManager.Instance.RenameScheme( oldName, newName );
        Assert.That( d.HasKnownColorScheme );
        NamedColorScheme renamedColorScheme = ColorSchemeManager.Instance.GetNamedColorScheme( newName );
        Assert.That( renamedColorScheme, Is.SameAs( originalNamedColorScheme ) );
        Assert.That( renamedColorScheme.Name, Is.EqualTo( newName ) );
    }

    [Test]
    public void HasColorScheme([Values]bool whenMultiSelected)
    {
        var window = new Window();
        var d = new Design(new SourceCodeFile(new FileInfo("TenByTen.cs")), Design.RootDesignName, window);
        window.Data = d;

        var state = Application.Begin(window);

        Assert.That( d.View.ColorScheme, Is.Not.Null.And.SameAs( Colors.ColorSchemes["Base"] ) );
        Assert.That( d.HasKnownColorScheme(), Is.False );

        var scheme = new ColorScheme();
        var prop = new SetPropertyOperation(d, d.GetDesignableProperty(nameof(View.ColorScheme))
            ?? throw new Exception("Expected Property did not exist or was not designable"), null, scheme);

        prop.Do();

        // we still don't know about this scheme yet
        Assert.That( d.HasKnownColorScheme(), Is.False );

        ColorSchemeManager.Instance.AddOrUpdateScheme("fff", scheme, d);

        if (whenMultiSelected)
        {
            SelectionManager.Instance.SetSelection(d);
        }

        // now we know about it
        Assert.That( d.HasKnownColorScheme() );

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

        Assume.That( mgr.Schemes, Is.Empty );
        mgr.FindDeclaredColorSchemes(d);
        Assert.That( mgr.Schemes, Has.Count.EqualTo( 2 ) );

        var found = mgr.GetNameForColorScheme(new ColorScheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        });

        Assert.That( found, Is.Not.Null );
        Assert.That( found, Is.EqualTo( "aaa" ) );
        mgr.Clear();
    }

    [Test]
    public void TestColorSchemeProperty_ToString([Values]bool testMultiSelectingSeveralTimes)
    {
        // default when creating a new view is to have no explicit
        // ColorScheme defined and just inherit from parent
        var v = Get10By10View();

        var btn = new Button{ Text = "Hey" };
        var op = new AddViewOperation(btn, v, "myBtn");
        op.Do();
        Design btnDesign = (Design)btn.Data;

        var p = (ColorSchemeProperty?)(btnDesign.GetDesignableProperty(nameof(View.ColorScheme)));

        Assert.That( p, Is.Not.Null );
        Assert.That( p!.ToString(), Is.EqualTo( "ColorScheme:(Inherited)" ) );

        // Define a new color scheme
        var mgr = ColorSchemeManager.Instance;
        mgr.Clear();

        var pink = new ColorScheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };

        mgr.AddOrUpdateScheme("pink", pink, btnDesign);

        p.SetValue(pink);
        Assert.That( p.ToString(), Is.EqualTo( "ColorScheme:pink" ) );

        // when multi-selecting (with a selection box) a bunch of views
        // all the views turn to green.  But we shouldn't lose track
        // of the actual color scheme the user set
        var selection = SelectionManager.Instance;

        if (testMultiSelectingSeveralTimes)
        {
            selection.SetSelection(p.Design);
            selection.Clear();
            selection.SetSelection(p.Design);
            selection.SetSelection(p.Design);
            selection.SetSelection(p.Design);
            selection.Clear();

            Assert.That( p.Design.View.ColorScheme, Is.EqualTo( pink ) );
        }

        selection.SetSelection(p.Design);
        Assert.Multiple( ( ) =>
        {
            Assert.That( p.Design.View.ColorScheme, Is.Not.EqualTo( pink ), "Expected view to be selected to be green, not pink");
            Assert.That( p.ToString(), Is.EqualTo( "ColorScheme:pink" ), "Expected us to know it was pink under the hood even while selected");
        } );
        selection.Clear();

        Assert.That( p.Design.View.ColorScheme, Is.EqualTo( pink ) );
    }

    [Test]
    public void TestColorSchemeProperty_ToString_SelectThenSetScheme()
    {
        // default when creating a new view is to have no explicit
        // ColorScheme defined and just inherit from parent
        var v = Get10By10View();
        var p = (ColorSchemeProperty?)v.GetDesignableProperty(nameof(View.ColorScheme));

        Assume.That( p, Is.Not.Null );
        Assert.That( p!.ToString(), Is.EqualTo( "ColorScheme:(Inherited)" ) );

        // Define a new color scheme
        var mgr = ColorSchemeManager.Instance;
        mgr.Clear();

        var pink = new ColorScheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };

        mgr.AddOrUpdateScheme("pink", pink, v);

        // select it first to make it green
        SelectionManager.Instance.SetSelection(p.Design);

        p.SetValue(pink);
        Assert.That( p.ToString(), Is.EqualTo( "ColorScheme:pink" ) );

        SelectionManager.Instance.Clear();
        Assert.That( p.ToString(), Is.EqualTo( "ColorScheme:pink" ), "Expected clearing selection not to reset an old scheme");
    }

    /// <summary>
    /// <para>
    /// Tests that setting a <see cref="ColorScheme"/> on a view saving and reloading
    /// the .Designer.cs file results in a loaded View with the same ColorScheme as when
    /// saving.
    /// </para>
    /// <para>Multi select changes ColorScheme to a selection color, so we also want to test
    /// that that doesn't interfere with things</para>
    /// </summary>
    /// <param name="multiSelectBeforeSaving"></param>
    [Test]
    [Category( "Code Generation" )]
    public void TestColorScheme_RoundTrip([Values]bool multiSelectBeforeSaving)
    {
        var mgr = ColorSchemeManager.Instance;

        var lblIn = RoundTrip<Dialog, Label>(
            (d, l) =>
        {
            mgr.Clear();
            mgr.AddOrUpdateScheme("pink", new ColorScheme
            {
                Normal = new Attribute(Color.Magenta, Color.Black),
                Focus = new Attribute(Color.Cyan, Color.Black),
            }, d.GetRootDesign());

            // unselect it so it is rendered with correct scheme
            SelectionManager.Instance.Clear();
            l.ColorScheme = d.State.OriginalScheme = mgr.Schemes.Single().Scheme;

            if (multiSelectBeforeSaving)
            {
                Assert.That(l.ColorScheme, Is.EqualTo( mgr.Schemes.Single().Scheme ) );
                SelectionManager.Instance.SetSelection((Design)l.Data);
                Assert.That(l.ColorScheme, Is.Not.EqualTo( mgr.Schemes.Single().Scheme ), "Expected multi selecting the view to change its color to the selected color");
            }
        }, out _);

        var lblDesignIn = (Design)lblIn.Data;
        Assert.That( lblDesignIn.HasKnownColorScheme() );

        // clear the selection before we do the comparison
        SelectionManager.Instance.Clear();

        Assert.That( mgr.GetNameForColorScheme(lblDesignIn.View.ColorScheme), Is.EqualTo( "pink" ) );

        mgr.Clear();
    }

    [Test]
    public void TestDefaultColors()
    {
        var defaultColorSchemes = new DefaultColorSchemes();
        var colorSchemes = defaultColorSchemes.GetDefaultSchemes().ToArray();
        Assert.Multiple( ( ) =>
        {
            Assert.That( colorSchemes, Does.Contain( defaultColorSchemes.GreenOnBlack ) );
            Assert.That( colorSchemes, Does.Contain( defaultColorSchemes.RedOnBlack ) );
            Assert.That( colorSchemes, Does.Contain( defaultColorSchemes.BlueOnBlack ) );
        } );
    }

    [Test]
    [Category( "Code Generation" )]
    public void TestEditingSchemeAfterLoad([Values]bool withSelection)
    {
        var scheme = new ColorScheme();

        const string expectedSchemeName = "yarg";
        var lblIn = RoundTrip<Dialog, Label>(
            (d, _) =>
            {
                // Clear known default colors
                ColorSchemeManager.Instance.Clear();
                Assert.That(ColorSchemeManager.Instance.Schemes, Is.Empty );

                // Add a new color for our Label
                ColorSchemeManager.Instance.AddOrUpdateScheme(expectedSchemeName, scheme, d.GetRootDesign());
                Assert.That(ColorSchemeManager.Instance.Schemes, Has.Count.EqualTo( 1 ) );

                // Assign the new color to the view
                var prop = new SetPropertyOperation(d, new ColorSchemeProperty(d), null, scheme);
                prop.Do();

                if (withSelection)
                {
                    SelectionManager.Instance.ForceSetSelection(d);
                }
            }, out _);

        var lblInDesign = lblIn.Data as Design;
        Assert.That(lblInDesign, Is.Not.Null.And.TypeOf<Design>( ), "Expected Design to exist on the label read in" );

        if (withSelection)
        {
            SelectionManager.Instance.ForceSetSelection(lblInDesign!);
        }

        ColorSchemeManager.Instance.Clear();
        ColorSchemeManager.Instance.FindDeclaredColorSchemes(lblInDesign!.GetRootDesign());
        Assert.That( ColorSchemeManager.Instance.Schemes.Count, Is.EqualTo( 1 ), "Reloading the view should find the explicitly declared scheme 'yarg'");

        var schemeBeforeUpdate = withSelection ? lblInDesign.State.OriginalScheme : lblIn.GetExplicitColorScheme();
        Assert.That( schemeBeforeUpdate, Is.Not.Null, "Expected lblIn to have an explicit ColorScheme");
        var schemeBeforeUpdateName = ColorSchemeManager.Instance.GetNameForColorScheme( schemeBeforeUpdate! );
        Assert.That( schemeBeforeUpdateName, Is.Not.Null, "Expected lblIn to have an explicit ColorScheme");
        
        Assert.That( schemeBeforeUpdateName,
                     Is.EqualTo( expectedSchemeName ),
                     "Expected designer to know the name of the labels color scheme" );

        // make a change to the yarg scheme (e.g. if user opened the color designer and made some changes)
        ColorSchemeManager.Instance.AddOrUpdateScheme(expectedSchemeName, new ColorScheme { Normal = new Attribute(Color.Cyan, Color.BrightBlue) }, lblInDesign.GetRootDesign());

        var schemeAfterUpdate = withSelection ? lblInDesign.State.OriginalScheme : lblIn.GetExplicitColorScheme();
        Assert.That( schemeAfterUpdate, Is.Not.Null, "Expected lblIn to have an explicit ColorScheme" );
        var schemeAfterUpdateName = ColorSchemeManager.Instance.GetNameForColorScheme( schemeAfterUpdate! );
        Assert.That( schemeAfterUpdateName, Is.Not.Null, "Expected lblIn to have an explicit ColorScheme");

        Assert.That( schemeAfterUpdateName,
                     Is.EqualTo( expectedSchemeName ),
                     "Expected designer to still know the name of lblIn ColorScheme" );

        Assert.Multiple( ( ) =>
        {
            Assert.That( lblIn.ColorScheme.Normal.Foreground, Is.EqualTo( new Color(Color.Cyan) ), "Expected Label to be updated with the new color after being changed in designer");
            Assert.That( lblInDesign.State.OriginalScheme?.Normal.Foreground, Is.EqualTo( new Color(Color.Cyan) ), "Expected Label Design to also be updated with the new color");
        } );
    }

    private class TestClass : View
    {
        private ColorScheme aaa = new ColorScheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };

        private ColorScheme bbb = new ColorScheme
        {
            Normal = new Attribute(Color.Green, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };
    }
}