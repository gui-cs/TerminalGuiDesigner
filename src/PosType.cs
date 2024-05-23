using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Describes the origins of a <see cref="Pos"/> (e.g.
/// <see cref="Pos.Center"/> maps to <see cref="PosType.Center"/>.
/// </summary>
public enum PosType
{
    /// <summary>
    /// An absolute fixed value <see cref="Pos"/> e.g. 5.
    /// May be the result of <see cref="Pos.Absolute(int)"/> call
    /// or an implicit cast of int value e.g.
    /// <code>myView.X = 5;</code>
    /// </summary>
    Absolute,

    /// <summary>
    /// Indicates use of <see cref="Pos.AnchorEnd(int)"/>.
    /// </summary>
    AnchorEnd,

    /// <summary>
    /// Indicates use of <see cref="Pos.Percent(float)"/>.
    /// </summary>
    Percent,

    /// <summary>
    /// Indicates use of <see cref="Pos.Right(View)"/> / <see cref="Pos.Left(View)"/> etc.
    /// </summary>
    Relative,

    /// <summary>
    /// Indicates use of <see cref="Pos.Center"/>.
    /// </summary>
    Center,
}
