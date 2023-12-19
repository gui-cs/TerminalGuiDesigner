namespace UnitTests.TestSupportTypes;

internal class ExpectedTrueTestCaseData : TestCaseData
{
    public ExpectedTrueTestCaseData( )
    {
        HasExpectedResult = true;
        ExpectedResult = true;
    }

    public ExpectedTrueTestCaseData( params object?[]? testParameters ) : base( testParameters )
    {
        HasExpectedResult = true;
        ExpectedResult = true;
    }
}
