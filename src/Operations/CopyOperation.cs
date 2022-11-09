namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Copies one or more <see cref="Design"/> to static variable <see cref="LastCopiedDesign"/>.
/// </summary>
public class CopyOperation : Operation
{
    /// <summary>
    /// The objects that will be copied if this operation is run.
    /// </summary>
    private Design[] toCopy;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyOperation"/> class.  When
    /// run copies <paramref name="toCopy"/> to <see cref="LastCopiedDesign"/>.
    /// </summary>
    /// <param name="toCopy">One or more designs to copy.</param>
    public CopyOperation(params Design[] toCopy)
    {
        if (toCopy.Any())
        {
            this.toCopy = toCopy;
        }
        else
        {
            this.toCopy = new Design[0];
            this.IsImpossible = true;
            return;
        }

        this.SupportsUndo = false;

        // cannot copy a view if it is orphaned or root
        if (this.toCopy.Any(c => c.View.SuperView == null || c.IsRoot))
        {
            this.IsImpossible = true;
        }
    }

    /// <summary>
    /// Gets the last copied Designs by the last run <see cref="CopyOperation"/>.
    /// </summary>
    public static Design[]? LastCopiedDesign { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this.toCopy.Length > 1)
        {
            return $"Copy {this.toCopy.Length} Items";
        }

        return this.toCopy.Length > 0 ? $"Copy {this.toCopy[0]}" : "Copy";
    }

    /// <inheritdoc/>
    public override bool Do()
    {
        LastCopiedDesign = this.toCopy;
        return true;
    }

    /// <summary>
    /// Throws <see cref="NotSupportedException"/> as you cannot undo a copy.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown if method run.</exception>
    public override void Undo()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Throws <see cref="NotSupportedException"/> as you cannot redo a copy.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown if method run.</exception>
    public override void Redo()
    {
        throw new NotSupportedException();
    }
}
