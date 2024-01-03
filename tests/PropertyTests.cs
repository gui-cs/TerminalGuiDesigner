using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CSharp;


using Attribute = Terminal.Gui.Attribute;

namespace UnitTests;

internal class PropertyTests : Tests
{
    [Test]
    public void PropertyOfType_Pos( )
    {
        Design d = new(new(nameof(PropertyOfType_Pos) + ".cs"), "FFF", new Label());
        Property xProp = d.GetDesignableProperties().Single( static p => p.PropertyInfo.Name.Equals(nameof(View.X)));

        xProp.SetValue( Pos.Center( ) );

        CodeSnippetExpression rhs = (CodeSnippetExpression)xProp.GetRhs();

        // The code generated for a Property of Type Pos should be the function call
        ClassicAssert.AreEqual( rhs.Value, "Pos.Center()" );
    }

    [Test]
    public void PropertyOfType_Size( )
    {
        Design d = new(new(nameof(PropertyOfType_Size) + ".cs"), "FFF", new ScrollView());
        Property xProp = d.GetDesignableProperties().Single( static p => p.PropertyInfo.Name.Equals(nameof(View.X)));

        xProp.SetValue( Pos.Center( ) );

        CodeSnippetExpression rhs = (CodeSnippetExpression)xProp.GetRhs();

        // The code generated for a Property of Type Pos should be the function call
        ClassicAssert.AreEqual( rhs.Value, "Pos.Center()" );
    }

    [Test]
    public void PropertyOfType_Attribute( )
    {
        Design d = new(new(nameof(PropertyOfType_Attribute) + ".cs"), "FFF", new GraphView());
        Property colorProp = d.GetDesignableProperties().Single( static p => p.PropertyInfo.Name.Equals(nameof(GraphView.GraphColor)));

        colorProp.SetValue( null );

        CodeSnippetExpression rhs = (CodeSnippetExpression)colorProp.GetRhs();
        ClassicAssert.AreEqual( rhs.Value, "null" );

        colorProp.SetValue( new Attribute( Color.BrightMagenta, Color.Blue ) );

        rhs = (CodeSnippetExpression)colorProp.GetRhs( );
        ClassicAssert.AreEqual( rhs.Value, "new Terminal.Gui.Attribute(Color.BrightMagenta,Color.Blue)" );
    }

    [Test]
    public void PropertyOfType_PointF( )
    {
        Design d = new(new(nameof(PropertyOfType_PointF) + ".cs"), "FFF", new GraphView());
        Property pointProp = d.GetDesignableProperties().Single( static p => p.PropertyInfo.Name.Equals(nameof(GraphView.ScrollOffset)));

        pointProp.SetValue( new PointF( 4.5f, 4.1f ) );

        CodeObjectCreateExpression rhs = (CodeObjectCreateExpression)pointProp.GetRhs();

        // The code generated should be a new PointF
        ClassicAssert.AreEqual( rhs.Parameters.Count, 2 );
    }

    [Test]
    public void PropertyOfType_Rune( )
    {
        ViewToCode viewToCode = new();

        FileInfo file = new("TestPropertyOfType_Rune.cs");
        LineView lv = new();
        Design d = new(new(file), "lv", lv);
        Property prop = d.GetDesignableProperties().Single( static p => p.PropertyInfo.Name.Equals("LineRune"));

        prop.SetValue( 'F' );

        ClassicAssert.AreEqual( new Rune( 'F' ), lv.LineRune );

        string code = ExpressionToCode(prop.GetRhs());

        ClassicAssert.AreEqual( "new System.Text.Rune('F')", code );
    }

    [Test]
    public void Changing_LineViewOrientation( )
    {
        Design v = Get10By10View();
        LineView lv = (LineView)ViewFactory.Create(typeof(LineView));
        Design d = new(v.SourceCode, "lv", lv);

        v.View.Add( lv );
        lv.IsInitialized = true;

        ClassicAssert.AreEqual( Orientation.Horizontal, lv.Orientation );
        ClassicAssert.AreEqual( new Rune( '─' ), lv.LineRune );
        Property? prop = d.GetDesignableProperty(nameof(LineView.Orientation));

        ClassicAssert.IsNotNull( prop );
        prop?.SetValue( Orientation.Vertical );
        ClassicAssert.AreEqual( ConfigurationManager.Glyphs.VLine, lv.LineRune );

        // now try with a dim fill
        lv.Height = Dim.Fill( );
        lv.Width = 1;

        prop?.SetValue( Orientation.Horizontal );
        ClassicAssert.AreEqual( Orientation.Horizontal, lv.Orientation );
        ClassicAssert.AreEqual( ConfigurationManager.Glyphs.HLine, lv.LineRune );
        ClassicAssert.AreEqual( Dim.Fill( ), lv.Width );
        ClassicAssert.AreEqual( Dim.Sized( 1 ), lv.Height );
    }

    public static string ExpressionToCode( CodeExpression expression )
    {
        CSharpCodeProvider provider = new ();

        using ( StringWriter sw = new( ) )
        {
            IndentedTextWriter tw = new(sw, "    ");
            provider.GenerateCodeFromExpression( expression, tw, new( ) );
            tw.Close( );

            return sw.GetStringBuilder( ).ToString( );
        }
    }
}
