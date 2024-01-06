using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner
{
    /// <summary>
    /// Extension methods for <see cref="TreeView"/>
    /// </summary>
    public static class TreeViewExtensions
    {
        /// <summary>
        /// For a given TreeView, returns true if there are any Objects defined in it.
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">If <paramref name="tv"/> is not a TreeView (or TreeView&lt;&gt; implementation)</exception>
        /// <exception cref="Exception">If Terminal.Gui API changes have been made that break reflection</exception>
        public static bool IsEmpty(this View tv)
        {
            if (tv is TreeView basicTreeView)
            {
                return !basicTreeView.Objects.Any();
            }

            if (!tv.GetType().IsGenericType(typeof(TreeView<>)))
            {
                throw new NotSupportedException("Expected Tree View Type to be an implementation of generic class TreeView<>");
            }



            var d = tv.Data as Design ?? throw new NotSupportedException("View does not have a Design");

            var prop = (ITreeObjectsProperty?)d.GetDesignableProperty(nameof(TreeView.Objects));

            if(prop == null)
            {
                // Options are not designable for this T type so TreeView must be empty
                return true;
            }

            return prop.IsEmpty();
        }
    }
}
