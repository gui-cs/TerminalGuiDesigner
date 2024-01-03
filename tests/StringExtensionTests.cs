using StringExtensions = TerminalGuiDesigner.StringExtensions;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( StringExtensions ) )]
[Category( "Core" )]
internal class StringExtensionTests
{
    [TestCase("bob", "bob", new[] {"fish"})]
    [TestCase("fish", "fish2", new[] { "fish" })]
    [TestCase("fish2", "fish3", new[] { "fish1","fish2" })]
    [TestCase(null, "blank", new[] { "fish1", "fish2" })]
    [TestCase("  ", "blank", new[] { "fish1", "fish2" })]
    [TestCase("bob", "bob3", new[] { "bob", "bob2" })]
    [TestCase("Fish1", "Fish1", new[] { "fish1"})] // default behavior is case sensitive
    public void MakeUnique_DefaultComparer(string? stringIn, string expectedOut, string[] whenCollection)
    {
        Assert.That( stringIn.MakeUnique(whenCollection), Is.EqualTo( expectedOut ) );
    }

    [TestCase("bob", "bob", new[] { "fish" })]
    [TestCase("Fish", "Fish2", new[] { "fish" })]
    [TestCase("fIsh2", "fIsh3", new[] { "fish1", "fish2" })]
    [TestCase("fish2", "fish3", new[] { "fiSH1", "fiSH2" })]
    public void MakeUnique_IgnoreCaps(string stringIn, string expectedOut, string[] whenCollection)
    {
        Assert.That( stringIn.MakeUnique(whenCollection,System.StringComparer.InvariantCultureIgnoreCase), Is.EqualTo( expectedOut ) );
    }

    [TestCase("a",4," a  ")]
    [TestCase("a", 5, "  a  ")]
    [TestCase("b l", 5, " b l ")]
    [TestCase("sooooLooong", 2, "sooooLooong")]
    public void PadBoth(string input, int length, string expectedOutput)
    {
        Assert.That( input.PadBoth(length), Is.EqualTo( expectedOutput ) );
    }
}
