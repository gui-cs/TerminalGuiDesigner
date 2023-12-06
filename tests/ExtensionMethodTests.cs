using TerminalGuiDesigner;

namespace UnitTests
{
    [TestFixture]
    internal class ExtensionMethodTests
    {
        [Test]
        public void String_CastToReflected( )
        {
            var str = "cat";
            object obj = str.CastToReflected( typeof( object ) );
            ClassicAssert.IsInstanceOf<object>( obj );
        }

    }
}