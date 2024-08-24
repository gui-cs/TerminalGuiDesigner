using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode
{
    internal class ColorPickerToCode : ToCodeBase
    {
        private readonly Design design;
        private readonly ColorPicker colorPicker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPickerToCode"/> class.
        /// </summary>
        /// <param name="design">Wrapper for a <see cref="ColorPicker"/>.</param>
        public ColorPickerToCode(Design design)
        {
            this.design = design;

            if (design.View is not ColorPicker cp)
            {
                throw new ArgumentException(nameof(design),
                    $"{nameof(ColorPickerToCode)} can only be used with {nameof(TerminalGuiDesigner.Design)} that wrap {nameof(ColorPicker)}");
            }

            this.colorPicker = cp;
        }

        /// <summary>
        /// Adds code to .Designer.cs to call <see cref="ColorPicker.ApplyStyleChanges"/>
        /// </summary>
        /// <param name="args">State object for the .Designer.cs file being generated.</param>
        public void ToCode(CodeDomArgs args)
        {
            var colorPickerFieldExpression = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), design.FieldName);

            AddMethodCall(args,colorPickerFieldExpression,nameof(ColorPicker.ApplyStyleChanges));
        }
    }
}
