namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
[NonParallelizable]
internal class StatusBarTests : Tests
{
    [Test]
    public void ItemsArePreserved( )
    {
        Key shortcutBefore = KeyCode.Null;

        using StatusBar statusBarIn = RoundTrip<Toplevel, StatusBar>( ( _, v ) =>
        {
            Assert.That( v.GetShortcuts(), Has.Length.EqualTo( 1 ), $"Expected {nameof( ViewFactory )} to create a placeholder status item in new StatusBars it creates" );
            shortcutBefore = v.GetShortcuts()[ 0 ].Key;

        }, out _ );

        Assert.That( statusBarIn.GetShortcuts(), Has.Length.EqualTo( 1 ), "Expected reloading StatusBar to create the same number of StatusItems" );
        Assert.That( statusBarIn.GetShortcuts()[ 0 ].Key, Is.EqualTo( shortcutBefore ) );
    }
}