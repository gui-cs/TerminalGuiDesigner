namespace UnitTests.TestSupportTypes;

internal class ExpectedFalseTestCaseData : TestCaseData
{
    public ExpectedFalseTestCaseData( )
    {
        HasExpectedResult = true;
        ExpectedResult = false;
    }

    public ExpectedFalseTestCaseData( params object?[]? testParameters ) : base( testParameters )
    {
        HasExpectedResult = true;
        ExpectedResult = false;
    }
}