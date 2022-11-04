namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Describes a single operation that can be performed (e.g. change the name
/// on a button).  Where possible all implementers should implement <see cref="Undo"/>
/// as well as <see cref="Do"/> and have <see cref="SupportsUndo"/> return true.
/// </summary>
public interface IOperation
{
    /// <summary>
    /// Gets a unique identifier for this instance.
    /// </summary>
    Guid UniqueIdentifier { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="IOperation"/> is in a valid
    /// state for execution.
    /// </summary>
    bool IsImpossible { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Undo"/> is supported for this
    /// <see cref="IOperation"/>.
    /// </summary>
    bool SupportsUndo { get; }

    /// <summary>
    /// Performs the operation.
    /// </summary>
    /// <returns>True if the operation was successful.  False if it was canceled during
    /// execution by the user or could not be performed for some reason.</returns>
    bool Do();

    /// <summary>
    /// Reverts the effects of <see cref="Do"/>.
    /// </summary>
    void Undo();

    /// <summary>
    /// Reverts the effects of the <see cref="Undo"/> operation.  This may
    /// be as simple as calling <see cref="Do"/> again or might involve less
    /// steps (e.g. not allocate new objects but operate on those already created
    /// by a previous <see cref="Do"/> call).
    /// </summary>
    void Redo();
}
