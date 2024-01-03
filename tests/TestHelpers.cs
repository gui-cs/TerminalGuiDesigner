using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.Contracts;
using Microsoft.CSharp;

namespace UnitTests
{
    internal static class TestHelpers
    {

        [Pure]
        internal static string ExpressionToCode( CodeExpression expression )
        {
            CSharpCodeProvider provider = new( );

            using StringWriter sw = new( );
            using IndentedTextWriter tw = new( sw, "    " );
            provider.GenerateCodeFromExpression( expression, tw, new( ) );
            tw.Close( );

            return sw.GetStringBuilder( ).ToString( );
        }
    }
}