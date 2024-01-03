using System.Runtime.CompilerServices;

namespace UnitTests.UI;

[TestFixture]
[TestOf( typeof( MouseManager ) )]
[Category( "UI" )]
internal class MouseManagerTests : Tests
{
    [Test]
    public void DragResizeView<T>( [ValueSource( nameof( DragResizeView_Types ) )] T dummy )
        where T : View, new( )
    {
        Design d = Get10By10View( );
        Assume.That( dummy, Is.TypeOf<T>( ) );

        T view = ViewFactory.Create<T>( );
        view.Width = 8;
        view.Height = 1;

        Design design = new( d.SourceCode, "myView", view );
        view.Data = design;
        d.View.Add( view );

        Assert.That( view.Bounds.Width, Is.EqualTo( 8 ) );
        MouseManager mgr = new( );

        // we haven't done anything yet
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        Assert.That( view.Bounds.X, Is.Zero );
        Assert.That( view.Bounds.Y, Is.Zero );
        Assert.That( view.Bounds.Width, Is.EqualTo( 8 ) );
        Assert.That( view.Bounds.Height, Is.EqualTo( 1 ) );

        // user presses down in the lower right of control
        MouseEvent e = new( )
        {
            X = 6,
            Y = 0,
            Flags = MouseFlags.Button1Pressed
        };

        mgr.HandleMouse( e, d );

        Assert.That( view.Bounds.Y, Is.Zero );

        // we still haven't committed to anything
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        // user pulled view size +1 width and +1 height
        e = new( )
        {
            X = 9,
            Y = 0,
            Flags = MouseFlags.Button1Pressed
        };
        mgr.HandleMouse( e, d );

        Assert.That( view.Bounds.X, Is.Zero );
        Assert.That( view.Bounds.Y, Is.Zero );
        Assert.That( view.Bounds.Width, Is.EqualTo( 10 ), "Expected resize to increase Width when dragging" );
        Assert.That( view.Bounds.Height, Is.EqualTo( 1 ), "Expected resize of button to ignore Y component" );

        // we still haven't committed to anything
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        // user releases mouse (in place)
        e = new( )
        {
            X = 9,
            Y = 0
        };
        mgr.HandleMouse( e, d );

        Assert.That( view.Bounds.X, Is.Zero );
        Assert.That( view.Bounds.Y, Is.Zero );
        Assert.That( view.Bounds.Width, Is.EqualTo( 10 ), "Expected resize to increase Width when dragging" );
        Assert.That( view.Bounds.Height, Is.EqualTo( 1 ) );

        // we have now committed the drag so could undo
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
    }

    [Test]
    [TestCaseSource( nameof( DragResizeView_CannotResize_DimFill_Types ) )]
    public void DragResizeView_CannotResize_DimFill<T>( T dummy )
        where T : View, new( )
    {
        Design d = Get10By10View( );
        Assume.That( dummy, Is.TypeOf<T>( ) );

        T view = ViewFactory.Create<T>( );
        view.Width = Dim.Fill( );
        view.Height = 1;

        Design design = new( d.SourceCode, "myView", view );
        view.Data = design;
        d.View.Add( view );

        MouseManager mgr = new( );

        // we haven't done anything yet
        Assert.Multiple( ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
            Assert.That( view.Bounds.X, Is.Zero );
            Assert.That( view.Bounds.Y, Is.Zero );
            Assert.That( view.Bounds.Width, Is.EqualTo( 10 ) );
            Assert.That( view.Bounds.Height, Is.EqualTo( 1 ) );
        } );

        // user presses down in the lower right of control
        MouseEvent e = new( )
        {
            X = 9,
            Y = 0,
            Flags = MouseFlags.Button1Pressed
        };

        mgr.HandleMouse( e, d );

        Assert.That( view.Bounds.Y, Is.Zero );

