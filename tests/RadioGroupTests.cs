using Terminal.Gui;

namespace UnitTests;

class RadioGroupTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveRadioGroups()
    {

        var rgIn = RoundTrip<Window, RadioGroup>((_, _) => { }, out _);

        ClassicAssert.AreEqual(2, rgIn.RadioLabels.Length);

        ClassicAssert.AreEqual("Option 1", rgIn.RadioLabels[0]);
        ClassicAssert.AreEqual("Option 2", rgIn.RadioLabels[1]);
    }

    [Test]
    public void TestRoundTrip_PreserveRadioGroups_Custom()
    {

        var rgIn = RoundTrip<Window, RadioGroup>(
            (_, r) =>
            {
                r.RadioLabels = new string[] { "Fish", "Cat", "Balloon" };
        }, out _);

        ClassicAssert.AreEqual(3, rgIn.RadioLabels.Length);

        ClassicAssert.AreEqual("Fish", rgIn.RadioLabels[0]);
        ClassicAssert.AreEqual("Cat", rgIn.RadioLabels[1]);
        ClassicAssert.AreEqual("Balloon", rgIn.RadioLabels[2]);
    }

    [Test]
    public void TestRoundTrip_PreserveRadioGroups_Empty()
    {
        var rgIn = RoundTrip<Window, RadioGroup>(
            (_, r) =>
            {
                r.RadioLabels = new string[] { };
            }, out _);

        ClassicAssert.IsEmpty(rgIn.RadioLabels);
    }
}
