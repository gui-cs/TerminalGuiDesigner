using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGuiDesigner
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Implementation of <see cref="Type.GetElementType"/> that also works for 
        /// <see cref="IList{T}"/>
        /// </summary>
        /// <returns>Element type of collection or <see langword="null"/>.</returns>
        public static Type? GetElementTypeEx(this Type type)
        {
            var elementType = type.GetElementType();

            if (elementType != null)
            {
                return elementType;
            }

            if (type.IsAssignableTo(typeof(IList)) && type.IsGenericType)
            {
                return type.GetGenericArguments().Single();
            }

            return null;
        }
    }
}
