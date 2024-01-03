using System.CodeDom;
using System.Text;
using TerminalGuiAttribute = Terminal.Gui.Attribute;
using TerminalGuiConfigurationManager = Terminal.Gui.ConfigurationManager;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( Property ) )]
[Category( "Core" )]
internal class PropertyTests : Tests
{
    [Test]
    public void Changing_LineViewOrientation( )
    {
        Design v = Get10By10View( );
        using LineView lv = ViewFactory.Create<LineView>( );
        Design d = new( v.SourceCode, "lv", lv );

        v.View.Add( lv );
        lv.IsInitialized = true;

        Assert.Multiple( ( ) =>
        {
            Assert.That( lv.Orientation, Is.EqualTo( Orientation.Horizontal ) );
            Assert.That( lv.LineRune, Is.EqualTo( new Rune( '─' ) ) );
        } );

        Property? prop = d.GetDesignableProperty( nameof( LineView.Orientation ) );

        Assert.That( prop, Is.Not.Null );
        prop?.SetValue( Orientation.Vertical );
        Assert.That( lv.LineRune, Is.EqualTo( TerminalGuiConfigurationManager.Glyphs.VLine ) );

        // now try with a dim fill
        lv.Height = Dim.Fill( );
        lv.Width = 1;

        prop?.SetValue( Orientation.Horizontal );

        Assert.Multiple( ( ) =>
        {
            Assert.That( lv.Orientation, Is.EqualTo( Orientation.Horizontal ) );
            Assert.That( lv.LineRune, Is.EqualTo( TerminalGuiConfigurationManager.Glyphs.HLine ) );
            Assert.That( lv.Width, Is.EqualTo( Dim.Fill( ) ) );
            Assert.That( lv.Height, Is.EqualTo( Dim.Sized( 1 ) ) );
        } );
    }

    [Test]
    public void PropertyOfType_Attribute( )
    {
        using GraphView graphView = new ( );
        Design d = new( new( $"{nameof( PropertyOfType_Attribute )}.cs" ), "FFF", graphView );
        Property colorProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( GraphView.GraphColor ) ) );

        colorProp.SetValue( null );

        CodeSnippetExpression rhs = (CodeSnippetExpression)colorProp.GetRhs( );
        Assert.That( rhs.Value, Is.EqualTo( "null" ) );

        colorProp.SetValue( new TerminalGuiAttribute( Color.BrightMagenta, Color.Blue ) );

        rhs = (CodeSnippetExpression)colorProp.GetRhs( );
        Assert.That( rhs.Value, Is.EqualTo( "new Terminal.Gui.Attribute(Color.BrightMagenta,Color.Blue)" ) );
    }

    [Test]
    public void PropertyOfType_PointF( )
    {
        using GraphView graphView = new ( );
        Design d = new( new( $"{nameof( PropertyOfType_PointF )}.cs" ), "FFF", graphView );
        Property pointProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( GraphView.ScrollOffset ) ) );

        pointProp.SetValue( new PointF( 4.5f, 4.1f ) );

        CodeObjectCreateExpression rhs = (CodeObjectCreateExpression)pointProp.GetRhs( );

        // The code generated should be a new PointF
        Assert.That( rhs.Parameters, Has.Count.EqualTo( 2 ) );
    }

    [Test]
    public void PropertyOfType_Pos( )
    {
        using Label label = new ( );
        Design d = new( new( $"{nameof( PropertyOfType_Pos )}.cs" ), "FFF", label );
        Property xProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( View.X ) ) );

        xProp.SetValue( Pos.Center( ) );

        CodeSnippetExpression rhs = (CodeSnippetExpression)xProp.GetRhs( );

        // The code generated for a Property of Type Pos should be the function call
        Assert.That( rhs.Value, Is.EqualTo( "Pos.Center()" ) );
    }

    [Test]
    public void PropertyOfType_Rune( )
    {
        FileInfo file = new( $"{nameof( PropertyOfType_Rune )}.cs" );
        using LineView lv = new( );
        Design d = new( new( file ), "lv", lv );
        Property prop = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( "LineRune" ) );

        prop.SetValue( 'F' );

        Assert.That( lv.LineRune, Is.EqualTo( new Rune( 'F' ) ) );

        string code = Helpers.ExpressionToCode( prop.GetRhs( ) );

        Assert.That( code, Is.EqualTo( "new System.Text.Rune('F')" ) );
    }

    [Test]
    public void PropertyOfType_Size( )
    {
        using ScrollView scrollView = new( );
        Design d = new( new( $"{nameof( PropertyOfType_Size )}.cs" ), "FFF", scrollView );
        Property xProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( View.X ) ) );

        xProp.SetValue( Pos.Center( ) );

        CodeSnippetExpression rhs = (CodeSnippetExpression)xProp.GetRhs( );

        // The code generated for a Property of Type Pos should be the function call
        Assert.That( rhs.Value, Is.EqualTo( "Pos.Center()" ) );
    }
}