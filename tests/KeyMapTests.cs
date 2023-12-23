using System.IO;
using Microsoft.VisualBasic.CompilerServices;
using Terminal.Gui;
using TerminalGuiDesigner.UI;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( KeyMap ) )]
[Category( "Core" )]
[Category( "UI" )]
[Parallelizable( ParallelScope.All )]
internal class KeyMapTests
{
    private const string ExpectedKeysYamlContent =
        """
        EditProperties: F4
        ShowContextMenu: Enter
        ViewSpecificOperations: ShiftMask, F4
        EditRootProperties: F5
        ShowHelp: F1
        New: N, CtrlMask
        Open: O, CtrlMask
        Save: S, CtrlMask
        Redo: Y, CtrlMask
        Undo: Z, CtrlMask
        Delete: DeleteChar
        ToggleDragging: F3
        AddView: F2
        ToggleShowFocused: L, CtrlMask
        ToggleShowBorders: B, CtrlMask
        RightClick: Button3Clicked
        Copy: C, CtrlMask
        Paste: V, CtrlMask
        Rename: R, CtrlMask
        SetShortcut: T, CtrlMask
        SelectAll: A, CtrlMask
        MoveRight: CursorRight, ShiftMask
        MoveLeft: CursorLeft, ShiftMask
        MoveUp: CursorUp, ShiftMask
        MoveDown: CursorDown, ShiftMask
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
        KeyMap stockKeyMap = KeyMap.LoadFromConfiguration( );
        KeyMap nullOrMissingFileKeyMap = KeyMap.LoadFromConfiguration( );

        Assert.Multiple( ( ) =>
        {
            Assert.That( stockKeyMap, Is.EqualTo( defaultKeyMap ) );
            Assert.That( nullOrMissingFileKeyMap, Is.EqualTo( defaultKeyMap ) );
        } );

        KeyMap alternateKeyMap = KeyMap.LoadFromConfiguration( "Keys_Alternate.yaml" );

        Assert.Multiple( ( ) =>
        {
            Assert.That( alternateKeyMap.EditProperties, Is.EqualTo( Key.F1 ) );
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
            Key.F4,
            Key.Enter,
            Key.F4.WithShift,
            Key.F5,
            Key.F1,
            Key.N.WithCtrl,
            Key.O.WithCtrl,
            Key.S.WithCtrl,
            Key.Y.WithCtrl,
            Key.Z.WithCtrl,
            Key.DeleteChar,
            Key.F3,
            Key.F2,
            Key.L.WithCtrl,
            Key.B.WithCtrl,
            MouseFlags.Button3Clicked,
            Key.C.WithCtrl,
            Key.V.WithCtrl,
            Key.R.WithCtrl,
            Key.T.WithCtrl,
            Key.A.WithCtrl,
            Key.CursorRight.WithShift,
            Key.CursorLeft.WithShift,
            Key.CursorUp.WithShift,
            Key.CursorDown.WithShift,
            Key.F6 );

        Assert.That( defaultValue, Is.EqualTo( expectedValue ) );
    }

    [Test]
    public void NonDestructiveMutation( )
    {
        KeyMap defaultKeyMap = new( );
        KeyMap mutatedKeyMap = defaultKeyMap with { AddView = Key.M };

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
