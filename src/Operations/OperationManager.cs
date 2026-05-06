namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Singleton which coordinates the execution of <see cref="Operation"/>.  This
/// involves running <see cref="Operation.Do"/>, <see cref="Operation.Undo"/> and
/// <see cref="Operation.Redo"/> and tracking what commands have been done
/// in undo/redo stacks.
/// </summary>
public class OperationManager
{
    private Stack<IOperation> undoStack = new();
    private Stack<IOperation> redoStack = new();

    private OperationManager()
    {
    }

    /// <summary>
    /// Gets the Singleton instance of <see cref="OperationManager"/>.
    /// </summary>
    public static OperationManager Instance { get; } = new();

    /// <summary>
    /// Gets the current number of <see cref="Operation"/> undo the undo stack.
    /// </summary>
    public int UndoStackSize => this.undoStack.Count;

    /// <summary>
    /// Gets the current number of <see cref="Operation"/> undo the redo stack.
    /// This is a count of the number of operations that have been undone since
    /// the last <see cref="Do(IOperation)"/> (which clears redo stack).
    /// </summary>
    public int RedoStackSize => this.redoStack.Count;

    /// <summary>
    /// Gets or sets an in-progress operation that has not yet been committed (e.g. the user
    /// is mid-keystroke editing a label or menu title).  Auto-committed by <see cref="Do"/>
    /// and <see cref="Undo"/> before anything else lands on the undo stack.
    /// </summary>
    public IOperation? PendingOperation { get; set; }

    /// <summary>
    /// Commits <see cref="PendingOperation"/> to the undo stack if one is set.
    /// Called automatically by <see cref="Do"/> and also on focus change.
    /// </summary>
    public void FlushPending()
    {
        var pending = this.PendingOperation;
        if (pending == null)
        {
            return;
        }

        this.PendingOperation = null;

        if (!pending.IsImpossible && pending.Do() && pending.SupportsUndo)
        {
            this.redoStack.Clear();
            this.undoStack.Push(pending);
        }
    }

    /// <summary>
    /// Runs <see cref="Operation.Do"/> on <paramref name="op"/> if it is
    /// not <see cref="IOperation.IsImpossible"/> and then pushes it onto
    /// the undo stack (see <see cref="UndoStackSize"/>) in case user changes
    /// their mind.
    /// </summary>
    /// <param name="op">The <see cref="IOperation"/> to <see cref="IOperation.Do"/>.</param>
    /// <returns>Result of <see cref="IOperation.Do"/> (true if operation did something).</returns>
    public bool Do(IOperation op)
    {
        // Commit any in-progress text-typing operation before stacking this one
        this.FlushPending();

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

    /// <summary>
    /// Pops the last performed <see cref="IOperation"/> off the undo stack and
    /// runs <see cref="IOperation.Undo"/>.  Does nothing if <see cref="UndoStackSize"/>
    /// is 0.
    /// </summary>
    public void Undo()
    {
        this.FlushPending();

        if (this.undoStack.TryPop(out var op))
        {
            op.Undo();
            this.redoStack.Push(op);
        }
    }

    /// <summary>
    /// Pops the last performed <see cref="IOperation"/> off the redo stack and
    /// runs <see cref="IOperation.Redo"/>.  Does nothing if <see cref="RedoStackSize"/>
    /// is 0.
    /// </summary>
    public void Redo()
    {
        if (this.redoStack.TryPop(out var op))
        {
            op.Redo();
            this.undoStack.Push(op);
        }
    }

    /// <summary>
    /// Clears all <see cref="Operation"/> from the undo/redo stacks.
    /// Use this in testing or when editor document is closed/opened.
    /// </summary>
    public void ClearUndoRedo()
    {
        this.PendingOperation = null;
        this.undoStack.Clear();
        this.redoStack.Clear();
    }

    /// <summary>
    /// Returns the latest operation undertaken.  This excludes any that have
    /// been Undone (i.e. <see cref="Undo"/>).
    /// </summary>
    /// <returns>Last performed <see cref="IOperation"/> or null if none have been done yet.</returns>
    public IOperation? GetLastAppliedOperation()
    {
        return this.undoStack.TryPeek(out var op) ? op : null;
    }
}
