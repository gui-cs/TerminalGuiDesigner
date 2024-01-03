using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CSharp;
using Attribute = Terminal.Gui.Attribute;

namespace UnitTests;

internal class PropertyTests : Tests
{
    [Test]
    public void Changing_LineViewOrientation( )
    {
        Design v = Get10By10View( );
        LineView lv = (LineView)ViewFactory.Create( typeof( LineView ) );
        Design d = new( v.SourceCode, "lv", lv );

        v.View.Add( lv );
        lv.IsInitialized = true;

        Assert.That( lv.Orientation, Is.EqualTo( Orientation.Horizontal ) );
        Assert.That( lv.LineRune, Is.EqualTo( new Rune( '─' ) ) );
        Property? prop = d.GetDesignableProperty( nameof( LineView.Orientation ) );

        Assert.That( prop, Is.Not.Null );
        prop?.SetValue( Orientation.Vertical );
        Assert.That( lv.LineRune, Is.EqualTo( ConfigurationManager.Glyphs.VLine ) );

        // now try with a dim fill
        lv.Height = Dim.Fill( );
        lv.Width = 1;

        prop?.SetValue( Orientation.Horizontal );
        Assert.That( lv.Orientation, Is.EqualTo( Orientation.Horizontal ) );
        Assert.That( lv.LineRune, Is.EqualTo( ConfigurationManager.Glyphs.HLine ) );
        Assert.That( lv.Width, Is.EqualTo( Dim.Fill( ) ) );
        Assert.That( lv.Height, Is.EqualTo( Dim.Sized( 1 ) ) );
    }

    public static string ExpressionToCode( CodeExpression expression )
    {
        CSharpCodeProvider provider = new( );

        using ( StringWriter sw = new( ) )
        {
            IndentedTextWriter tw = new( sw, "    " );
            provider.GenerateCodeFromExpression( expression, tw, new( ) );
            tw.Close( );

            return sw.GetStringBuilder( ).ToString( );
        }
    }

    [Test]
    public void PropertyOfType_Attribute( )
    {
        Design d = new( new( nameof( PropertyOfType_Attribute ) + ".cs" ), "FFF", new GraphView( ) );
        Property colorProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( GraphView.GraphColor ) ) );

        colorProp.SetValue( null );

        CodeSnippetExpression rhs = (CodeSnippetExpression)colorProp.GetRhs( );
        Assert.That( "null", Is.EqualTo( rhs.Value ) );

        colorProp.SetValue( new Attribute( Color.BrightMagenta, Color.Blue ) );

        rhs = (CodeSnippetExpression)colorProp.GetRhs( );
        Assert.That( rhs.Value, Is.EqualTo( "new Terminal.Gui.Attribute(Color.BrightMagenta,Color.Blue)" ) );
    }

    [Test]
    public void PropertyOfType_PointF( )
    {
        Design d = new( new( nameof( PropertyOfType_PointF ) + ".cs" ), "FFF", new GraphView( ) );
        Property pointProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( GraphView.ScrollOffset ) ) );

        pointProp.SetValue( new PointF( 4.5f, 4.1f ) );

        CodeObjectCreateExpression rhs = (CodeObjectCreateExpression)pointProp.GetRhs( );

        // The code generated should be a new PointF
        Assert.That( 2, Is.EqualTo( rhs.Parameters.Count ) );
    }

    [Test]
    public void PropertyOfType_Pos( )
    {
        Design d = new( new( nameof( PropertyOfType_Pos ) + ".cs" ), "FFF", new Label( ) );
        Property xProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( View.X ) ) );

        xProp.SetValue( Pos.Center( ) );

        CodeSnippetExpression rhs = (CodeSnippetExpression)xProp.GetRhs( );

        // The code generated for a Property of Type Pos should be the function call
        Assert.That( rhs.Value, Is.EqualTo( "Pos.Center()" ) );
    }

    [Test]
    public void PropertyOfType_Rune( )
    {
        ViewToCode viewToCode = new( );

        FileInfo file = new( "TestPropertyOfType_Rune.cs" );
        LineView lv = new( );
        Design d = new( new( file ), "lv", lv );
        Property prop = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( "LineRune" ) );

        prop.SetValue( 'F' );

        Assert.That( lv.LineRune, Is.EqualTo( new Rune( 'F' ) ) );

        string code = ExpressionToCode( prop.GetRhs( ) );

        Assert.That( code, Is.EqualTo( "new System.Text.Rune('F')" ) );
    }

    [Test]
    public void PropertyOfType_Size( )
    {
        Design d = new( new( nameof( PropertyOfType_Size ) + ".cs" ), "FFF", new ScrollView( ) );
        Property xProp = d.GetDesignableProperties( ).Single( static p => p.PropertyInfo.Name.Equals( nameof( View.X ) ) );

        xProp.SetValue( Pos.Center( ) );

        CodeSnippetExpression rhs = (CodeSnippetExpression)xProp.GetRhs( );

        // The code generated for a Property of Type Pos should be the function call
        Assert.That( rhs.Value, Is.EqualTo( "Pos.Center()" ) );
    }
}
