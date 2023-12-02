using NStack;
using NUnit.Framework;
using Terminal.Gui;

namespace UnitTests;

class RadioGroupTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveRadioGroups()
    {

        var rgIn = this.RoundTrip<Window, RadioGroup>((_, _) => { }, out _);

        ClassicAssert.AreEqual(2, rgIn.RadioLabels.Length);

        ClassicAssert.AreEqual("Option 1", rgIn.RadioLabels[0].ToString());
        ClassicAssert.AreEqual("Option 2", rgIn.RadioLabels[1].ToString());
    }

    [Test]
    public void TestRoundTrip_PreserveRadioGroups_Custom()
    {

        var rgIn = this.RoundTrip<Window, RadioGroup>(
            (_, r) =>
            {
                r.RadioLabels = new ustring[] { "Fish", "Cat", "Balloon" };
        }, out _);

        ClassicAssert.AreEqual(3, rgIn.RadioLabels.Length);

        ClassicAssert.AreEqual("Fish", rgIn.RadioLabels[0].ToString());
        ClassicAssert.AreEqual("Cat", rgIn.RadioLabels[1].ToString());
        ClassicAssert.AreEqual("Balloon", rgIn.RadioLabels[2].ToString());
    }

    [Test]
    public void TestRoundTrip_PreserveRadioGroups_Empty()
    {
        var rgIn = this.RoundTrip<Window, RadioGroup>(
            (_, r) =>
            {
                r.RadioLabels = new ustring[] { };
            }, out _);

        ClassicAssert.IsEmpty(rgIn.RadioLabels);
    }
}
