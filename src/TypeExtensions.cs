using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

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
            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return type.GetGenericArguments().Single();
            }


            return null;
        }

        /// <summary>
        /// Returns true if <paramref name="type"/> is an implementation of a generic parent
        /// type <paramref name="genericParentHypothesis"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericParentHypothesis">Generic parent e.g. <see langword="typeof"/>(TreeView&lt;&gt;)</param>
        /// <returns></returns>
        public static bool IsGenericType(this Type type, Type genericParentHypothesis)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericParentHypothesis;
        }
    }
}