        // we still haven't committed to anything
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        // user pulled view size down and left
        e = new( )
        {
            X = 6,
            Y = 3,
            Flags = MouseFlags.Button1Pressed
        };
        mgr.HandleMouse( e, d );

        Assert.That( view.Bounds.X, Is.Zero );
        Assert.That( view.Bounds.Y, Is.Zero );
        Assert.That( view.Bounds.Width, Is.EqualTo( 10 ), "Expected Width to remain constant because it is Dim.Fill()" );
        Assert.That( view.Bounds.Height, Is.EqualTo( 4 ), "Expected resize to update Y component" );
        Assert.That( view.Width, Is.EqualTo( Dim.Fill( ) ) );
        Assert.That( view.Height, Is.EqualTo( Dim.Sized( 4 ) ) );

        // we still haven't committed to anything
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        // user releases mouse (in place)
        e = new( )
        {
            X = 6,
            Y = 3
        };
        mgr.HandleMouse( e, d );

        Assert.Multiple( ( ) =>
        {
            Assert.That( view.Bounds.X, Is.Zero );
            Assert.That( view.Bounds.Y, Is.Zero );
            Assert.That( view.Bounds.Width, Is.EqualTo( 10 ), "Expected Width to remain constant because it is Dim.Fill()" );
            Assert.That( view.Bounds.Height, Is.EqualTo( 4 ), "Expected resize to update Y component" );
            Assert.That( view.Width, Is.EqualTo( Dim.Fill( ) ) );
            Assert.That( view.Height, Is.EqualTo( Dim.Sized( 4 ) ) );
        } );

