
using System.Collections.ObjectModel;

namespace TerminalGuiDesigner.Operations;

public class CompositeOperation : Operation
{
    private Operation[] _operations;

    public IReadOnlyCollection<Operation> Operations => new ReadOnlyCollection<Operation>(_operations);

    /// <summary>
    /// <para>
    /// Combines multiple operations into a single operation.  That operation
    /// can be applied or undone in one go by <see cref="OperationManager"/>.
    /// </para>
    /// <para>
    /// Note that <see cref="Operation.IsImpossible"/> is true if any of the passed
    /// <paramref name="operations"/> are impossible
    /// </para>
    /// </summary>
    /// <param name="operations"></param>
    public CompositeOperation(params Operation[] operations)
    {
        _operations = operations;

        // If we cann't do one of them then we cannot do any
        IsImpossible = operations.Any(o => o.IsImpossible);
    }


    /// <summary>
    /// Performs the operation and returns true if any of the 
    /// sub operations did anything
    /// </summary>
    /// <returns></returns>
    public override bool Do()
    {
        bool did = false;

        foreach(var op in _operations)
        {
            // if any operation worked
            if(op.Do())
            {
                did = true;
            }
        }

        // we report true
        return did;
    }

    public override void Redo()
    {
        foreach (var op in _operations)
        {
            op.Redo();
        }
    }

    public override void Undo()
    {
        foreach (var op in _operations)
        {
            op.Undo();
        }
    }
}
