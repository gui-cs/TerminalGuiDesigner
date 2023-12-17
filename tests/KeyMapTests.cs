using System.IO;
using TerminalGuiDesigner.UI;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( KeyMap ) )]
[Category( "Core" )]
[Category( "UI" )]
[Parallelizable( ParallelScope.All )]
internal class KeyMapTests : Tests
{
    private const string _expectedKeysYamlContent =
        @"EditProperties: F4
ShowContextMenu: Enter
ViewSpecificOperations: F4, ShiftMask
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
    
";

    [Test]
    [Category( "Change Control" )]
    [Description( "Ensures the default keymap file is present and as expected for testing, with allowances for line ending changes." )]
    [Order( 1 )]
    public void DefaultKeyMapFile_NotChanged( )
    {
        // This may seem redundant to just having it all in code, but this is a two-way guarantee,
        // to ensure that changes to the file OR the collection in code are deliberate.
        var defaultKeyMapFileName = Path.Combine( TestContext.CurrentContext.TestDirectory, "Keys.yaml" );

        Assert.That( defaultKeyMapFileName, Does.Exist );
        Assert.That( defaultKeyMapFileName, Is.Not.Empty );

        FileInfo defaultKeyMapFileInfo = new( defaultKeyMapFileName );

        Assert.That( defaultKeyMapFileInfo, Has.Length.EqualTo( 921 ) );

        Assert.That( File.ReadAllText( defaultKeyMapFileName ),
                     Is.EqualTo( _expectedKeysYamlContent ),
                     "Content of Keys.yaml, including newline style, must match expected input." );
    }
}
