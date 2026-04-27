using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Terminal.Gui.ViewBase;

namespace TerminalGuiDesigner.ToCode
{
    /// <summary>
    /// <para>
    /// Handles the fact that ShadowStyle is a magic property.
    /// </para>
    /// <para>
    /// Get its value will return e.g. ShadowStyles.Transparent even on a new instance but
    /// if you set its property to ShadowStyles.Transparent then a margin will appear i.e.
    /// it has side effects.
    /// </para>
    /// <para>
    /// Class handles it by looking at the View and investigating its Margin to see if a real
    /// shadow is in effect or not.
    /// </para>
    /// </summary>
    internal class ShadowStyleProperty : Property
    {
        public ShadowStyleProperty(Design design, PropertyInfo property) : base(design, property)
        {
        }

        public override object? GetValue()
        {
            if (!HasMargin())
            {
                return ShadowStyles.None;
            }

            return base.GetValue();
        }

        private bool HasMargin()
        {
            return Design.View.Margin != null
                && (Design.View.Margin.Thickness.Left != 0
                     || Design.View.Margin.Thickness.Top != 0
                     || Design.View.Margin.Thickness.Right != 0
                     || Design.View.Margin.Thickness.Bottom != 0);
        }
    }
}
