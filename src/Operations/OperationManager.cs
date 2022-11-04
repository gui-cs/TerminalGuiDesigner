namespace TerminalGuiDesigner.Operations;

public class OperationManager
{
    Stack<IOperation> undoStack = new();
    Stack<IOperation> redoStack = new();

    public int UndoStackSize => this.undoStack.Count;
    public int RedoStackSize => this.redoStack.Count;

    public static OperationManager Instance = new();

    private OperationManager()
    {
    }

    public bool Do(IOperation op)
    {
        // If operation completes successfully
        if (!op.IsImpossible && op.Do())
        {
            if (op.SupportsUndo)
            {
                // We can no longer redo
                this.redoStack.Clear();
                this.undoStack.Push(op);
            }

            return true;
        }

        return false;
    }

    public void Undo()
    {
        if (this.undoStack.TryPop(out var op))
        {
            op.Undo();
            this.redoStack.Push(op);
        }
    }

    public void Redo()
    {
        if (this.redoStack.TryPop(out var op))
        {
            op.Redo();
            this.undoStack.Push(op);
        }
    }

    public void ClearUndoRedo()
    {
        this.undoStack.Clear();
        this.redoStack.Clear();
    }

    /// <summary>
    /// Returns the latest operation undertaken.  This excludes any that have
    /// been Undone (i.e. <see cref="Undo"/>).
    /// </summary>
    /// <returns></returns>
    public IOperation? GetLastAppliedOperation()
    {
        return this.undoStack.TryPeek(out var op) ? op : null;
    }
}
