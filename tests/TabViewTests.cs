using TerminalGuiDesigner.Operations.TabOperations;

namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
internal class TabViewTests : Tests
{
    [Test]
    public void RoundTrip_PreserveTabs( )
    {
        using TabView tabIn =
            RoundTrip<Dialog, TabView>(
                static ( _, t ) =>
                {
                    Assume.That( t.Tabs, Is.Not.Empty, "Expected default TabView created by ViewFactory to have some placeholder Tabs" );
                    Assume.That( t.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );
                    Assume.That( t.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab1" ) );
                    Assume.That( t.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab2" ) );
                },
                out TabView tabOut );

        Assert.That( tabIn.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tabIn.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab1" ) );
            Assert.That( tabIn.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab2" ) );
            Assert.That( tabOut.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab1" ) );
            Assert.That( tabOut.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab2" ) );

            // Also prove they aren't the same objects
            Assert.That( tabIn, Is.Not.SameAs( tabOut ) );
            Assert.That( tabIn.Tabs, Has.ItemAt( 0 ).Not.SameAs( tabOut.Tabs.ElementAt( 0 ) ) );
            Assert.That( tabIn.Tabs, Has.ItemAt( 1 ).Not.SameAs( tabOut.Tabs.ElementAt( 1 ) ) );
        } );
        tabOut.Dispose( );
    }

    /// <summary>
    /// Creates a Dialog with a <see cref="TabView"/> in it.  Returns the <see cref="Design"/>
    /// </summary>
    /// <returns></returns>
    private static Design GetTabView()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestGetTabView.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var tvOut = ViewFactory.Create<TabView>( );

        AddViewOperation? addViewOperation = new (tvOut, designOut, "myTabview");
        Assume.That( addViewOperation, Is.Not.Null.And.InstanceOf<AddViewOperation>( ) );
        Assume.That( addViewOperation.IsImpossible, Is.False );
        Assume.That( addViewOperation.SupportsUndo );

        bool addViewOperationSucceeded = false;
        Assume.That( ( ) => addViewOperationSucceeded = OperationManager.Instance.Do( addViewOperation ), Throws.Nothing );
        Assume.That( addViewOperationSucceeded );

        // The above operation interferes with expected results in tests,
        // so let's clear it out.
        OperationManager.Instance.ClearUndoRedo( );

        return (Design)tvOut.Data;
    }

    [Test]
    [Category( "UI" )]
    [TestOf( typeof( MoveTabOperation ) )]
    public void ChangeTabViewOrder_MoveTabLeft( )
    {
        Design d = GetTabView( );
        using TabView tv = (TabView)d.View;

        Assume.That( tv.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );
        Assume.That( tv.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab1" ) );
        Assume.That( tv.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab2" ) );
        Assume.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        Assume.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        // Select Tab1
        tv.SelectedTab = tv.Tabs.First( );

        // try to move tab 1 left
        MoveTabOperation cmd = new( d, tv.SelectedTab, -1 );
        Assert.That( cmd.IsImpossible );
        Assert.That( cmd.SupportsUndo );
        bool cmdSucceeded = false;
        Assert.That( ( ) => cmdSucceeded = OperationManager.Instance.Do( cmd ), Throws.Nothing );
        Assert.Multiple( ( ) =>
        {
            Assert.That( cmdSucceeded, Is.False, "Expected not to be able to move tab left because selected is the first" );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        } );

        // Select Tab2
        tv.SelectedTab = tv.Tabs.Last( );

        Assert.That( tv.SelectedTab.Text, Is.EqualTo( "Tab2" ), "Tab2 should be selected before operation is applied" );

        // try to move tab 2 left
        MoveTabOperation cmd2 = new( d, tv.SelectedTab, -1 );
        Assert.Multiple( ( ) =>
        {
            Assert.That( cmd2.IsImpossible, Is.False );
            Assert.That( cmd2.SupportsUndo );
        } );
        bool cmd2Succeeded = false;
        Assert.That( ( ) => cmd2Succeeded = OperationManager.Instance.Do( cmd2 ), Throws.Nothing );
        Assert.Multiple( ( ) =>
        {
            Assert.That( cmd2Succeeded );
            Assert.That( tv.SelectedTab.Text, Is.EqualTo( "Tab2" ), "Tab2 should still be selected after operation is applied" );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        } );

        // tabs should now be in reverse order
        Assert.That( tv.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab2" ) );
            Assert.That( tv.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab1" ) );
        } );

        Assert.That( OperationManager.Instance.Undo, Throws.Nothing );
        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        } );

        // undoing command should revert to original tab order
        Assert.That( tv.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab1" ) );
            Assert.That( tv.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab2" ) );
        } );

        // Now let's redo it
        Assert.That( OperationManager.Instance.Redo, Throws.Nothing );
        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        } );

        // Redoing command should put the tabs back in reversed order
        Assert.That( tv.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab2" ) );
            Assert.That( tv.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab1" ) );
        } );
    }

    [Test]
    [Category( "UI" )]
    [TestOf( typeof( RemoveTabOperation ) )]
    public void RemoveTabOperation( )
    {
        Design d = GetTabView( );
        using TabView tv = (TabView)d.View;

        Assume.That( tv.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );
        Assume.That( tv.Tabs, Has.ItemAt( 0 ).Property( "Text" ).EqualTo( "Tab1" ) );
        Assume.That( tv.Tabs, Has.ItemAt( 1 ).Property( "Text" ).EqualTo( "Tab2" ) );
        Assume.That( OperationManager.Instance.RedoStackSize, Is.Zero );
        Assume.That( OperationManager.Instance.UndoStackSize, Is.Zero );

        Tab tab1 = tv.Tabs.ElementAt( 0 );
        Tab tab2 = tv.Tabs.ElementAt( 1 );
        Assume.That( tab1, Is.Not.SameAs( tab2 ) );

        // Select Tab1
        tv.SelectedTab = tv.Tabs.First( );

        // try to remove the first tab
        RemoveTabOperation removeTab1 = new( d, tv.SelectedTab );
        Assert.That( removeTab1.IsImpossible, Is.False );
        Assert.That( removeTab1.SupportsUndo );
        bool removeTab1Succeeded = false;
        Assert.That( ( ) => removeTab1Succeeded = OperationManager.Instance.Do( removeTab1 ), Throws.Nothing );
        Assert.That( removeTab1Succeeded );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Has.Exactly( 1 ).InstanceOf<Tab>( ) );
            Assert.That( tv.Tabs, Does.Not.Contain( tab1 ) );
            Assert.That( tv.Tabs, Does.Contain( tab2 ) );
            Assert.That( tv.SelectedTab, Is.SameAs( tab2 ) );
        } );

        Assert.Multiple( static ( ) =>
        {
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        } );

        // remove the last tab (tab2)
        RemoveTabOperation removeTab2 = new( d, tv.SelectedTab );
        bool removeTab2Succeeded = false;
        Assert.That( ( ) => removeTab2Succeeded = OperationManager.Instance.Do( removeTab2 ), Throws.Nothing );
        Assert.Multiple( ( ) =>
        {
            Assert.That( removeTab2Succeeded );
            Assert.That( tv.Tabs, Is.Empty );
            Assert.That( tv.SelectedTab, Is.Null );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 2 ) );
        } );

        Assert.That( OperationManager.Instance.Undo, Throws.Nothing );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Has.Exactly( 1 ).InstanceOf<Tab>( ) );
            Assert.That( tv.Tabs, Does.Not.Contain( tab1 ) );
            Assert.That( tv.Tabs, Does.Contain( tab2 ) );
            Assert.That( tv.SelectedTab, Is.SameAs( tab2 ) );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        } );

        Assert.That( OperationManager.Instance.Redo, Throws.Nothing );
        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Is.Empty );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 2 ) );
        } );

        // undo removing both
        Assert.That( OperationManager.Instance.Undo, Throws.Nothing );
        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Has.Exactly( 1 ).InstanceOf<Tab>( ) );
            Assert.That( tv.Tabs, Does.Not.Contain( tab1 ) );
            Assert.That( tv.Tabs, Does.Contain( tab2 ) );
            Assert.That( tv.SelectedTab, Is.SameAs( tab2 ) );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        } );

        Assert.That( OperationManager.Instance.Undo, Throws.Nothing );
        Assert.Multiple( ( ) =>
        {
            Assert.That( tv.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );
            Assert.That( tv.Tabs, Has.ItemAt( 0 ).SameAs( tab1 ) );
            Assert.That( tv.Tabs, Has.ItemAt( 1 ).SameAs( tab2 ) );
            // TODO: Shouldn't tab1 be selected now, after undoing its remove operation?
            Assert.Warn( "Possible unintended behavior in tab selection after undo" );
            // Assert.That( tv.SelectedTab, Is.SameAs( tab1 ) );
            Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 2 ) );
            Assert.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        } );
    }

    [Test]
    public void RoundTrip_DuplicateTabTitles( [Values( "TextTabTitle", "12345", "TextAndNumber5" )] string tabText )
    {
        // TODO: I'm not sure this test is really necessary.
        // Why would the title text be any different from any other property?
        ViewToCode viewToCode = new( );

        FileInfo file = new( $"{nameof( RoundTrip_DuplicateTabTitles )}.cs" );
        Design designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Dialog ) );

        using TabView tvOut = ViewFactory.Create<TabView>( );

        AddViewOperation addOperation = new( tvOut, designOut, "myTabview" );
        Assume.That( addOperation.IsImpossible, Is.False );
        bool addOperationSucceeded = false;
        Assume.That( ( ) => addOperationSucceeded = OperationManager.Instance.Do( addOperation ), Throws.Nothing );
        Assume.That( addOperationSucceeded );

        // Give both tabs the same title text
        tvOut.Tabs.ElementAt( 0 ).Text = tabText;
        tvOut.Tabs.ElementAt( 1 ).Text = tabText;

        viewToCode.GenerateDesignerCs( designOut, typeof( Dialog ) );

        Assume.That( designOut.View.GetActualSubviews( ).OfType<TabView>( ).Single( ), Is.SameAs( tvOut ) );

        CodeToView codeToView = new( designOut.SourceCode );
        Design? designBackIn = null;
        Assert.That( ( ) => designBackIn = codeToView.CreateInstance( ), Throws.Nothing );
        Assert.That( designBackIn, Is.Not.Null.And.InstanceOf<Design>( ) );

        using TabView tvIn = designBackIn!.View.GetActualSubviews( ).OfType<TabView>( ).Single( );

        Assert.That( tvIn.Tabs, Has.Exactly( 2 ).InstanceOf<Tab>( ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tvIn, Is.Not.SameAs( tvOut ) );
            Assert.That( tvIn.Tabs.ElementAt( 0 ).Text, Is.EqualTo( tabText ) );
            Assert.That( tvIn.Tabs.ElementAt( 1 ).Text, Is.EqualTo( tabText ) );
        } );
    }

    private static IEnumerable<View> TabView_Tab_SubViewTypes =>
    [
        (Label)RuntimeHelpers.GetUninitializedObject( typeof(Label) ),
        (Button)RuntimeHelpers.GetUninitializedObject( typeof(Button) ),
        (StatusBar)RuntimeHelpers.GetUninitializedObject( typeof(StatusBar) ),
    ];

    [Test]
    public void AddingSubControlToTab<T>( [ValueSource( nameof( TabView_Tab_SubViewTypes ) )] T dummyObject )
        where T : View, new( )
    {
        ViewToCode viewToCode = new( );

        FileInfo file = new( $"{nameof( AddingSubControlToTab )}.cs" );
        Design designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Dialog ) );

        using TabView tvOut = ViewFactory.Create<TabView>( );

        AddViewOperation addOperation = new( tvOut, designOut, "myTabview" );
        Assume.That( addOperation.IsImpossible, Is.False );
        bool addOperationSucceeded = false;
        Assume.That( ( ) => addOperationSucceeded = OperationManager.Instance.Do( addOperation ), Throws.Nothing );
        Assume.That( addOperationSucceeded );

        using T subview = ViewFactory.Create<T>( );

        AddViewOperation addSubviewOperation = new( subview, (Design)tvOut.Data, $"my{typeof( T ).Name}" );
        Assert.That( addSubviewOperation.IsImpossible, Is.False );
        bool addSubviewOperationSucceeded = false;
        Assert.That( ( ) => addSubviewOperationSucceeded = OperationManager.Instance.Do( addSubviewOperation ), Throws.Nothing );
        Assert.That( addSubviewOperationSucceeded );
        Assert.That( tvOut.SelectedTab.View.Subviews.ToArray( ), Does.Contain( subview ), "Expected currently selected tab to have the new view but it did not" );

        viewToCode.GenerateDesignerCs( designOut, typeof( Dialog ) );

        CodeToView codeToView = new( designOut.SourceCode );
        Design designBackIn = codeToView.CreateInstance( );

        using TabView tabIn = designBackIn.View.GetActualSubviews( ).OfType<TabView>( ).Single( );
        using T tabInSubview = tabIn.SelectedTab.View.Subviews.OfType<T>( ).Single( );

        Assert.That( tabInSubview.Text, Is.EqualTo( subview.Text ) );
    }

    [Test]
    public void GetAllDesigns_TabView<T>( [ValueSource( nameof( TabView_Tab_SubViewTypes ) )] T dummyObject )
        where T : View, new( )
    {
        using TabView tv = new( );

        SourceCodeFile source = new( new FileInfo( $"{nameof( GetAllDesigns_TabView )}.cs" ) );

        using T subview1 = ViewFactory.Create<T>( null, null, "fff" );
        Design subview1Design = new( source, "subview1", subview1 );
        subview1Design.View.Data = subview1Design;

        using T subview2 = ViewFactory.Create<T>( null, null, "ddd" );
        Design subview2Design = new( source, "subview2", subview2 );
        subview2Design.View.Data = subview2Design;

        tv.AddTab( new( "Yay", subview1Design.View ), true );
        tv.AddTab( new( "Yay", subview2Design.View ), false );

        Design tvDesign = new( source, "tv", tv );

        Design[] designs = tvDesign.GetAllDesigns( ).ToArray( );
        Assert.That( designs, Is.EquivalentTo( (Design[]) [tvDesign, subview1Design, subview2Design] ) );
    }

    [Test]
    [Category( "Terminal.Gui Extensions" )]
    [TestOf( typeof( TabViewExtensions ) )]
    public void TabView_IsBorderless_DependsOnShowBorder()
    {
        var inst = new TabView();

        ClassicAssert.IsTrue(inst.Style.ShowBorder);
        ClassicAssert.False(inst.IsBorderlessContainerView());

        inst.Style.ShowBorder = false;

        ClassicAssert.True(inst.IsBorderlessContainerView());
    }

    [Test]
    [Category( "Terminal.Gui Extensions" )]
    [TestOf( typeof( TabViewExtensions ) )]
    public void TabView_IsBorderless_DependsOnTabsOnBottom()
    {
        var inst = new TabView();

        ClassicAssert.IsFalse(inst.Style.TabsOnBottom);
        ClassicAssert.False(inst.IsBorderlessContainerView());

        inst.Style.TabsOnBottom = true;

        ClassicAssert.True(inst.IsBorderlessContainerView());
    }
}