        // we have now committed the drag so could undo
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );

        OperationManager.Instance.Undo( );

        // Should reset us to the initial state
        Assert.Multiple( ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
            Assert.That( view.Bounds.X, Is.Zero );
            Assert.That( view.Bounds.Y, Is.Zero );
            Assert.That( view.Bounds.Width, Is.EqualTo( 10 ) );
            Assert.That( view.Bounds.Height, Is.EqualTo( 1 ) );
            Assert.That( view.Width, Is.EqualTo( Dim.Fill( ) ) );
            Assert.That( view.Height, Is.EqualTo( Dim.Sized( 1 ) ) );
        } );
    }

    [Test]
    public void DragView<T>( [ValueSource( nameof( GetDummyViewsForDrag ) )] T dummy, [Values( -5, -1, 0, 1, 5 )] int deltaX, [Values( -5, -1, 0, 1, 5 )] int deltaY )
        where T : View, new( )
    {
        const int initialViewXPos = 0;
        const int initialViewYPos = 0;
        const int startDragX = 1;
        const int startDragY = 0;

        Assume.That( dummy.GetType( ), Is.EqualTo( typeof( T ) ) );
        Design d = Get10By10View( );
        T view = ViewFactory.Create<T>( null, null, "Hi there buddy" );
        Design viewDesign = new( d.SourceCode, $"my{typeof( T ).Name}", view );
        view.Data = viewDesign;
        d.View.Add( view );

        MouseManager mgr = new( );

        // we haven't done anything yet
        Assume.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        Assume.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        Assert.That( view.X, Is.EqualTo( (Pos)initialViewXPos ) );
        Assert.That( view.Y, Is.EqualTo( (Pos)initialViewYPos ) );

        // user presses down over the control
        MouseEvent firstClick = new( )
        {
            X = startDragX,
            Y = startDragY,
            Flags = MouseFlags.Button1Pressed
        };

        mgr.HandleMouse( firstClick, d );

        // we still haven't committed to anything
        Assert.That( view.X, Is.EqualTo( (Pos)initialViewXPos ) );
        Assert.That( view.Y, Is.EqualTo( (Pos)initialViewYPos ) );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        // user moved view but still has mouse down
        MouseEvent dragWithMouseButton1Down = new( )
        {
            X = startDragX + deltaX,
            Y = startDragY + deltaY,
            Flags = MouseFlags.Button1Pressed
        };
        mgr.HandleMouse( dragWithMouseButton1Down, d );

        Assert.That( view.X, Is.EqualTo( (Pos)( initialViewXPos + deltaX ) ) );
        Assert.That( view.Y, Is.EqualTo( (Pos)( initialViewYPos + deltaY ) ) );

        // we still haven't committed to anything
        Assert.That( view.Bounds.X, Is.Zero );
        Assert.That( view.Bounds.Y, Is.Zero );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        // user releases mouse
        MouseEvent releaseMouseButton1AtNewCoordinates = new( )
        {
            X = startDragX + deltaX,
            Y = startDragY + deltaY
        };
        mgr.HandleMouse( releaseMouseButton1AtNewCoordinates, d );

        // We have now committed the drag.
        // Check position
        Assert.Multiple( ( ) =>
        {
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( view.X, Is.EqualTo( (Pos)( initialViewXPos + deltaX ) ) );
            Assert.That( view.Y, Is.EqualTo( (Pos)( initialViewYPos + deltaY ) ) );
        } );

        // Now check that undo state was properly tracked.
        // TODO: This should condense down after the bugs are fixed.
        // Verbose for now for easier debugging.
        switch ( deltaX, deltaY )
        {
            case (-1, 0): // Left only by exactly 1
                Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
                break;
            case (< -1, 0): // Left only by more than 1
                Assert.Warn( "Bug in handling of drag events. Moving left by more than 1 moves the view but does not push to undo stack" );
                Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
                break;
            case (0, -1): // Up only by exactly 1
                Assert.Warn( "Bug in handling of drag events. Moving up by exactly 1 moves the view but does not push to undo stack" );
                Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
                break;
            case (0, < -1): // Up only by more than 1
                Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
                break;
            case (< 0, < 0): // Up and left
                Assert.Warn( "Bug in handling of drag events. Moving up and left to outside bounds moves the view but does not push to undo stack" );
                // TODO: Fix this after bug is fixed.
                Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
                break;
            case (0, 0): // Not actually moved
                Assert.Warn( "Bug in handling of drag events. 0-pixel move still gets pushed onto the undo stack." );
                // TODO: Fix this after bug is fixed.
                // Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
                // The undo stack should be empty, as this was not actually a move.
                Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
                break;
            case (> 0, 0): // Right only
                Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
                break;
            case (0, > 0): // Down only
                Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
                break;
            case (> 0, > 0): // Down and right
                Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
                break;
        }
    }

    [Test]
    public void TestDragResizeView_CannotResize_1By1View( [Values( 0, 3 )] int locationOfViewX, [Values( 0, 3 )] int locationOfViewY )
    {
        Design d = Get10By10View( );

        View view = ViewFactory.Create<View>( );
        view.Width = Dim.Fill( );
        view.X = locationOfViewX;
        view.Y = locationOfViewY;
        view.Width = 1;
        view.Height = 1;

        Design design = new( d.SourceCode, "myView", view );
        view.Data = design;
        d.View.Add( view );

        d.View.LayoutSubviews( );

        MouseManager mgr = new( );

        // we haven't done anything yet
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        Assert.That( view.Frame.X, Is.EqualTo( locationOfViewX ) );
        Assert.That( view.Frame.Y, Is.EqualTo( locationOfViewY ) );
        Assert.That( view.Bounds.Width, Is.EqualTo( 1 ) );
        Assert.That( view.Bounds.Height, Is.EqualTo( 1 ) );

        // user presses down in the lower right of control
        MouseEvent e = new( )
        {
            X = locationOfViewX,
            Y = locationOfViewY,
            Flags = MouseFlags.Button1Pressed
        };

        View? hit = view.HitTest( e, out bool isBorder, out bool isLowerRight );
        Assert.That( hit, Is.SameAs( view ) );
        Assert.That( isBorder );
        Assert.That( isLowerRight, Is.False, "Upper left should never be considered lower right even if view is 1x1" );

        mgr.HandleMouse( e, d );

        Assert.That( view.Frame.Y, Is.EqualTo( locationOfViewY ) );

        // we still haven't committed to anything
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        // user pulled view size down and left
        e = new( )
        {
            X = 6,
            Y = 3,
            Flags = MouseFlags.Button1Pressed
        };
        mgr.HandleMouse( e, d );

        Assert.That( view.Bounds.Width, Is.EqualTo( 1 ) );
        Assert.That( view.Bounds.Height, Is.EqualTo( 1 ) );
    }

    [TestCase( 1, 1, 4, 6, new[] { 0, 2 } )]      // drag from 1,1 to 4,6 and expect labels 0 and 2 to be selected
    [TestCase( 1, 1, 10, 10, new[] { 0, 1, 2 } )] // drag over all
    public void TestDragSelectionBox( int xStart, int yStart, int xEnd, int yEnd, int[] expectSelected )
    {
        Design d = Get10By10View( );

        /*
          Hi
            Hi
          Hi
        */

        Label lbl1 = new( 2, 1, "Hi" );
        Label lbl2 = new( 4, 2, "Hi" );
        Label lbl3 = new( 2, 3, "Hi" );

        Design lbl1Design = new( d.SourceCode, "lbl1", lbl1 );
        Design lbl2Design = new( d.SourceCode, "lbl2", lbl2 );
        Design lbl3Design = new( d.SourceCode, "lbl3", lbl3 );

        lbl1.Data = lbl1Design;
        lbl2.Data = lbl2Design;
        lbl3.Data = lbl3Design;

        Design[] labels = { lbl1Design, lbl2Design, lbl3Design };

        d.View.Add( lbl1 );
        d.View.Add( lbl2 );
        d.View.Add( lbl3 );

        SelectionManager selection = SelectionManager.Instance;
        selection.Clear( );
        MouseManager mgr = new( );

        // user presses down
        MouseEvent e = new( )
        {
            X = xStart,
            Y = yStart,
            Flags = MouseFlags.Button1Pressed
        };

        mgr.HandleMouse( e, d );

        // user pulled selection box to destination
        e = new( )
        {
            X = xEnd,
            Y = yEnd,
            Flags = MouseFlags.Button1Pressed
        };
        mgr.HandleMouse( e, d );

        // user releases mouse (in place)
        e = new( )
        {
            X = xEnd,
            Y = yEnd
        };
        mgr.HandleMouse( e, d );

        // do not expect dragging selection box to change anything
        // or be undoable
        Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        // for each selectable thing
        for ( int i = 0; i < labels.Length; i++ )
        {
            // its selection status should match the expectation
            // passed in the test case
            Assert.That(
                selection.Selected.ToList( ).Contains( labels[ i ] ), Is.EqualTo( expectSelected.Contains( i ) ),
                $"Expectation wrong for label index {i} (indexes are 0 based)" );
        }
    }

    private static IEnumerable<TestCaseData> DragResizeView_CannotResize_DimFill_Types( )
    {
        yield return new( (View)RuntimeHelpers.GetUninitializedObject( typeof( View ) ) ) { TestName = "DragResizeView_CannotResize_DimFill<View>" };
    }

    private static IEnumerable<View> DragResizeView_Types( )
    {
        yield return (Label)RuntimeHelpers.GetUninitializedObject( typeof( Label ) );
        yield return (Button)RuntimeHelpers.GetUninitializedObject( typeof( Button ) );
        yield return (TabView)RuntimeHelpers.GetUninitializedObject( typeof( TabView ) );
        yield return (TableView)RuntimeHelpers.GetUninitializedObject( typeof( TableView ) );
        yield return (View)RuntimeHelpers.GetUninitializedObject( typeof( View ) );
    }

    private static IEnumerable<View> GetDummyViewsForDrag( )
    {
        yield return (Label)RuntimeHelpers.GetUninitializedObject( typeof( Label ) );
    }
}
