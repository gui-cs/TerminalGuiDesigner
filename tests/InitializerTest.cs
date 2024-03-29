using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
