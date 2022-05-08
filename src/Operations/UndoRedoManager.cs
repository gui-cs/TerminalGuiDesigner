namespace TerminalGuiDesigner.Operations;

public class OperationManager
{
    Stack<IOperation> undoStack = new();
    Stack<IOperation> redoStack = new();

    public static OperationManager Instance = new();

    private OperationManager()
    {

    }

    public bool Do(IOperation op){
        
        // If operation completes successfully
        if(op.Do())
        {
            // We can no longer redo
            redoStack.Clear();
            undoStack.Push(op);
            return true;
        }

        return false;
    }

    public void Undo()
    {
        if (undoStack.TryPop(out var op))
        {
            op.Undo();
            redoStack.Push(op);
        }
    }

    public void Redo()
    {
        if (redoStack.TryPop(out var op))
        {
            op.Redo();
            undoStack.Push(op);
        }
    }

    public void ClearUndoRedo()
    {
        undoStack.Clear();
        redoStack.Clear();
    }
}
