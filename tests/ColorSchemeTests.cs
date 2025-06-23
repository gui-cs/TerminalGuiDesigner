using System.IO;
using System.Linq;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(SchemeManager))]
[Category("Core")]
internal class SchemeTests : Tests
{
    [Test]
    public void RenameScheme( )
    {
        var window = new Window( );
        var d = new Design( new SourceCodeFile( new FileInfo( "TenByTen.cs" ) ), Design.RootDesignName, window );
        window.Data = d;

        var state = Application.Begin( window );

        Assume.That( d.View.Scheme, Is.Not.Null.And.SameAs( Colors.Schemes["Base"] ) );
        Assume.That( d.HasKnownScheme( ), Is.False );

        var scheme = new Scheme( );
        var prop = new SetPropertyOperation( d, d.GetDesignableProperty( nameof( View.Scheme ) )
                                                ?? throw new Exception( "Expected Property did not exist or was not designable" ), null, scheme );

        prop.Do( );

        // we still don't know about this scheme yet
        Assume.That( d.HasKnownScheme( ), Is.False );

        const string oldName = "fff";
        SchemeManager.Instance.AddOrUpdateScheme( oldName, scheme, d );
        var originalNamedScheme = SchemeManager.Instance.GetNamedScheme( oldName );
        Assume.That( d.HasKnownScheme );

        // Now rename it and verify

        const string newName = "FancyNewName";
        SchemeManager.Instance.RenameScheme( oldName, newName );
        Assert.That( d.HasKnownScheme );
        NamedScheme renamedScheme = SchemeManager.Instance.GetNamedScheme( newName );
        Assert.That( renamedScheme, Is.SameAs( originalNamedScheme ) );
        Assert.That( renamedScheme.Name, Is.EqualTo( newName ) );
    }

    [Test]
    public void HasScheme([Values]bool whenMultiSelected)
    {
        var window = new Window();
        var d = new Design(new SourceCodeFile(new FileInfo("TenByTen.cs")), Design.RootDesignName, window);
        window.Data = d;

        var state = Application.Begin(window);

        Assert.That( d.View.Scheme, Is.Not.Null.And.SameAs( Colors.Schemes["Base"] ) );
        Assert.That( d.HasKnownScheme(), Is.False );

        var scheme = new Scheme();
        var prop = new SetPropertyOperation(d, d.GetDesignableProperty(nameof(View.Scheme))
            ?? throw new Exception("Expected Property did not exist or was not designable"), null, scheme);

        prop.Do();

        // we still don't know about this scheme yet
        Assert.That( d.HasKnownScheme(), Is.False );

        SchemeManager.Instance.AddOrUpdateScheme("fff", scheme, d);

        if (whenMultiSelected)
        {
            SelectionManager.Instance.SetSelection(d);
        }

        // now we know about it
        Assert.That( d.HasKnownScheme() );

        SchemeManager.Instance.Clear();

        Application.End(state);
    }

    [Test]
    public void TestTrackingSchemes()
    {
        var mgr = SchemeManager.Instance;
        mgr.Clear();

        var view = new TestClass();

        var d = new Design(new SourceCodeFile(new FileInfo("TestTrackingSchemes.cs")), Design.RootDesignName, view);

        Assume.That( mgr.Schemes, Is.Empty );
        mgr.FindDeclaredSchemes(d);
        Assert.That( mgr.Schemes, Has.Count.EqualTo( 2 ) );

        var found = mgr.GetNameForScheme(new Scheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        });

