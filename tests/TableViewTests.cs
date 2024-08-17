using System.Data;

namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
internal class TableViewTests : Tests
{
    [Test]
    [Category( "Terminal.Gui Extensions" )]
    [TestOf( typeof( TableViewExtensions ) )]
    public void RoundTrip_PreservesColumns( )
    {
        using TableView tableIn =
            RoundTrip<Window, TableView>( static ( _, v ) =>
                                          {
                                              Assume.That(
                                                  v.GetDataTable( ).Columns,
                                                  Is.Not.Empty,
                                                  "Default ViewFactory should create some columns for new TableViews" );
                                          },
                                          out TableView tableOut );

        Assert.That( tableIn.Table, Is.Not.Null );

        Assert.Multiple( ( ) =>
        {
            Assert.That( tableIn.Table.Columns, Is.EqualTo( tableOut.Table.Columns ) );
            Assert.That( tableIn.Table.Rows, Is.EqualTo( tableOut.Table.Rows ) );
        } );
        tableOut.Dispose( );
    }

    [Test]
    [Category( "Terminal.Gui Extensions" )]
    [TestOf( typeof( TableViewExtensions ) )]
    public void RoundTrip_TwoTablesWithDuplicatedColumns( )
    {
        // Create a TableView
        using TableView tableIn = RoundTrip<Window, TableView>( static ( d, _ ) =>
                                                                {
                                                                    // create a second TableView also on the root
                                                                    TableView tvOut2 = ViewFactory.Create<TableView>( );
                                                                    OperationManager.Instance.Do( new AddViewOperation( tvOut2, d.GetRootDesign( ), "myTable2" ) );
                                                                },
                                                                out TableView tableOut );

        // Views should collide on column name but still compile
        var designBackIn = ( (Design)tableIn.Data ).GetRootDesign( );
        var tables = designBackIn.View.GetActualSubviews( ).OfType<TableView>( ).ToArray( );

        Assert.That( tables, Has.Exactly( 2 ).InstanceOf<TableView>( ) );

        Assert.That(
            tables[ 0 ].GetDataTable( ).Columns.Cast<DataColumn>( ).Select( static c => c.ColumnName ),
            Is.EquivalentTo(
                tables[ 1 ].GetDataTable( ).Columns.Cast<DataColumn>( ).Select( static c => c.ColumnName ) ),
            "Expected both TableViews to have columns with the same names" );

        tableOut.Dispose( );
    }
}
