using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode
{
    /// <summary>
    /// Ancillary code generation for <see cref="SpinnerView"/> e.g. setting up auto spin.
    /// </summary>
    internal class SpinnerViewToCode : ToCodeBase
    {
        private Design design;

        public SpinnerViewToCode(Design design)
        {
            this.design = design;

            if (design.View is not SpinnerView sv)
            {
                throw new ArgumentException(nameof(design), $"{nameof(SpinnerViewToCode)} can only be used with {nameof(TerminalGuiDesigner.Design)} that wrap {nameof(SpinnerView)}");
            }
        }

        /// <summary>
        /// Adds code to .Designer.cs to configure <see cref="SpinnerView"/> in the same way it
        /// is designed.
        /// </summary>
        /// <param name="args">State object for the .Designer.cs file being generated.</param>
        public void ToCode(CodeDomArgs args)
        {
            // Adds:
            // this.mySpinner.AutoSpin();

            this.AddMethodCall(args,
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.design.FieldName),
                nameof(SpinnerView.AutoSpin));
        }
    }
}
