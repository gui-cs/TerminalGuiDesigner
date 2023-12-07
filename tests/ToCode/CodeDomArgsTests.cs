using System.Collections.Generic;
using TerminalGuiDesigner.ToCode;

namespace UnitTests.ToCode;

[TestFixture]
[TestOf(typeof(CodeDomArgs))]
internal class CodeDomArgsTests
{
    /// TODO
    /// <remarks>
    /// May be worth considering making these tests combinatorial
    /// </remarks>
    private static IEnumerable<TestCaseData> MakeValidFieldName_Cases()
    { 
        yield return new ( "fff", "fff" );
        yield return new ( "33Dalmatians", "dalmatians" );
        yield return new ( "Dalmatians33", "dalmatians33" );
        yield return new ( "", "blank" );
        yield return new ( "bob is great", "bobIsGreat" );
        yield return new ( "\t", "blank" );
        yield return new ( null, "blank" );
        yield return new ( "test\r\nffish\r\n", "testFfish" );
        yield return new ( "test\r\nffish\r\n", "testFfish" );
    }

    [Test]
    [TestCaseSource(nameof( MakeValidFieldName_Cases ))]
    public void MakeValidFieldName(string? input, string expectedOutput)
    {
        ClassicAssert.AreEqual(expectedOutput, CodeDomArgs.MakeValidFieldName(input));

        // when public we should start with an upper case letter
        ClassicAssert.IsTrue(char.IsUpper(CodeDomArgs.MakeValidFieldName(name: input, isPublic: true)[0]));

        // when private we should start with an upper case letter
        ClassicAssert.IsFalse(char.IsUpper(CodeDomArgs.MakeValidFieldName(name: input, isPublic: false)[0]));

        ClassicAssert.AreEqual(
            CodeDomArgs.MakeValidFieldName(input, true).Substring(1),
            CodeDomArgs.MakeValidFieldName(input, false).Substring(1),
            "Expected public/private to only differ on first letter caps");
    }

    [Test]
    [TestCaseSource(nameof( MakeValidFieldName_Cases ))]
    public void Test_GetUniqueFieldName(string? input, string expectOutput)
    {
        var args = new CodeDomArgs(new(), new());
        ClassicAssert.AreEqual(expectOutput, args.GetUniqueFieldName(input), "Expected GetUniqueFieldName to sanitize input in the same way as MakeValidFieldName (see Test_MakeValidFieldName tests)");
    }

    [Test]
    [TestCase("bob", "bob", "bob2")]
    [TestCase("blank","", "blank2")]
    public void Test_GetUniqueFieldName_AfterAdding(string firstAdd, string? thenInput, string expectOutput)
    {
        var args = new CodeDomArgs(new(), new());
        args.FieldNamesUsed.Add(firstAdd);
        ClassicAssert.AreEqual(expectOutput,args.GetUniqueFieldName(thenInput));
    }

    [Test]
    [TestCase("if", "_if")]
    [TestCase("ref", "_ref")]
    [TestCase("default", "_default")]
    [TestCase("out", "_out")]
    [TestCase("bool", "_bool")]
    [TestCase("else", "_else")]
    public void Test_MakeValidFieldName_ShouldPrependUnderscoreToReservedKeywords(string input, string expectedOutput)
    {
        ClassicAssert.AreEqual(expectedOutput, CodeDomArgs.MakeValidFieldName(input));
    }
}