using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// The type of a <see cref="Dim"/>.
/// </summary>
public enum DimType
{
    /// <summary>
    /// Absolute fixed measure of width/height e.g. 5.
    /// </summary>
    Absolute,

    /// <summary>
    /// Percent of the remaining width/height e.g. <see cref="Dim.Percent(int, bool)"/>.
    /// </summary>
    Percent,

    /// <summary>
    /// Filling the remaining space with a margin e.g. <see cref="Dim.Fill(int)"/>.
    /// </summary>
    Fill,

    /// <summary>
    /// Automatically size based on Text property e.g. <see cref="Dim.Auto(DimAutoStyle, Dim?, Dim?)"/>
    /// </summary>
    Auto
}