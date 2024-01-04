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
    public void TestRoundTrip_DuplicateTabNames()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_DuplicateTabNames.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var tvOut = ViewFactory.Create<TabView>( );

        OperationManager.Instance.Do(new AddViewOperation(tvOut, designOut, "myTabview"));

        // Give both tabs the same name
        tvOut.Tabs.ElementAt(0).Text = "MyTab";
        tvOut.Tabs.ElementAt(1).Text = "MyTab";

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var tabOut = designOut.View.GetActualSubviews().OfType<TabView>().Single();

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tabIn = designBackIn.View.GetActualSubviews().OfType<TabView>().Single();

        ClassicAssert.AreEqual(2, tabIn.Tabs.Count());

        ClassicAssert.AreEqual("MyTab", tabIn.Tabs.ElementAt(0).Text);
        ClassicAssert.AreEqual("MyTab", tabIn.Tabs.ElementAt(1).Text);
    }

    [Test]
    public void TestAddingSubControlToTab()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAddingSubcontrolToTab.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var tvOut = ViewFactory.Create<TabView>( );

        OperationManager.Instance.Do(new AddViewOperation(tvOut, designOut, "myTabview"));

        var label = ViewFactory.Create<Label>( );

        OperationManager.Instance.Do(new AddViewOperation(label, (Design)tvOut.Data, "myLabel"));
        ClassicAssert.Contains(label, tvOut.SelectedTab.View.Subviews.ToArray(), "Expected currently selected tab to have the new label but it did not");

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tabIn = designBackIn.View.GetActualSubviews().OfType<TabView>().Single();
        var tabInLabel = tabIn.SelectedTab.View.Subviews.Single();

        ClassicAssert.AreEqual(label.Text, tabInLabel.Text);
    }

    [Test]
    public void TestGetAllDesigns_TabView()
    {
        var tv = new TabView();

        var source = new SourceCodeFile(new FileInfo("yarg.cs"));

        var lbl1 = new Design(source, "lbl1", new Label("fff"));
        lbl1.View.Data = lbl1;

        var lbl2 = new Design(source, "lbl2", new Label("ddd"));
        lbl2.View.Data = lbl2;

        tv.AddTab(new Tab("Yay", lbl1.View), true);
        tv.AddTab(new Tab("Yay", lbl2.View), false);

        var tvDesign = new Design(source, "tv", tv);

        ClassicAssert.Contains(tvDesign, tvDesign.GetAllDesigns().ToArray());
        ClassicAssert.Contains(lbl1, tvDesign.GetAllDesigns().ToArray());
        ClassicAssert.Contains(lbl2, tvDesign.GetAllDesigns().ToArray());

        ClassicAssert.AreEqual(3, tvDesign.GetAllDesigns().Count(), $"Expected only 3 Designs but they were {string.Join(",", tvDesign.GetAllDesigns())}");
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