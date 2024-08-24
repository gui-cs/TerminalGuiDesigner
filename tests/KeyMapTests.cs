using TerminalGuiDesigner.UI;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( KeyMap ) )]
[Category( "Core" )]
[Category( "UI" )]
[NonParallelizable]
internal class KeyMapTests
{
    private const string ExpectedKeysYamlContent =
        """
        EditProperties: F4
        ShowContextMenu: Enter
        ViewSpecificOperations: Shift+F4
        EditRootProperties: F5
        ShowHelp: F1
        New: Ctrl+N
        Open: Ctrl+O
        Save: Ctrl+S
        Redo: Ctrl+Y
        Undo: Ctrl+Z
        Delete: Delete
        ToggleDragging: F3
        AddView: F2
        ToggleShowFocused: Ctrl+L
        ToggleShowBorders: Ctrl+B
        RightClick: Button3Clicked
        Copy: Ctrl+C
        Paste: Ctrl+V
        Rename: Ctrl+R
        SetShortcut: Ctrl+T
        SelectAll: Ctrl+A
        MoveRight: Shift+CursorRight
        MoveLeft: Shift+CursorLeft
        MoveUp: Shift+CursorUp
        MoveDown: Shift+CursorDown
        ShowColorSchemes: F6
        SelectionColor:
          NormalForeground: BrightGreen
          NormalBackground: Green
          HotNormalForeground: BrightGreen
          HotNormalBackground: Green
          FocusForeground: BrightYellow
          FocusBackground: Green
          HotFocusForeground: BrightYellow
          HotFocusBackground: Green
          DisabledForeground: BrightGreen
          DisabledBackground: Green
            
        
        """;

    [Test]
    [Category( "Configuration" )]
    public void Configuration_LoadingAndBinding( )
    {
        KeyMap defaultKeyMap = new( );
        KeyMap stockKeyMap = KeyMap.LoadFromYamlConfigurationFile( );
        KeyMap nullOrMissingFileKeyMap = KeyMap.LoadFromYamlConfigurationFile( );

        Assert.Multiple( ( ) =>
        {
            Assert.That( stockKeyMap, Is.EqualTo( defaultKeyMap ) );
            Assert.That( nullOrMissingFileKeyMap, Is.EqualTo( defaultKeyMap ) );
        } );

        KeyMap alternateKeyMap = KeyMap.LoadFromYamlConfigurationFile( "Keys_Alternate.yaml" );

        Assert.Multiple( ( ) =>
        {
            Assert.That( alternateKeyMap.EditProperties, Is.EqualTo( Key.F1.ToString() ) );
            Assert.That( alternateKeyMap, Is.Not.EqualTo( defaultKeyMap ) );
        } );
    }

    [Test]
    [Order( 1 )]
    public void Constructor_ReturnsValidInstance( )
    {
        KeyMap? k = null;
        Assert.That( ( ) => k = new( ), Throws.Nothing );
        Assert.That( k, Is.Not.Null.And.InstanceOf<KeyMap>( ) );
    }

    [Test]
    [Order( 3 )]
    [Category( "Change Control" )]
    [Description( "Ensures the default keymap file is present and as expected for testing, with allowances for line ending changes." )]
    public void DefaultKeyMapFile_NotChanged( )
    {
        // This may seem redundant to just having it all in code, but this is a two-way guarantee,
        // to ensure that changes to the file OR the collection in code are deliberate.
        var defaultKeyMapFileName = Path.Combine( TestContext.CurrentContext.TestDirectory, "Keys.yaml" );

        Assert.That( defaultKeyMapFileName, Does.Exist );
        Assert.That( defaultKeyMapFileName, Is.Not.Empty );

        Assert.That( File.ReadAllText( defaultKeyMapFileName ).ReplaceLineEndings( ),
                     Is.EqualTo( ExpectedKeysYamlContent.ReplaceLineEndings() ),
                     "Content of Keys.yaml, including newline style, must match expected input." );
    }

    [Test]
    [Order( 2 )]
    [Category( "Change Control" )]
    public void EmptyConstructor_HasExpectedDefaults( )
    {
        KeyMap defaultValue = new( );
        KeyMap expectedValue = new(
            Key.F4.ToString( ),
            Key.Enter.ToString( ),
            Key.F4.WithShift.ToString( ),
            Key.F5.ToString( ),
            Key.F1.ToString( ),
            Key.N.WithCtrl.ToString( ),
            Key.O.WithCtrl.ToString( ),
            Key.S.WithCtrl.ToString( ),
            Key.Y.WithCtrl.ToString( ),
            Key.Z.WithCtrl.ToString( ),
            Key.DeleteChar.ToString( ),
            Key.F3.ToString( ),
            Key.F2.ToString( ),
            Key.L.WithCtrl.ToString( ),
            Key.B.WithCtrl.ToString( ),
            MouseFlags.Button3Clicked,
            Key.C.WithCtrl.ToString( ),
            Key.V.WithCtrl.ToString( ),
            Key.R.WithCtrl.ToString( ),
            Key.T.WithCtrl.ToString( ),
            Key.A.WithCtrl.ToString( ),
            Key.CursorRight.WithShift.ToString( ),
            Key.CursorLeft.WithShift.ToString( ),
            Key.CursorUp.WithShift.ToString( ),
            Key.CursorDown.WithShift.ToString( ),
            Key.F6.ToString( ) );

        Assert.That( defaultValue, Is.EqualTo( expectedValue ) );
    }

    [Test]
    public void NonDestructiveMutation( )
    {
        KeyMap defaultKeyMap = new( );
        KeyMap mutatedKeyMap = defaultKeyMap with { AddView = Key.M.ToString() };

        Assert.Multiple( ( ) =>
        {
            // Both value and reference equality should be lost after mutating with a different value.
            Assert.That( mutatedKeyMap, Is.Not.EqualTo( defaultKeyMap ) );
            Assert.That( mutatedKeyMap, Is.Not.SameAs( defaultKeyMap ) );
        } );

        KeyMap mutatedKeyMapWithOriginalAddViewKey = mutatedKeyMap with { AddView = defaultKeyMap.AddView };
        Assert.Multiple( ( ) =>
        {
            // Both value and reference equality should be lost after mutating with a different value.
            Assert.That( mutatedKeyMapWithOriginalAddViewKey, Is.Not.EqualTo( mutatedKeyMap ) );
            Assert.That( mutatedKeyMapWithOriginalAddViewKey, Is.Not.SameAs( mutatedKeyMap ) );

            // Value equality should have been restored between these two, but not reference equality.
            Assert.That( mutatedKeyMapWithOriginalAddViewKey, Is.EqualTo( defaultKeyMap ) );
            Assert.That( mutatedKeyMapWithOriginalAddViewKey, Is.Not.SameAs( defaultKeyMap ) );
        } );
    }
}
