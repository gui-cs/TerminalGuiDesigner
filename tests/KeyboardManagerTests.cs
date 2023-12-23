using System.Collections.Generic;
using System.IO;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( KeyboardManager ) )]
[Category( "UI" )]
internal class KeyboardManagerTests : Tests
{

    private readonly Key backspace = Key.Backspace;

    /// <summary>
    /// This test runs first and will leave behind a KeyboardManager we can reuse in the rest of these tests.
    /// </summary>
    [Test]
    [Order( 1 )]
    public void Constructor( )
    {
        KeyMap? keyMap = null;
        Assume.That( ( ) => keyMap = new( ), Throws.Nothing );
        Assume.That( keyMap, Is.Not.Null.And.TypeOf<KeyMap>( ) );

        KeyboardManager? mgr = null;
        Assert.That( ( ) => mgr = new( keyMap! ), Throws.Nothing );
        Assert.That( mgr, Is.Not.Null.And.TypeOf<KeyboardManager>( ) );
    }


    [Test]
    public void Backspace_WithDateFieldSelected( )
    {
        DateField v = ViewFactory.Create<DateField>( );
        Assume.That( v, Is.Not.Null.And.TypeOf<DateField>( ) );

        FileInfo? file = null;
        Assume.That( ( ) => file = new( "ff.cs" ), Throws.Nothing );
        Assume.That( file, Is.Not.Null.And.TypeOf<FileInfo>( ) );

        SourceCodeFile? sourceCodeFile = null;
        Assume.That( ( ) => sourceCodeFile = new( file! ), Throws.Nothing );
        Assume.That( sourceCodeFile, Is.Not.Null.And.TypeOf<SourceCodeFile>( ) );

        Design? d = null;
        Assume.That( ( ) => d = new( sourceCodeFile!, "ff", v ), Throws.Nothing );
        Assume.That( d, Is.Not.Null.And.TypeOf<Design>( ) );
        v.Data = d;

        KeyMap? keyMap = null;
        Assume.That( ( ) => keyMap = new( ), Throws.Nothing );
        Assume.That( keyMap, Is.Not.Null.And.TypeOf<KeyMap>( ) );

        KeyboardManager? mgr = null;
        Assert.That( ( ) => mgr = new( keyMap! ), Throws.Nothing );
        Assert.That( mgr, Is.Not.Null.And.TypeOf<KeyboardManager>( ) );

        bool keyEventSuppressed = false;
        Assert.That( ( ) => keyEventSuppressed = mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( keyEventSuppressed, Is.False );

        //TODO: What is this stuff doing and why?
        Application.Top.Add( v );
        v.Bounds = new( 0, 0, 6, 1 );
        v.Draw( );
    }

    [Test]
    [Parallelizable(ParallelScope.Self)]
    public void ButtonRename( )
    {
        Button v = ViewFactory.Create<Button>( );
        Assume.That( v, Is.Not.Null.And.TypeOf<Button>( ) );

        FileInfo? file = null;
        Assume.That( ( ) => file = new( "ff.cs" ), Throws.Nothing );
        Assume.That( file, Is.Not.Null.And.TypeOf<FileInfo>( ) );

        SourceCodeFile? sourceCodeFile = null;
        Assume.That( ( ) => sourceCodeFile = new( file! ), Throws.Nothing );
        Assume.That( sourceCodeFile, Is.Not.Null.And.TypeOf<SourceCodeFile>( ) );

        Design? d = null;
        Assume.That( ( ) => d = new( sourceCodeFile!, "ff", v ), Throws.Nothing );
        Assume.That( d, Is.Not.Null.And.TypeOf<Design>( ) );
        v.Data = d;

        KeyMap? keyMap = null;
        Assume.That( ( ) => keyMap = new( ), Throws.Nothing );
        Assume.That( keyMap, Is.Not.Null.And.TypeOf<KeyMap>( ) );

        KeyboardManager? mgr = null;
        Assert.That( ( ) => mgr = new( keyMap! ), Throws.Nothing );
        Assert.That( mgr, Is.Not.Null.And.TypeOf<KeyboardManager>( ) );

        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );

        Assert.That( string.IsNullOrEmpty( v.Text ) );

        mgr!.HandleKey( v, Key.B.WithShift);
        mgr.HandleKey( v, (Key)'a');
        mgr.HandleKey( v, (Key)'d');

        Assert.That( v.Text, Is.EqualTo( "Bad" ) );

        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );
        Assert.That( ( ) => mgr!.HandleKey( v, backspace ), Throws.Nothing );

        Assert.That( v.Text, Is.EqualTo( "B" ) );
    }


    [Test]
    public void HandleKey_AddsExpectedCharactersToView_AsciiAlphanumeric<T>( [ValueSource( nameof( Get_HandleKey_AddsExpectedCharactersToView_AsciiAlphanumeric_TestViews ) )] T testView, [ValueSource( nameof( GetAsciiAlphanumerics ) )] char k )
        where T : View
    {

        KeyMap? keyMap = null;
        Assume.That( () => keyMap = new(), Throws.Nothing );
        Assume.That( keyMap, Is.Not.Null.And.TypeOf<KeyMap>() );

        KeyboardManager? mgr = null;
        Assert.That( () => mgr = new( keyMap! ), Throws.Nothing );
        Assert.That( mgr, Is.Not.Null.And.TypeOf<KeyboardManager>() );

        Assume.That( testView, Is.Not.Null.And.InstanceOf<T>() );

        Assume.That( mgr, Is.Not.Null.And.InstanceOf<KeyboardManager>() );

        Design d = Get10By10View();
        Assume.That( new AddViewOperation( testView, d, "testView" ).Do() );
        testView.SetFocus();

        Assert.That( () => mgr!.HandleKey( testView,  (Key)k), Throws.Nothing );

        Assert.That( testView.Text, Is.Not.Null );
        Assert.That( testView.Text[ ^1 ], Is.EqualTo( k ) );
    }

    private static IEnumerable<View> Get_HandleKey_AddsExpectedCharactersToView_AsciiAlphanumeric_TestViews()
    {
        yield return ViewFactory.Create<Button>();
        yield return ViewFactory.Create<Label>();
    }

    private static IEnumerable<char> GetAsciiAlphanumerics( )
    {
        for ( char charValue = char.MinValue; charValue < char.MaxValue; charValue++ )
        {
            if ( char.IsAsciiLetterOrDigit( charValue ) )
            {
                yield return charValue;
            }
        }
    }
}
