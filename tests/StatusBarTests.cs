namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
[Parallelizable(ParallelScope.All)]
internal class StatusBarTests : Tests
{
    [Test]
    public void ItemsArePreserved( )
    {
        Key shortcutBefore = KeyCode.Null;

        using StatusBar statusBarIn = RoundTrip<Toplevel, StatusBar>( ( _, v ) =>
        {
            Assert.That( v.Items, Has.Length.EqualTo( 1 ), $"Expected {nameof( ViewFactory )} to create a placeholder status item in new StatusBars it creates" );
            shortcutBefore = v.Items[ 0 ].Shortcut;

        }, out _ );

        Assert.That( statusBarIn.Items, Has.Length.EqualTo( 1 ), "Expected reloading StatusBar to create the same number of StatusItems" );
        Assert.That( statusBarIn.Items[ 0 ].Shortcut, Is.EqualTo( shortcutBefore ) );
    }
}