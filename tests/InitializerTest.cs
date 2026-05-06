using System.CodeDom;

namespace UnitTests
{
    internal class InitializerTest
    {
        [Test]
        public void TestConstructorInitializers()
        {
            var ctor = new CodeObjectCreateExpression(
                new CodeTypeReference("MyClass"));
            

            TestContext.WriteLine(Helpers.ExpressionToCode(ctor));
        }
    }
}
