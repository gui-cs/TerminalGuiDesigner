﻿using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    /// <summary>
    /// Moves a <see cref="Shortcut"/> on a <see cref="StatusBar"/> left or right.
    /// </summary>
    public class MoveStatusItemOperation : MoveOperation<StatusBar, Shortcut>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveStatusItemOperation"/> class.
        /// </summary>
        /// <param name="design">Wrapper for a <see cref="StatusBar"/>.</param>
        /// <param name="toMove">The <see cref="Shortcut"/> to move.</param>
        /// <param name="adjustment">Negative for left, positive for right.</param>
        public MoveStatusItemOperation(Design design, Shortcut toMove, int adjustment)
            : base(
                 (v) => v.GetShortcuts(),
                 (v, a) => v.SetShortcuts(a),
                 (e) => e.Title?.ToString() ?? Operation.Unnamed,
                 design,
                 toMove,
                 adjustment)
        {
        }
    }
}
