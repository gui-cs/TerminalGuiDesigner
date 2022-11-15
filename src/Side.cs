using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Describes the method used to create a <see cref="PosType.Relative"/>.
/// These map to <see cref="Pos.Left"/>, <see cref="Pos.Right(View)"/> etc.
/// </summary>
/// <remarks>
/// These enum values match private field 'side' in PosView see:
/// <code>
/// https://github.com/gui-cs/Terminal.Gui/blob/develop/Terminal.Gui/Core/PosDim.cs
/// </code>
/// </remarks>
public enum Side
{
    /// <summary>
    /// Describes <see cref="Pos.Left(View)"/>.
    /// </summary>
    Left = 0,

    /// <summary>
    /// Describes <see cref="Pos.Top(View)"/>.
    /// </summary>
    Top = 1,

    /// <summary>
    /// Describes <see cref="Pos.Right(View)"/>.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Describes <see cref="Pos.Bottom(View)"/>.
    /// </summary>
    Bottom = 3,
}