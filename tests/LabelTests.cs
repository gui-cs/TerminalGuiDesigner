namespace UnitTests;

    internal class LabelTests
    {
        internal static IEnumerable<TestCaseData> ExpectedDesignableProperties_Cases
        {
            get
            {
                yield return new( ViewFactory.Create<Label>( 11, 1, "Hello World" ), "X", typeof( Pos ) );
                yield return new( ViewFactory.Create<Label>( 11, 1, "Hello World" ), "Y", typeof( Pos ) );
                yield return new( ViewFactory.Create<Label>( 11, 1, "Hello World" ), "Text", typeof( string ) );
            }
        }
    }

