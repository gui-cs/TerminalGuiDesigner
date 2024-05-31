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
            Assert.That( lv.Height, Is.EqualTo( Dim.Absolute( 1 ) ) );
        } );
    }

    [Test( ExpectedResult = "new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightMagenta,Terminal.Gui.Color.Blue)")]
    public string PropertyOfType_Attribute( )
    {
        using GraphView graphView = new( );
        Design d = new( new( $"{nameof( PropertyOfType_Attribute )}.cs" ), "FFF", graphView );
        Property colorProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( GraphView.GraphColor ) ) );

        colorProp.SetValue( null );

        CodeSnippetExpression? rhs = colorProp.GetRhs( ) as CodeSnippetExpression;
        Assert.That( rhs, Is.Not.Null.And.InstanceOf<CodeSnippetExpression>( ) );
        Assert.That( rhs!.Value, Is.Not.Null.And.EqualTo( "null" ) );

        colorProp.SetValue( new TerminalGuiAttribute( Color.BrightMagenta, Color.Blue ) );

        rhs = (CodeSnippetExpression)colorProp.GetRhs( );
        return rhs.Value;
    }

    [Test]
    public void PropertyOfType_PointF( [Values( 4.5f, 10.1f )] float x, [Values( 4.5f, 10.1f )] float y )
    {
        using GraphView graphView = new( );
        Design d = new( new( $"{nameof( PropertyOfType_PointF )}.cs" ), "FFF", graphView );
        Property pointProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( GraphView.ScrollOffset ) ) );

        PointF pointF = new( x, y );
        pointProp.SetValue( pointF );

        // The code generated should be a new PointF
        CodeObjectCreateExpression? rhs = pointProp.GetRhs( ) as CodeObjectCreateExpression;

        Assert.That( rhs, Is.Not.Null.And.InstanceOf<CodeObjectCreateExpression>( ) );
        Assert.That( rhs!.Parameters, Is.Not.Null.And.InstanceOf<CodeExpressionCollection>( ) );
        Assert.That( rhs.Parameters, Has.Count.EqualTo( 2 ) );
    }

    [Test( ExpectedResult = "Pos.Center()" )]
    [Category( "Code Generation" )]
    public string PropertyOfType_Pos( )
    {
        using Label label = new( );
        Design d = new( new( $"{nameof( PropertyOfType_Pos )}.cs" ), "FFF", label );
        Property xProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( View.X ) ) );

        xProp.SetValue( Pos.Center( ) );

        return ( (CodeSnippetExpression)xProp.GetRhs( ) ).Value;
    }

    [Test]
    public void PropertyOfType_Rune( [Values( 'a', 'A', 'f', 'F' )] char runeCharacter )
    {
        FileInfo file = new( $"{nameof( PropertyOfType_Rune )}_{runeCharacter}.cs" );
        using LineView lv = new( );
        Design d = new( new( file ), "lv", lv );
        Property prop = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( "LineRune" ) );

        prop.SetValue( runeCharacter );

        Assert.That( lv.LineRune, Is.EqualTo( new Rune( runeCharacter ) ) );

        string code = Helpers.ExpressionToCode( prop.GetRhs( ) );

        Assert.That( code, Is.EqualTo( $"new System.Text.Rune('{runeCharacter}')" ) );
    }

    [Test( ExpectedResult = "Pos.Center()" )]
    [Category( "Code Generation" )]
    public string PropertyOfType_Size( )
    {
        using ScrollView scrollView = new( );
        Design d = new( new( $"{nameof( PropertyOfType_Size )}.cs" ), "FFF", scrollView );
        Property xProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( View.X ) ) );

        xProp.SetValue( Pos.Center( ) );

        return ( (CodeSnippetExpression)xProp.GetRhs( ) ).Value;
    }
}