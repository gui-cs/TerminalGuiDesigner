using System.CodeDom;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner
{
    /// <summary>
    /// Provides knowledge about how to handle different T types for generic
    /// views e.g. <see cref="Slider{T}"/>, <see cref="TreeView{T}"/>
    /// </summary>
    public static class TTypes
    {
        /// <summary>
        /// Returns <see cref="CodeObjectCreateExpression"/> or <see cref="CodePrimitiveExpression"/>
        /// or similar that represents <paramref name="value"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="design"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CodeExpression ToCode(CodeDomArgs args, Design design, object? value)
        {
            if(value == null || value is string || value.GetType().IsValueType)
            {
                return value.ToCodePrimitiveExpression();
            }

            if(value is FileSystemInfo fsi)
            {
                return new CodeObjectCreateExpression(value.GetType(), fsi.ToString().ToCodePrimitiveExpression());
            }

            throw new NotSupportedException("Value Type ToCode not known" + value.GetType());
        }

        /// <summary>
        /// Returns all Types which can be used with generic view of the given <paramref name="viewType"/>.
        /// </summary>
        /// <param name="viewType">A generic view type e.g. <see langword="typeof"/>(Slider&lt;&gt;)</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSupportedTTypesForGenericViewOfType(Type viewType)
        {
            if (viewType == typeof(Slider<>))
            {
                return new[] { typeof(int), typeof(string), typeof(int), typeof(double), typeof(bool) };
            }

            if (viewType == typeof(TreeView<>))
            {
                return new[] { typeof(object), typeof(FileSystemInfo) };
            }

            throw new NotSupportedException($"Generic View {viewType} is not yet supported");
        }

    }
}
