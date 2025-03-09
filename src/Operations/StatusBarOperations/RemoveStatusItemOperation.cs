﻿using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Removes a <see cref="Shortcut"/> from a <see cref="StatusBar"/>.
    /// </summary>
    public class RemoveStatusItemOperation : RemoveOperation<StatusBar, Shortcut>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveStatusItemOperation"/> class.
        /// </summary>
        /// <param name="design">Wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="toRemove">A <see cref="Shortcut"/> to remove from bar.</param>
        public RemoveStatusItemOperation(Design design, Shortcut toRemove)
            : base(
                  (v) => v.GetShortcuts(),
                  (v, a) => v.SetShortcuts(a),
                  (e) => e.Title?.ToString() ?? Operation.Unnamed,
                  design,
                  toRemove)
        {
        }
    }
}
