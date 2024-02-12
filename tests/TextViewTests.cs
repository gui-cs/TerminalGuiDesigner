namespace UnitTests;

[TestFixture]
[Category( "Core" )]
[TestOf( typeof( SetPropertyOperation ) )]
[TestOf( typeof( Property ) )]
internal class TextViewTests : Tests
{
    [Test]
    public void SetProperty_YieldsExpectedValue( [Values(null,"", "fff", "abc123" )] string? originalValue, [Values( null, "", "fff", "abc123" )] string? newValue )
    {
        using TextView tv = ViewFactory.Create<TextView>( text: originalValue );

        Design d = new( new( $"{nameof( TextViewTests )}.{nameof( SetProperty_YieldsExpectedValue )}.cs" ), "mytv", tv );
        tv.Data = d;

        Property? textProperty = d.GetDesignableProperty( "Text" );
        Assert.That( textProperty, Is.Not.Null.And.InstanceOf<Property>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( textProperty!.DeclaringObject, Is.SameAs( tv ) );
            Assert.That( textProperty.GetValue( ), Is.EqualTo(
                             originalValue switch
                             {
                                 // TODO: That string should be turned into a constant.
                                 // That way it can be referenced in tests, and helps prevent possible future bugs from mismatches.
                                 null => "Heya",
                                 _ => originalValue
                             } ) );
        } );

        SetPropertyOperation op = new( d, textProperty!, tv.Text, newValue );

        Assert.That( op.Do, Throws.Nothing );

        switch ( newValue )
        {
            case null:
            case "":
                Assert.That( tv.Text, Is.Empty );
                break;
            default:
                Assert.That( tv.Text, Is.EqualTo( newValue ) );
                break;

        }
    }
}
