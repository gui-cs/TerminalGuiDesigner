using TerminalGuiDesigner;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( ObjectExtensions ) )]
[Category( "Core" )]
[Parallelizable( ParallelScope.All )]
internal class ObjectExtensionsTests
{
    [Test]
    public void String_CastToReflected( )
    {
        const string str = "cat";
        object? obj = null;

        Assert.That( ( ) => obj = str.CastToReflected( typeof( object ) ), Throws.Nothing );

        //TODO: This is always going to return true if the previous assertion passed, because every object is an object.
        Assert.That( obj, Is.Not.Null.And.InstanceOf<object>( ) );
    }
}
