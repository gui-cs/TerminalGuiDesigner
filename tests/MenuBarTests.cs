using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using TerminalGuiDesigner.Operations.MenuOperations;
using TerminalGuiDesigner.UI.Windows;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( OperationManager ) )]
[Category( "UI" )]
internal class MenuBarTests : Tests
{
    [Test]
    [TestOf( typeof( RemoveMenuItemOperation ) )]
    public void DeletingLastMenuItem_ShouldRemoveWholeBar( )
    {
        using MenuBar bar = GetMenuBar( out Design root );

        MenuItem mi = bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0];

        RemoveMenuItemOperation? removeMenuItemOperation = null;
        Assert.That( ( ) => removeMenuItemOperation = new( App, mi ), Throws.Nothing );
        Assert.That( removeMenuItemOperation, Is.Not.Null.And.InstanceOf<RemoveMenuItemOperation>( ) );

        bool removeMenuItemOperationSucceeded = false;
        Assert.That( ( ) => removeMenuItemOperationSucceeded = OperationManager.Instance.Do( removeMenuItemOperation! ), Throws.Nothing );
        Assert.That( removeMenuItemOperationSucceeded );
        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        } );

        Assert.Multiple( ( ) =>
        {
            Assert.That( bar.SubViews.OfType<MenuBarItem>(), Is.Empty );
            Assert.That( root.View.SubViews, Has.None.InstanceOf<MenuBar>( ) );
        } );

        Assert.That( OperationManager.Instance.Undo, Throws.Nothing );
        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );
            // TODO: This needs to clean up after itself in a safe fashion
        } );

        // Same conditions as at the start
        // The MenuBar should be back in the root view...
        Assert.That( root.View.SubViews, Has.Exactly( 1 ).InstanceOf<MenuBar>( ) );
        Assert.That( root.View.SubViews.ElementAt(0), Is.Not.Null.And.SameAs( bar ) );

        // ...And the original MenuBar should be back as it was at the start.
        Assert.That( bar.SubViews.OfType<MenuBarItem>(), Is.Not.Empty );
        Assert.That( bar.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
        Assert.That( bar.SubViews.OfType<MenuBarItem>().First(), Is.Not.Null.And.InstanceOf<MenuBarItem>( ) );
        Assert.That( bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        Assert.That( bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0], Is.Not.Null.And.InstanceOf<MenuItem>( ) );
    }

    [Test]
    [TestOf( typeof( RemoveMenuItemOperation ) )]
    public void DeletingMenuItemFromSubmenu_AllSubmenuChild( )
    {
        using MenuBarWithSubmenuItems m = GetMenuBarWithSubmenuItems( );

        MenuItem? bottomChild = m.Head2.GetMenuItems(out _)[1];

        Assume.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );
        Assume.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[1], Is.Not.Null.And.InstanceOf<MenuBarItem>( ) );
        Assume.That( m.Head2.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assume.That( m.Head2.GetMenuItems(out _)[0], Is.SameAs( m.TopChild ) );

        RemoveMenuItemOperation cmd1 = new( App, m.TopChild );
        Assert.That( cmd1.Do, Throws.Nothing );

        RemoveMenuItemOperation cmd2 = new( App, bottomChild );
        Assert.That( cmd2.Do, Throws.Nothing );

        // Deleting both children should convert us from
        // a dropdown submenu to just a regular MenuItem
        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );
        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[1], Is.Not.Null.And.InstanceOf<MenuItem>( ) );

        Assert.That( cmd2.Undo, Throws.Nothing );

        // should bring the bottom one back
        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );
        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[1], Is.Not.Null.And.InstanceOf<MenuBarItem>( ) );
        Assert.That( ( (MenuBarItem)m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[1] ).GetMenuItems(out _)[0], Is.SameAs( bottomChild ) );

        Assert.That( cmd1.Undo, Throws.Nothing );

        // Both submenu items should now be back
        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[1], Is.Not.Null.And.InstanceOf<MenuBarItem>( ) );
            Assert.That( m.Head2.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        } );
        Assert.Multiple( ( ) =>
        {
            Assert.That( m.Head2.GetMenuItems(out _)[0], Is.SameAs( m.TopChild ) );
            Assert.That( ( (MenuBarItem)m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[1] ).GetMenuItems(out _)[0], Is.SameAs( m.TopChild ) );
            Assert.That( ( (MenuBarItem)m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[1] ).GetMenuItems(out _)[1], Is.SameAs( bottomChild ) );
        } );
    }

    [Test]
    [TestOf( typeof( RemoveMenuItemOperation ) )]
    public void DeletingMenuItemFromSubmenu_TopChild( )
    {
        using MenuBarWithSubmenuItems m = GetMenuBarWithSubmenuItems( );

        RemoveMenuItemOperation cmd = new ( App, m.TopChild );
        bool cmdSucceeded = false;
        Assert.That( ( ) => cmdSucceeded = cmd.Do( ), Throws.Nothing );
        Assert.That( cmdSucceeded );

        // Delete the top child should leave only 1 in submenu
        Assert.Multiple( ( ) =>
        {
            Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>(  ) );
            Assert.That( m.Head2.GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>(  ) );
            Assert.That( m.Head2.GetMenuItems(out _)[0], Is.Not.SameAs( m.TopChild ) );
        } );

        Assert.That( cmd.Undo, Throws.Nothing );

        Assert.Multiple( ( ) =>
        {
            // should come back now
            Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );
            Assert.That( m.Head2.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
            Assert.That( m.Head2.GetMenuItems(out _)[0], Is.SameAs( m.TopChild ) );
        } );
    }

    [Test]
    [TestOf( typeof( MenuBarTests ) )]
    [Category( "Change Control" )]
    [Order( 1 )]
    [Repeat( 10 )]
    [Description( "Ensures that the GetMenuBar helper method returns the expected objects and doesn't fail even if used multiple times." )]
    public void GetMenuBar_BehavesAsExpected( )
    {
        using MenuBar bar = GetMenuBar( out Design root );
        Assert.Multiple( ( ) =>
        {
            Assert.That( bar, Is.Not.Null.And.InstanceOf<MenuBar>( ) );
            Assert.That( root, Is.Not.Null.And.InstanceOf<Design>( ) );
        } );
        Assert.That( root.View.SubViews, Has.Exactly( 1 ).InstanceOf<MenuBar>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( root.View.SubViews.ElementAt(0), Is.Not.Null.And.SameAs( bar ) );
            Assert.That( bar.SubViews.OfType<MenuBarItem>(), Is.Not.Empty );
        } );
        Assert.That( bar.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
        Assert.That( bar.SubViews.OfType<MenuBarItem>().First(), Is.Not.Null.And.InstanceOf<MenuBarItem>( ) );
        Assert.That( bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0], Is.Not.Null.And.InstanceOf<MenuItem>( ) );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        } );
    }

    [Test]
    [TestOf( typeof( MenuBarWithSubmenuItems ) )]
    [Category( "Change Control" )]
    [Order( 2 )]
    [Repeat( 10 )]
    [Description( "Ensures that the GetMenuBarWithSubmenuItems helper method returns the expected objects and doesn't fail even if used multiple times." )]
    public void GetMenuBarWithSubmenuItems_BehavesAsExpected( )
    {
        using MenuBarWithSubmenuItems m = GetMenuBarWithSubmenuItems( );

        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );

        MenuBarItem menu0 = m.Bar.SubViews.OfType<MenuBarItem>().First();
        Assert.That( menu0.GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );

        // First item
        MenuItem menu0Child0 = menu0.GetMenuItems(out _)[0];
        Assert.That( menu0Child0.Title, Is.EqualTo( "Head1" ) );

        // Second item and its children
        Assert.That( menu0.GetMenuItems(out _)[1], Is.Not.Null.And.InstanceOf<MenuBarItem>( ) );
        MenuBarItem menu0Child1 = (MenuBarItem)menu0.GetMenuItems(out _)[1];
        Assert.Multiple( ( ) =>
        {
            Assert.That( menu0Child1.Title, Is.EqualTo( "Head2" ) );
            Assert.That( menu0Child1.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        } );
        MenuItem menu0Child1Leaf0 = menu0Child1.GetMenuItems(out _)[0];
        MenuItem menu0Child1Leaf1 = menu0Child1.GetMenuItems(out _)[1];
        Assert.Multiple( ( ) =>
        {
            Assert.That( menu0Child1Leaf0.Title, Is.EqualTo( "Child1" ) );
            Assert.That( menu0Child1Leaf0.Key, Is.EqualTo( Key.J.WithCtrl ) );
            Assert.That( menu0Child1Leaf1.Title, Is.EqualTo( "Child2" ) );
            Assert.That( menu0Child1Leaf1.Key, Is.EqualTo( Key.F.WithCtrl ) );
        } );

        // Third item
        Assert.That( menu0.GetMenuItems(out _)[2], Is.Not.Null.And.InstanceOf<MenuItem>( ) );
        MenuItem menu0Child2 = menu0.GetMenuItems(out _)[2];
        Assert.That( menu0Child2.Title, Is.EqualTo( "Head3" ) );

        //Now just make sure the record properties were set to the right references
        Assert.Multiple( ( ) =>
        {
            Assert.That( m.Head2, Is.SameAs( menu0Child1 ) );
            Assert.That( m.TopChild, Is.SameAs( menu0Child1Leaf0 ) );
        } );
    }

    [Test]
    [TestOf( typeof( MoveMenuItemLeftOperation ) )]
    public void MoveMenuItemLeft_CannotMoveRootItems( )
    {
        using MenuBar bar = GetMenuBar( );

        // cannot move a root item
        MoveMenuItemLeftOperation moveMenuItemLeftOperation = new( App, bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0] );
        Assert.That( moveMenuItemLeftOperation.IsImpossible );
        bool moveMenuItemLeftOperationSucceeded = false;
        Assert.That( ( ) => moveMenuItemLeftOperationSucceeded = moveMenuItemLeftOperation.Do( ), Throws.Nothing );
        Assert.That( moveMenuItemLeftOperationSucceeded, Is.False );
    }

    [Test]
    public void MoveMenuItemLeft_MoveTopChild( )
    {
        using MenuBarWithSubmenuItems m = GetMenuBarWithSubmenuItems( );

        MoveMenuItemLeftOperation moveMenuItemLeftOperation = new ( App, m.TopChild );
        Assert.That( moveMenuItemLeftOperation.IsImpossible, Is.False );
        bool moveMenuItemLeftOperationSucceeded = false;
        Assert.That( ( ) => moveMenuItemLeftOperationSucceeded = moveMenuItemLeftOperation.Do( ), Throws.Nothing );
        Assert.That( moveMenuItemLeftOperationSucceeded );

        // move the top child left should pull
        // it out of the submenu and onto the root
        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 4 ).InstanceOf<MenuItem>( ) );
        Assert.That( m.Head2.GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        Assert.That( m.Head2.GetMenuItems(out _)[0], Is.Not.SameAs( m.TopChild ) );

        // it should be pulled out underneath its parent
        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[2], Is.SameAs( m.TopChild ) );

        // undoing command should return us to previous state
        Assert.That( moveMenuItemLeftOperation.Undo, Throws.Nothing );

        Assert.That( m.Bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );
        Assert.That( m.Head2.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.That( m.Head2.GetMenuItems(out _)[0], Is.SameAs( m.TopChild ) );
    }

    [Test]
    [TestOf( typeof( AddMenuItemOperation ) )]
    public void MoveMenuItemRight_CannotMoveElementZero( )
    {
        using MenuBar bar = GetMenuBar( );

        var fileMenu = bar.SubViews.OfType<MenuBarItem>().First();
        MenuItem? mi = fileMenu.GetMenuItems(out _)[0];
        mi.Data = "yarg";
        mi.Key = Key.Y.WithCtrl;

        AddMenuItemOperation addAnother = new( App, mi );
        Assert.That( addAnother.IsImpossible, Is.False );
        bool addAnotherSucceeded = false;
        Assert.That( ( ) => addAnotherSucceeded = addAnother.Do( ), Throws.Nothing );
        Assert.That( addAnotherSucceeded );

        // should have added below us
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.SameAs( mi ) );
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.Not.SameAs( mi ) );
        } );

        // cannot move element 0
        MoveMenuItemRightOperation impossibleMoveRightOp = new( App, fileMenu.GetMenuItems(out _)[0] );
        Assert.That( impossibleMoveRightOp.IsImpossible );
        bool impossibleMoveRightOpSucceeded = false;
        Assert.That( ( ) => impossibleMoveRightOpSucceeded = impossibleMoveRightOp.Do( ), Throws.Nothing );
        Assert.That( impossibleMoveRightOpSucceeded, Is.False );

        // can move element 1
        MoveMenuItemRightOperation validMoveRightOp = new( App, fileMenu.GetMenuItems(out _)[1] );
        Assert.That( validMoveRightOp.IsImpossible, Is.False );
        bool validMoveRightOpSucceeded = false;
        Assert.That( ( ) => validMoveRightOpSucceeded = validMoveRightOp.Do( ), Throws.Nothing );
        Assert.That( validMoveRightOpSucceeded );

        // In Terminal.Gui v2, nested items with sub-menus remain plain MenuItem instances
        // (with SubMenu set) rather than being converted to MenuBarItem.  MenuBarItem is
        // exclusively for top-level MenuBar entries.  The same object reference (mi) is
        // retained at index 0 — it is not replaced with a new object.
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        MenuItem miWithSubMenu = fileMenu.GetMenuItems(out _)[0];

        Assert.Multiple( ( ) =>
        {
            Assert.That( miWithSubMenu, Is.SameAs( mi ) );
            Assert.That( miWithSubMenu.Title, Is.EqualTo( mi.Title ) );
            Assert.That( miWithSubMenu.Data, Is.EqualTo( mi.Data ) );
            Assert.That( miWithSubMenu.GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        } );

        // Now undo it.
        Assert.That( validMoveRightOp.Undo, Throws.Nothing );

        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.Null.And.InstanceOf<MenuItem>( ) );
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.Not.Null.And.InstanceOf<MenuItem>( ) );
        } );
        MenuItem firstChildAfterUndo = fileMenu.GetMenuItems(out _)[0];

        Assert.Multiple( ( ) =>
        {
            // The same object reference is retained after undo.
            Assert.That( firstChildAfterUndo, Is.SameAs( mi ) );

            // The sub-menu should have been cleared after undo.
            Assert.That( firstChildAfterUndo.GetMenuItems(out _), Is.Empty );

            // Values need to be preserved.
            Assert.That( firstChildAfterUndo.Title, Is.EqualTo( mi.Title ) );
            Assert.That( firstChildAfterUndo.Data, Is.EqualTo( mi.Data ) );
            Assert.That( firstChildAfterUndo.Key, Is.EqualTo( mi.Key) );
        } );
    }

    /// <summary>
    /// Tests that when there is only one menu item
    /// that it cannot be moved into a submenu
    /// </summary>
    [Test]
    [TestOf( typeof( MoveMenuItemRightOperation ) )]
    public void MoveMenuItemRight_CannotMoveLast( )
    {
        MenuBar bar = GetMenuBar( );

        MenuItem? mi = bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0];
        MoveMenuItemRightOperation cmd = new( App, mi );
        Assert.That( cmd.IsImpossible );
        Assert.That( cmd.Do, Is.False );
    }

    /// <summary>
    /// Tests removing the last menu item (i.e. 'Do Something')
    /// under the only remaining menu header (e.g. 'File F9')
    /// should result in a completely empty menu bar and be undoable
    /// </summary>
    [Test]
    [TestOf( typeof( RemoveMenuItemOperation ) )]
    public void RemoveFinalMenuItemOnBar( )
    {
        using MenuBar bar = GetMenuBar( );

        MenuBarItem? fileMenu = bar.SubViews.OfType<MenuBarItem>().First();
        MenuItem? placeholderMenuItem = fileMenu.GetMenuItems(out _)[0];

        RemoveMenuItemOperation removeOp = new( App, placeholderMenuItem );

        // we are able to remove the last one
        Assert.That( removeOp.IsImpossible, Is.False );
        bool removeOpSucceeded = false;
        Assert.That( ( ) => removeOpSucceeded = removeOp.Do( ), Throws.Nothing );
        Assert.That( removeOpSucceeded );
        Assert.That( bar.SubViews.OfType<MenuBarItem>(), Is.Empty );

        Assert.That( removeOp.Undo, Throws.Nothing );

        // should be back to where we started
        Assert.That( bar.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
        Assert.That( bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        Assert.That( bar.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0], Is.SameAs( placeholderMenuItem ) );
    }

    [Test]
    public void RoundTrip_PreserveMenuItems()
    {
        var viewToCode = new ViewToCode(App);

        var file = new FileInfo($"{nameof(RoundTrip_PreserveMenuItems)}.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        using MenuBar mbOut = ViewFactory.Create<MenuBar>( );

        Assume.That( mbOut, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

        // 1 visible root menu (e.g. File)
        Assert.That( mbOut.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );

        // 1 child menu item (e.g. Open)
        Assert.That( mbOut.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Is.Not.Empty );
        Assert.That( mbOut.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly(1).InstanceOf<MenuItem>(  ) );

        AddViewOperation? addViewOperation = null;
        Assume.That( ( ) => addViewOperation = new ( App, mbOut, designOut, "myMenuBar" ), Throws.Nothing );
        Assume.That( addViewOperation, Is.Not.Null.And.InstanceOf<AddViewOperation>( ) );

        bool addViewOperationSucceeded = false;
        Assert.That( ( ) => addViewOperationSucceeded = OperationManager.Instance.Do( addViewOperation! ), Throws.Nothing );
        Assert.That( addViewOperationSucceeded );

        Assume.That( ( ) => viewToCode.GenerateDesignerCs( designOut, typeof( Dialog ) ), Throws.Nothing );

        CodeToView? codeToView = null;
        Assert.That( ( ) => codeToView = new( App, designOut.SourceCode ), Throws.Nothing );
        Assert.That( codeToView, Is.Not.Null.And.InstanceOf<CodeToView>( ) );

        Design? designBackIn = null;
        Assert.That( ( ) => designBackIn = codeToView!.CreateInstance( ), Throws.Nothing );
        Assert.That( designBackIn, Is.Not.Null.And.InstanceOf<Design>( ) );

        // 1 visible root menu (e.g. File)
        MenuBar? mbIn = null;
        Assert.That( designBackIn!.View, Is.Not.Null.And.InstanceOf<View>( ) );
        IList<View> actualSubviews = designBackIn.View.GetActualSubviews().ToList();
        Assert.That( actualSubviews, Has.Exactly( 1 ).InstanceOf<MenuBar>( ) );
        Assert.That( ( ) => mbIn = actualSubviews.OfType<MenuBar>(  ).Single( ), Throws.Nothing );
        Assert.That( mbIn, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

        // 1 child menu item (e.g. Open)
        Assert.That( mbIn!.SubViews.OfType<MenuBarItem>(), Is.Not.Empty );
        Assert.That( mbIn.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
        Assert.That( mbIn.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        Assert.That( mbIn.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0].Title,
            Is.EqualTo( mbOut.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0].Title ) );
    }

    [Test]
    [TestOf( typeof( AddMenuItemOperation ) )]
    public void RoundTrip_PreserveMenuItems_WithSubmenus( )
    {
        ViewToCode viewToCode = new( App );

        FileInfo file = new( $"{nameof( RoundTrip_PreserveMenuItems_WithSubmenus )}.cs" );
        Design designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Dialog ) );

        MenuBar mbOut = ViewFactory.Create<MenuBar>( );
        Assume.That( mbOut, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

        AddViewOperation addViewOperation = new( App, mbOut, designOut, "myMenuBar" );
        Assume.That( addViewOperation, Is.Not.Null.And.InstanceOf<AddViewOperation>( ) );

        bool addViewOperationSucceeded = false;
        Assert.That( ( ) => addViewOperationSucceeded = OperationManager.Instance.Do( addViewOperation ), Throws.Nothing );
        Assert.That( addViewOperationSucceeded );

        // create some more children in the menu
        var firstItem = mbOut.SubViews.OfType<MenuBarItem>().First().GetMenuItems(out _)[0];
        AddMenuItemOperation? addChildMenuOperation1 = null;
        AddMenuItemOperation? addChildMenuOperation2 = null;
        AddMenuItemOperation? addChildMenuOperation3 = null;
        Assert.That( ( ) => addChildMenuOperation1 = new( App, firstItem ), Throws.Nothing );
        Assert.That( ( ) => addChildMenuOperation2 = new( App, firstItem ), Throws.Nothing );
        Assert.That( ( ) => addChildMenuOperation3 = new( App, firstItem ), Throws.Nothing );

        Assert.Multiple( ( ) =>
        {
            bool addChildMenuOperation1Succeeded = false;
            bool addChildMenuOperation2Succeeded = false;
            bool addChildMenuOperation3Succeeded = false;
            Assert.That( ( ) => addChildMenuOperation1Succeeded = addChildMenuOperation1!.Do( ), Throws.Nothing );
            Assert.That( addChildMenuOperation1Succeeded );
            Assert.That( ( ) => addChildMenuOperation2Succeeded = addChildMenuOperation2!.Do( ), Throws.Nothing );
            Assert.That( addChildMenuOperation2Succeeded );
            Assert.That( ( ) => addChildMenuOperation3Succeeded = addChildMenuOperation3!.Do( ), Throws.Nothing );
            Assert.That( addChildMenuOperation3Succeeded );
        } );

        // move the last child
        var fileMenu = mbOut.SubViews.OfType<MenuBarItem>().First();
        MoveMenuItemRightOperation moveMenuItemOperation = new( App, fileMenu.GetMenuItems(out _)[1] );
        Assert.That( ( ) => moveMenuItemOperation.Do( ), Throws.Nothing );

        // 1 visible root menu (e.g. File)
        Assert.That( mbOut.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );

        // 3 child menu item (original one + 3 we added -1 because we moved it to submenu)
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );

        // should be 1 submenu item (the one we moved)
        Assert.That( fileMenu.GetMenuItems(out _)[0], Is.InstanceOf<MenuItem>( ) );
        Assert.That( ( (MenuItem)fileMenu.GetMenuItems(out _)[0] ).GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );

        Assume.That( ( ) => viewToCode.GenerateDesignerCs( designOut, typeof( Dialog ) ), Throws.Nothing );
        Assume.That( designOut, Is.Not.Null.And.InstanceOf<Design>( ) );

        CodeToView? codeToView = null;
        Assume.That( ( ) => codeToView = new( App, designOut.SourceCode ), Throws.Nothing );
        Assume.That( codeToView, Is.Not.Null.And.InstanceOf<CodeToView>( ) );

        Design? designBackIn = null;

        Assume.That( ( ) => designBackIn = codeToView!.CreateInstance( ), Throws.Nothing );
        Assume.That( designBackIn, Is.Not.Null.And.InstanceOf<Design>( ) );

        MenuBar? mbIn = null;
        Assume.That( designBackIn!.View, Is.Not.Null.And.InstanceOf<View>( ) );

        IList<View> actualSubviews = designBackIn.View.GetActualSubviews( ).ToList();
        Assert.That( actualSubviews, Has.Exactly( 1 ).InstanceOf<MenuBar>( ) );
        Assert.That( ( ) => mbIn = actualSubviews.OfType<MenuBar>( ).Single( ), Throws.Nothing );
        Assert.That( mbIn, Is.Not.Null.And.InstanceOf<MenuBar>( ) );

        // 1 visible root menu (e.g. File)
        Assert.That( mbIn!.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
        var mbInFileMenu = mbIn.SubViews.OfType<MenuBarItem>().First();
        // 3 child menu item (original one + 3 we added -1 because we moved it to submenu)
        Assert.That( mbInFileMenu.GetMenuItems(out _), Has.Exactly( 3 ).InstanceOf<MenuItem>( ) );
        Assert.That( mbInFileMenu.GetMenuItems(out _), Has.All.Not.Null );

        // should be 1 submenu item (the one we moved)
        Assert.That( ( (MenuItem)mbInFileMenu.GetMenuItems(out _)[0] ).GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        Assert.That(
            ( (MenuItem)mbInFileMenu.GetMenuItems(out _)[0] ).GetMenuItems(out _)[0].Title,
            Is.EqualTo( ( (MenuItem)fileMenu.GetMenuItems(out _)[0] ).GetMenuItems(out _)[0].Title ) );
    }

    [Test]
    [TestOf( typeof( MenuTracker ) )]
    // TODO: Break this one up into smaller units at some point.
    public void TestMenuOperations( )
    {
        ViewToCode viewToCode = new( App );

        FileInfo file = new( $"{nameof( TestMenuOperations )}.cs" );
        Design designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Dialog ) );
        Assume.That( designOut, Is.Not.Null.And.InstanceOf<Design>( ) );
        Assume.That( designOut.View, Is.Not.Null.And.InstanceOf<Dialog>( ) );

        using MenuBar mbOut = ViewFactory.Create<MenuBar>( );
        Assume.That( mbOut, Is.Not.Null.And.InstanceOf<MenuBar>( ) );
        Assume.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        Assume.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        Assert.Warn( "MenuTracker.Instance.CurrentlyOpenMenuItem cannot be guaranteed null at this time. See https://github.com/tui-cs/TerminalGuiDesigner/issues/270" );
        // TODO: Enable this pre-condition once MenuTracker changes are implemented.
        // See https://github.com/tui-cs/TerminalGuiDesigner/issues/270
        //Assume.That( MenuTracker.Instance.CurrentlyOpenMenuItem, Is.Null );

        MenuTracker.Instance.Register( mbOut );

        var fileMenu = mbOut.SubViews.OfType<MenuBarItem>().First();

        // 1 visible root menu (e.g. File)
        Assert.That( mbOut.SubViews.OfType<MenuBarItem>(), Has.Exactly( 1 ).InstanceOf<MenuBarItem>( ) );
        // 1 child menu item (e.g. Open)
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );

        MenuItem? orig = fileMenu.GetMenuItems(out _)[0];
        Assert.That( orig, Is.Not.Null.And.InstanceOf<MenuItem>( ) );

        AddMenuItemOperation? addMenuItemOperation = null;
        Assert.That( ( ) => addMenuItemOperation = new( App, fileMenu.GetMenuItems(out _)[0] ), Throws.Nothing );
        Assert.That( addMenuItemOperation, Is.Not.Null.And.InstanceOf<AddMenuItemOperation>( ) );

        bool addMenuItemOperationSucceeded = false;
        Assert.That( ( ) => addMenuItemOperationSucceeded = OperationManager.Instance.Do( addMenuItemOperation! ), Throws.Nothing );
        Assert.That( addMenuItemOperationSucceeded );

        // Now 2 child menu item
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.Null.And.SameAs( orig ) ); // original is still at top
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.Not.Null.And.Not.SameAs( orig ) );
        } );

        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        } );

        OperationManager.Instance.Undo();

        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );
        } );

        // Now only 1 child menu item
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 1 ).InstanceOf<MenuItem>( ) );
        Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.Null.And.SameAs( orig ) ); // original is still at top

        OperationManager.Instance.Redo();

        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        } );

        // Now 2 child menu items again
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.Null.And.SameAs( orig ) );     // original is still at top
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.Not.Null.And.Not.SameAs( orig ) ); // original is still at top
        } );

        // Now test moving an item around
        MenuItem? toMove = fileMenu.GetMenuItems(out _)[1];
        Assume.That( toMove, Is.Not.Null.And.InstanceOf<MenuItem>( ) );

        // Move second menu item up
        MoveMenuItemOperation? up = null;
        Assert.That( ( ) => up = new( App, toMove, true ), Throws.Nothing );
        Assert.That( up, Is.Not.Null.And.InstanceOf<MoveMenuItemOperation>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( up!.Bar, Is.SameAs( mbOut ) );
            Assert.That( up.IsImpossible, Is.False );
        } );

        bool moveUpSucceeded = false;
        Assert.That( ( ) => moveUpSucceeded = OperationManager.Instance.Do( up! ), Throws.Nothing );
        Assert.That( moveUpSucceeded );

        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 2 ) );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        } );

        // Original one should now be bottom
        Assume.That( orig, Is.Not.SameAs( toMove ) );
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.Null.And.Not.SameAs( orig ) );
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.Null.And.SameAs( toMove ) );
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.SameAs( orig ) );
        } );

        // can't move top one up
        MoveMenuItemOperation? impossibleMoveUpOperation = null;
        Assert.That( ( ) => impossibleMoveUpOperation = new( App, toMove, true ), Throws.Nothing );
        Assert.That( impossibleMoveUpOperation, Is.Not.Null.And.InstanceOf<MoveMenuItemOperation>( ) );
        Assert.That( impossibleMoveUpOperation!.IsImpossible );

        // cant move bottom one down
        MoveMenuItemOperation? impossibleMoveDownOperation = null;
        Assert.That( ( ) => impossibleMoveDownOperation = new( App, fileMenu.GetMenuItems(out _)[1], false ), Throws.Nothing );
        Assert.That( impossibleMoveDownOperation, Is.Not.Null.And.InstanceOf<MoveMenuItemOperation>( ) );
        Assert.That( impossibleMoveDownOperation!.IsImpossible );

        Assert.That( static ( ) => OperationManager.Instance.Undo( ), Throws.Nothing );

        // Original one should be back on top
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.SameAs( orig ) );
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.SameAs( toMove ) );
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.SameAs( toMove ) );
        } );

        // test moving the top one down
        MenuItem? toMove2 = fileMenu.GetMenuItems(out _)[1];

        // Move first menu item down
        MoveMenuItemOperation? down = null;
        Assert.That( ( ) => down = new( App, toMove2, true ), Throws.Nothing );
        Assert.That( down, Is.Not.Null.And.InstanceOf<MoveMenuItemOperation>( ) );
        Assert.That( down!.IsImpossible, Is.False );

        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        } );

        bool moveDownSucceeded = false;
        Assert.That( ( ) => moveDownSucceeded = OperationManager.Instance.Do( down ), Throws.Nothing );
        Assert.That( moveDownSucceeded );
        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 2 ) );
        } );

        // Original one should now be bottom
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.SameAs( toMove2 ) );
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.SameAs( orig ) );
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.SameAs( orig ) );
        } );

        Assert.That( static ( ) => OperationManager.Instance.Undo( ), Throws.Nothing );

        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        } );


        // should be back to how we started now
        Assert.That( fileMenu.GetMenuItems(out _), Has.Exactly( 2 ).InstanceOf<MenuItem>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.SameAs( orig ) );
            Assert.That( fileMenu.GetMenuItems(out _)[0], Is.Not.SameAs( toMove2 ) );
            Assert.That( fileMenu.GetMenuItems(out _)[1], Is.SameAs( toMove2 ) );
        } );

        MenuTracker.Instance.UnregisterMenuBar( mbOut );
    }

    [TestCase(KeyCode.ShiftMask,false)]
    [TestCase(KeyCode.AltMask, false)]
    [TestCase(KeyCode.CtrlMask, false)]
    [TestCase(KeyCode.ShiftMask | KeyCode.CtrlMask, false)]
    [TestCase(KeyCode.ShiftMask | KeyCode.AltMask, false)]
    [TestCase(KeyCode.CtrlMask | KeyCode.AltMask, false)]
    [TestCase(KeyCode.ShiftMask | KeyCode.CtrlMask | KeyCode.AltMask, false)]
    [TestCase(KeyCode.CtrlMask | KeyCode.C, true)]
    [TestCase(KeyCode.ShiftMask | KeyCode.CtrlMask | KeyCode.AltMask | KeyCode.C, true)]

    public void TestShortcutIsValid(KeyCode k,bool expected)
    {
        Assert.That(Modals.IsValidShortcut(k),Is.EqualTo(expected));
    }

    private MenuBar GetMenuBar( )
    {
        return GetMenuBar( out _ );
    }

    private MenuBar GetMenuBar( out Design root )
    {
        root = Get10By10View( );

        var bar = ViewFactory.Create<MenuBar>( );
        var addBarCmd = new AddViewOperation( App, bar, root, "mb" );
        addBarCmd.Do( );

        return bar;
    }

    private MenuBarWithSubmenuItems GetMenuBarWithSubmenuItems( )
    {
        MenuBarWithSubmenuItems toReturn = new( GetMenuBar( ), null!, null! )
        {
            Bar = GetMenuBar( )
        };
        // Set up a menu like:

        /*
           File
            Head1
            Head2 -> Child1
            Head3    Child2
        */

        var fileMenu = toReturn.Bar.SubViews.OfType<MenuBarItem>().First();
        var mi = fileMenu.GetMenuItems(out _)[0];
        mi.Title = "Head1";

        toReturn.Head2 = CreateHead2Item();
        fileMenu.SetMenuItems(new List<MenuItem>
        {
            fileMenu.GetMenuItems(out _)[0],
            toReturn.Head2,
            new( "Head3", null, static ( ) => { } ),
        });

        return toReturn;

        MenuBarItem CreateHead2Item( )
        {
            toReturn.TopChild = CreateHead2Child1Item( );
            return new MenuBarItem( "Head2", new MenuItem[] { toReturn.TopChild, CreateHead2Child2Item( ) } );

            static MenuItem CreateHead2Child1Item( )
            {
                return new( "Child1", null, static ( ) => { } )
                {
                    Data = "Child1",
                    Key = Key.J.WithCtrl,
                };
            }

            static MenuItem CreateHead2Child2Item( )
            {
                return new( "Child2", null, static ( ) => { } )
                {
                    Data = "Child2",
                    Key = Key.F.WithCtrl,
                };
            }
        }
    }

    private sealed record MenuBarWithSubmenuItems( MenuBar Bar, MenuBarItem Head2, MenuItem TopChild ) : IDisposable
    {
        public MenuBarItem Head2 { get; set; } = Head2;
        public MenuItem TopChild { get; set; } = TopChild;

        /// <inheritdoc />
        public void Dispose( )
        {
            Bar.Dispose( );
        }
    }
}
