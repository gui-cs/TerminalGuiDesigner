namespace UnitTests;

[TestFixture]
[TestOf( typeof( ListView ) )]
[Category( "Core" )]
[Category( "Code Generation" )]
internal class ListViewTests : Tests
{
    private static IEnumerable<IList<string>> RoundTrip_PreservesList_Cases
    {
        get
        {
            List<string> list = new( );
            for ( int i = 1; i < 5; i++ )
            {
                list.Add( $"Item{i}" );
                yield return list;
            }
        }
    }

    [Test]
    public void RoundTrip_PreservesList( [ValueSource( nameof( RoundTrip_PreservesList_Cases ) )] List<string> listViewContents )
    {
        var viewToCode = new ViewToCode( );

        var file = new FileInfo( "TestRoundTrip_PreserveList.cs" );
        var designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Window ) );

        Assume.That( designOut, Is.Not.Null.And.InstanceOf<Design>( ) );

        ListView initialListView = ViewFactory.Create<ListView>( );
        Assume.That( initialListView, Is.Not.Null.And.InstanceOf<ListView>( ) );

        Assume.That( ( ) => initialListView.SetSource( listViewContents ), Throws.Nothing );
        Assume.That( initialListView.Source, Has.Count.EqualTo( listViewContents.Count ) );

        IList initialListViewSource = initialListView.Source.ToList( );
        Assume.That( initialListViewSource, Is.Not.Null.And.Not.Empty );
        Assume.That( initialListViewSource, Is.EquivalentTo( listViewContents ) );

        OperationManager.Instance.Do( new AddViewOperation( initialListView, designOut, "myList" ) );

        viewToCode.GenerateDesignerCs( designOut, typeof( Window ) );

        ListView? listViewFromDesigner = designOut.View.GetActualSubviews( ).OfType<ListView>( ).SingleOrDefault( );
        Assert.That( listViewFromDesigner, Is.Not.Null.And.InstanceOf<ListView>( ) );

        var codeToView = new CodeToView( designOut.SourceCode );
        var designBackIn = codeToView.CreateInstance( );

        var listViewFromGeneratedCode = designBackIn.View.GetActualSubviews( ).OfType<ListView>( ).SingleOrDefault( );
        Assert.That( listViewFromGeneratedCode, Is.Not.Null.And.InstanceOf<ListView>( ) );

        IList finalListViewSource = listViewFromGeneratedCode!.Source.ToList( );

        // Check that the compiled ListView has the same default values it was created with.
        Assert.That( listViewFromGeneratedCode.Source, Is.Not.Null.And.InstanceOf<IListDataSource>( ) );
        Assert.That( listViewFromGeneratedCode.Source, Has.Count.EqualTo( listViewContents.Count ) );

        Assert.That( finalListViewSource, Is.Not.Null.And.Not.Empty );
        Assert.That( finalListViewSource, Has.Exactly( listViewContents.Count ).InstanceOf<string>( ) );
        Assert.That( finalListViewSource, Is.EquivalentTo( listViewContents ) );
        Assert.That( finalListViewSource, Is.Not.SameAs( initialListViewSource ) );
    }

    [Test]
    public void TestIListSourceProperty_Rhs( )
    {
        var file = new FileInfo( "TestIListSourceProperty_Rhs.cs" );
        var lv = new ListView( );
        var d = new Design( new( file ), "lv", lv );
        var prop = d.GetDesignableProperties( ).Single( p => p.PropertyInfo.Name.Equals( "Source" ) );

        Assert.That( lv.Source, Is.Null );

        prop.SetValue( new[] { "hi there", "my friend" } );

        Assert.That( lv.Source, Has.Count.EqualTo( 2 ) );

        var code = TestHelpers.ExpressionToCode( prop.GetRhs( ) );

        Assert.That(
            code, Is.EqualTo( "new Terminal.Gui.ListWrapper(new string[] {\n            \"hi there\",\n            \"my friend\"})".Replace( "\n", Environment.NewLine ) ) );
    }
}
