namespace UnitTests;

[TestFixture]
[TestOf( typeof( OperationManager ) )]
[TestOf( typeof( CodeToView ) )]
[TestOf( typeof( ViewToCode ) )]
[Category( "Code Generation" )]
internal class RadioGroupTests : Tests
{
    [Test]
    public void RoundTrip_PreserveRadioGroups( )
    {
        var rgIn = RoundTrip<Window, RadioGroup>( static ( _, _ ) => { }, out _ );

        Assert.That( rgIn.RadioLabels, Has.Length.EqualTo( 2 ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( rgIn.RadioLabels[ 0 ], Is.EqualTo( "Option 1" ) );
            Assert.That( rgIn.RadioLabels[ 1 ], Is.EqualTo( "Option 2" ) );
        } );
    }

    [Test]
    public void RoundTrip_PreserveRadioGroups_Custom( )
    {
        var rgIn = RoundTrip<Window, RadioGroup>( static ( _, r ) => { r.RadioLabels = ["Fish", "Cat", "Balloon"]; }, out _ );

        Assert.That( rgIn.RadioLabels, Has.Length.EqualTo( 3 ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( rgIn.RadioLabels[ 0 ], Is.EqualTo( "Fish" ) );
            Assert.That( rgIn.RadioLabels[ 1 ], Is.EqualTo( "Cat" ) );
            Assert.That( rgIn.RadioLabels[ 2 ], Is.EqualTo( "Balloon" ) );
        } );
    }

    [Test]
    public void RoundTrip_PreserveRadioGroups_Empty( )
    {
        var rgIn = RoundTrip<Window, RadioGroup>( static ( _, r ) => { r.RadioLabels = []; }, out _ );

        Assert.That( rgIn.RadioLabels, Is.Empty );
    }
}
