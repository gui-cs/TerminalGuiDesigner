using NUnit.Framework;
using TerminalGuiDesigner.ToCode;

namespace UnitTests.ToCode;

internal class CodeDomArgsTests
{

    public static object[] cases = new object[]{
        new object[]{"fff", "fff"},
        new object[]{ "33Dalmations", "dalmations"},
        new object[]{ "Dalmations33", "dalmations33"},
        new object[]{ "", "blank" },
        new object[]{ "bob is great", "bobIsGreat" },
        new object[]{ "\t", "blank" },
        new object?[]{ null, "blank" },
        new object[]{ "test\r\nffish\r\n", "testFfish" },
        new object[]{ "test\r\nffish\r\n", "testFfish" },
    };

    [TestCaseSource("cases")]
    public void Test_MakeValidFieldName(string? input, string expectedOutput)
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

    [TestCaseSource("cases")]
    public void Test_GetUniqueFieldName(string? input, string expectOutput)
    {
        var args = new CodeDomArgs(new System.CodeDom.CodeTypeDeclaration(), new System.CodeDom.CodeMemberMethod());
        ClassicAssert.AreEqual(expectOutput, args.GetUniqueFieldName(input), "Expected GetUniqueFieldName to sanitize input in the same way as MakeValidFieldName (see Test_MakeValidFieldName tests)");
    }

    [TestCase("bob", "bob", "bob2")]
    [TestCase("blank","", "blank2")]
    public void Test_GetUniqueFieldName_AfterAdding(string firstAdd, string? thenInput, string expectOutput)
    {
        var args = new CodeDomArgs(new System.CodeDom.CodeTypeDeclaration(), new System.CodeDom.CodeMemberMethod());
        args.FieldNamesUsed.Add(firstAdd);
        ClassicAssert.AreEqual(expectOutput,args.GetUniqueFieldName(thenInput));
    }

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