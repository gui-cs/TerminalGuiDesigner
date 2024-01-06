using System.Diagnostics.CodeAnalysis;
using StringExtensions = TerminalGuiDesigner.StringExtensions;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( StringExtensions ) )]
[Category( "Core" )]
[Parallelizable( ParallelScope.All )]
[SuppressMessage( "Performance", "CA1861:Avoid constant arrays as arguments", Justification = "We don't really care for unit tests..." )]
internal class StringExtensionTests
{
    private static IEnumerable<TestCaseData> MakeUnique_DefaultComparer_Cases =>
    [
        new( "bob", "bob", new[] { "fish" } ),
        new( "fish", "fish2", new[] { "fish" } ),
        new( "fish2", "fish3", new[] { "fish1", "fish2" } ),
        new( null, "blank", new[] { "fish1", "fish2" } ),
        new( "  ", "blank", new[] { "fish1", "fish2" } ),
        new( "bob", "bob3", new[] { "bob", "bob2" } ),
        new( "Fish1", "Fish1", new[] { "fish1" } )
    ];

    private static IEnumerable<TestCaseData> MakeUnique_IgnoreCase_Cases =>
    [
        new( "bob", "bob", new[] { "fish" } ),
        new( "Fish", "Fish2", new[] { "fish" } ),
        new( "fIsh2", "fIsh3", new[] { "fish1", "fish2" } ),
        new( "fish2", "fish3", new[] { "fiSH1", "fiSH2" } )
    ];

    private static IEnumerable<TestCaseData> PadBoth_Cases =>
    [
        new( "a", 4, " a  " ),
        new( "a", 5, "  a  " ),
        new( "b l", 5, " b l " ),
        new( "sooooLooong", 2, "sooooLooong" )
    ];

    [Test]
    [TestCaseSource( nameof( MakeUnique_DefaultComparer_Cases ) )]
    public void MakeUnique_DefaultComparer( string? stringIn, string expectedOut, string[] whenCollection )
    {
        Assert.That( stringIn.MakeUnique( whenCollection ), Is.EqualTo( expectedOut ) );
    }

    [Test]
    [TestCaseSource( nameof( MakeUnique_IgnoreCase_Cases ) )]
    public void MakeUnique_IgnoreCase( string stringIn, string expectedOut, string[] whenCollection )
    {
        Assert.That( stringIn.MakeUnique( whenCollection, StringComparer.InvariantCultureIgnoreCase ), Is.EqualTo( expectedOut ) );
    }

    [Test]
    [TestCaseSource( nameof( PadBoth_Cases ) )]
    public void PadBoth( string input, int length, string expectedOutput )
    {
        Assert.That( input.PadBoth( length ), Is.EqualTo( expectedOutput ) );
    }
}
