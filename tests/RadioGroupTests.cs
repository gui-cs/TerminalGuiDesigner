namespace UnitTests;

[TestFixture]
[TestOf( typeof( OperationManager ) )]
[TestOf( typeof( CodeToView ) )]
[TestOf( typeof( ViewToCode ) )]
[Category( "Code Generation" )]
internal class RadioGroupTests : Tests
{
    [Test]
    public void RoundTrip_PreserveRadioGroups()
    {

        var rgIn = RoundTrip<Window, RadioGroup>( static (_, _) => { }, out _);

        ClassicAssert.AreEqual(2, rgIn.RadioLabels.Length);

        ClassicAssert.AreEqual("Option 1", rgIn.RadioLabels[0]);
        ClassicAssert.AreEqual("Option 2", rgIn.RadioLabels[1]);
    }

    [Test]
    public void RoundTrip_PreserveRadioGroups_Custom()
    {

        var rgIn = RoundTrip<Window, RadioGroup>( static (_, r) =>
            {
                r.RadioLabels = [ "Fish", "Cat", "Balloon" ];
        }, out _);

        ClassicAssert.AreEqual(3, rgIn.RadioLabels.Length);

        ClassicAssert.AreEqual("Fish", rgIn.RadioLabels[0]);
        ClassicAssert.AreEqual("Cat", rgIn.RadioLabels[1]);
        ClassicAssert.AreEqual("Balloon", rgIn.RadioLabels[2]);
    }

    [Test]
    public void RoundTrip_PreserveRadioGroups_Empty()
    {
        var rgIn = RoundTrip<Window, RadioGroup>( static (_, r) =>
            {
                r.RadioLabels = [ ];
            }, out _);

        ClassicAssert.IsEmpty(rgIn.RadioLabels);
    }
}
