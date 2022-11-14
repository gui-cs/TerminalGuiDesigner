using System.Collections.ObjectModel;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Single <see cref="Operation"/> with a collection of sub <see cref="Operation"/>
/// that are all Done or Undone at the same time. Use for tasks where you want to apply
/// same effect (E.g. move) to multiple objects at once but want a single Undo entry in
/// <see cref="OperationManager"/>.
/// </summary>
public class CompositeOperation : Operation
{
    private Operation[] operations;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeOperation"/> class.
    /// <para>
    /// Combines multiple operations into a single operation.  That operation
    /// can be applied or undone in one go by <see cref="OperationManager"/>.
    /// </para>
    /// <para>
    /// Note that <see cref="Operation.IsImpossible"/> is true if any of the passed
    /// <paramref name="operations"/> are impossible.
    /// </para>
    /// </summary>
    /// <param name="operations">All operations to perform in <see cref="Operation.Do"/>.</param>
    public CompositeOperation(params Operation[] operations)
    {
        this.operations = operations;

        // If we can't do one of them then we cannot do any
        this.IsImpossible = operations.Any(o => o.IsImpossible);
    }

    /// <summary>
    /// Gets the collection of sub operations performed on <see cref="Operation.Do"/> / <see cref="Undo"/>.
    /// </summary>
    public IReadOnlyCollection<Operation> Operations => new ReadOnlyCollection<Operation>(this.operations);

    /// <inheritdoc/>
    public override void Redo()
    {
        foreach (var op in this.operations)
        {
            op.Redo();
        }
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        foreach (var op in this.operations)
        {
            op.Undo();
        }
    }

    /// <summary>
    /// Performs the operation and returns true if any of the
    /// sub operations did anything.
    /// </summary>
    /// <returns>True if any of the <see cref="Operation"/> did anything.</returns>
    protected override bool DoImpl()
    {
        bool did = false;

        foreach (var op in this.operations)
        {
            // if any operation worked
            if (op.Do())
            {
                did = true;
            }
        }

        // we report true
        return did;
    }
}