        Assert.That( found, Is.Not.Null );
        Assert.That( found, Is.EqualTo( "aaa" ) );
        mgr.Clear();
    }

    [Test]
    public void TestSchemeProperty_ToString([Values]bool testMultiSelectingSeveralTimes)
    {
        // default when creating a new view is to have no explicit
        // Scheme defined and just inherit from parent
        var v = Get10By10View();

        var btn = new Button{ Text = "Hey" };
        var op = new AddViewOperation(btn, v, "myBtn");
        op.Do();
        Design btnDesign = (Design)btn.Data;

        var p = (SchemeProperty?)(btnDesign.GetDesignableProperty(nameof(View.Scheme)));

        Assert.That( p, Is.Not.Null );
        Assert.That( p!.ToString(), Is.EqualTo( "Scheme:(Inherited)" ) );

        // Define a new color scheme
        var mgr = SchemeManager.Instance;
        mgr.Clear();

        var pink = new Scheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };

        mgr.AddOrUpdateScheme("pink", pink, btnDesign);

        p.SetValue(pink);
        Assert.That( p.ToString(), Is.EqualTo( "Scheme:pink" ) );

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

            Assert.That( p.Design.View.Scheme, Is.EqualTo( pink ) );
        }

        selection.SetSelection(p.Design);
        Assert.Multiple( ( ) =>
        {
            Assert.That( p.Design.View.Scheme, Is.Not.EqualTo( pink ), "Expected view to be selected to be green, not pink");
            Assert.That( p.ToString(), Is.EqualTo( "Scheme:pink" ), "Expected us to know it was pink under the hood even while selected");
        } );
        selection.Clear();

        Assert.That( p.Design.View.Scheme, Is.EqualTo( pink ) );
    }

    [Test]
    public void TestSchemeProperty_ToString_SelectThenSetScheme()
    {
        // default when creating a new view is to have no explicit
        // Scheme defined and just inherit from parent
        var v = Get10By10View();
        var p = (SchemeProperty?)v.GetDesignableProperty(nameof(View.Scheme));

        Assume.That( p, Is.Not.Null );
        Assert.That( p!.ToString(), Is.EqualTo( "Scheme:(Inherited)" ) );

        // Define a new color scheme
        var mgr = SchemeManager.Instance;
        mgr.Clear();

        var pink = new Scheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };

        mgr.AddOrUpdateScheme("pink", pink, v);

        // select it first to make it green
        SelectionManager.Instance.SetSelection(p.Design);

        p.SetValue(pink);
        Assert.That( p.ToString(), Is.EqualTo( "Scheme:pink" ) );

        SelectionManager.Instance.Clear();
        Assert.That( p.ToString(), Is.EqualTo( "Scheme:pink" ), "Expected clearing selection not to reset an old scheme");
    }

    /// <summary>
    /// <para>
    /// Tests that setting a <see cref="Scheme"/> on a view saving and reloading
    /// the .Designer.cs file results in a loaded View with the same Scheme as when
    /// saving.
    /// </para>
    /// <para>Multi select changes Scheme to a selection color, so we also want to test
    /// that that doesn't interfere with things</para>
    /// </summary>
    /// <param name="multiSelectBeforeSaving"></param>
    [Test]
    [Category( "Code Generation" )]
    public void TestScheme_RoundTrip([Values]bool multiSelectBeforeSaving)
    {
        var mgr = SchemeManager.Instance;

        var lblIn = RoundTrip<Dialog, Label>(
            (d, l) =>
        {
            mgr.Clear();
            mgr.AddOrUpdateScheme("pink", new Scheme
            {
                Normal = new Attribute(Color.Magenta, Color.Black),
                Focus = new Attribute(Color.Cyan, Color.Black),
            }, d.GetRootDesign());

            // unselect it so it is rendered with correct scheme
            SelectionManager.Instance.Clear();
            l.Scheme = d.State.OriginalScheme = mgr.Schemes.Single().Scheme;

            if (multiSelectBeforeSaving)
            {
                Assert.That(l.Scheme, Is.EqualTo( mgr.Schemes.Single().Scheme ) );
                SelectionManager.Instance.SetSelection((Design)l.Data);
                Assert.That(l.Scheme, Is.Not.EqualTo( mgr.Schemes.Single().Scheme ), "Expected multi selecting the view to change its color to the selected color");
            }
        }, out _);

        var lblDesignIn = (Design)lblIn.Data;
        Assert.That( lblDesignIn.HasKnownScheme() );

        // clear the selection before we do the comparison
        SelectionManager.Instance.Clear();

        Assert.That( mgr.GetNameForScheme(lblDesignIn.View.Scheme), Is.EqualTo( "pink" ) );

        mgr.Clear();
    }

    [Test]
    public void TestDefaultColors()
    {
        var defaultSchemes = new DefaultSchemes();
        var Schemes = defaultSchemes.GetDefaultSchemes().ToArray();
        Assert.Multiple( ( ) =>
        {
            Assert.That( Schemes, Does.Contain( defaultSchemes.GreenOnBlack ) );
            Assert.That( Schemes, Does.Contain( defaultSchemes.RedOnBlack ) );
            Assert.That( Schemes, Does.Contain( defaultSchemes.BlueOnBlack ) );
        } );
    }

    [Test]
    [Category( "Code Generation" )]
    public void TestEditingSchemeAfterLoad([Values]bool withSelection)
    {
        var scheme = new Scheme();

        const string expectedSchemeName = "yarg";
        var lblIn = RoundTrip<Dialog, Label>(
            (d, _) =>
            {
                // Clear known default colors
                SchemeManager.Instance.Clear();
                Assert.That(SchemeManager.Instance.Schemes, Is.Empty );

                // Add a new color for our Label
                SchemeManager.Instance.AddOrUpdateScheme(expectedSchemeName, scheme, d.GetRootDesign());
                Assert.That(SchemeManager.Instance.Schemes, Has.Count.EqualTo( 1 ) );

                // Assign the new color to the view
                var prop = new SetPropertyOperation(d, new SchemeProperty(d), null, scheme);
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

        SchemeManager.Instance.Clear();
        SchemeManager.Instance.FindDeclaredSchemes(lblInDesign!.GetRootDesign());
        Assert.That( SchemeManager.Instance.Schemes.Count, Is.EqualTo( 1 ), "Reloading the view should find the explicitly declared scheme 'yarg'");

        var schemeBeforeUpdate = withSelection ? lblInDesign.State.OriginalScheme : lblIn.GetExplicitScheme();
        Assert.That( schemeBeforeUpdate, Is.Not.Null, "Expected lblIn to have an explicit Scheme");
        var schemeBeforeUpdateName = SchemeManager.Instance.GetNameForScheme( schemeBeforeUpdate! );
        Assert.That( schemeBeforeUpdateName, Is.Not.Null, "Expected lblIn to have an explicit Scheme");
        
        Assert.That( schemeBeforeUpdateName,
                     Is.EqualTo( expectedSchemeName ),
                     "Expected designer to know the name of the labels color scheme" );

        // make a change to the yarg scheme (e.g. if user opened the color designer and made some changes)
        SchemeManager.Instance.AddOrUpdateScheme(expectedSchemeName, new Scheme { Normal = new Attribute(Color.Cyan, Color.BrightBlue) }, lblInDesign.GetRootDesign());

        var schemeAfterUpdate = withSelection ? lblInDesign.State.OriginalScheme : lblIn.GetExplicitScheme();
        Assert.That( schemeAfterUpdate, Is.Not.Null, "Expected lblIn to have an explicit Scheme" );
        var schemeAfterUpdateName = SchemeManager.Instance.GetNameForScheme( schemeAfterUpdate! );
        Assert.That( schemeAfterUpdateName, Is.Not.Null, "Expected lblIn to have an explicit Scheme");

        Assert.That( schemeAfterUpdateName,
                     Is.EqualTo( expectedSchemeName ),
                     "Expected designer to still know the name of lblIn Scheme" );

        Assert.Multiple( ( ) =>
        {
            Assert.That( lblIn.Scheme.Normal.Foreground, Is.EqualTo( new Color(Color.Cyan) ), "Expected Label to be updated with the new color after being changed in designer");
            Assert.That( lblInDesign.State.OriginalScheme?.Normal.Foreground, Is.EqualTo( new Color(Color.Cyan) ), "Expected Label Design to also be updated with the new color");
        } );
    }

    private class TestClass : View
    {
        private Scheme aaa = new Scheme
        {
            Normal = new Attribute(Color.Magenta, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };

        private Scheme bbb = new Scheme
        {
            Normal = new Attribute(Color.Green, Color.Black),
            Focus = new Attribute(Color.Cyan, Color.Black),
        };
    }
}