using NUnit.Framework;
using Terminal.Gui;

namespace UnitTests;

class RadioGroupTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveRadioGroups()
    {

        var rgIn = this.RoundTrip<Window, RadioGroup>((_, _) => { }, out _);

        Assert.AreEqual(2, rgIn.RadioLabels.Length);

        Assert.AreEqual("Option 1", rgIn.RadioLabels[0].ToString());
        Assert.AreEqual("Option 2", rgIn.RadioLabels[1].ToString());
    }

    [Test]
    public void TestRoundTrip_PreserveRadioGroups_Custom()
    {

        var rgIn = this.RoundTrip<Window, RadioGroup>(
            (_, r) =>
            {
                r.RadioLabels = new string[] { "Fish", "Cat", "Balloon" };
        }, out _);

        Assert.AreEqual(3, rgIn.RadioLabels.Length);

        Assert.AreEqual("Fish", rgIn.RadioLabels[0].ToString());
        Assert.AreEqual("Cat", rgIn.RadioLabels[1].ToString());
        Assert.AreEqual("Balloon", rgIn.RadioLabels[2].ToString());
    }

    [Test]
    public void TestRoundTrip_PreserveRadioGroups_Empty()
    {
        var rgIn = this.RoundTrip<Window, RadioGroup>(
            (_, r) =>
            {
                r.RadioLabels = new string[] { };
            }, out _);

        Assert.IsEmpty(rgIn.RadioLabels);
    }
}
