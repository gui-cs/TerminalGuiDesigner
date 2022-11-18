using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner;

/// <summary>
/// Abstract base class for <see cref="Operation"/> which move something
/// within an array (e.g. Tabs, Menus etc).
/// </summary>
/// <typeparam name="T1">The <see cref="View"/> that your operation is modifying (e.g. <see cref="MenuBar"/>).</typeparam>
/// <typeparam name="T2">The element type within the array that is being moved (e.g. <see cref="MenuBarItem"/>).</typeparam>
public abstract class MoveOperation<T1, T2> : Operation
    where T1 : View
{
    /// <summary>
    /// Gets the number of index positions the element will
    /// be moved. Negative for left, positive for right.
    /// </summary>
    private readonly int adjustment;
    private readonly int originalIdx;
    private readonly int newIndex;
    private readonly Func<T2, string> stringGetter;
    private readonly Func<T1, T2[]> arrayGetter;
    private readonly Action<T1, T2[]> arraySetter;

    private T1 view;
    private T2 toMove;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveOperation{T1, T2}"/> class.
    /// </summary>
    /// <param name="arrayGetter">Method for retrieving the Array that should be modified.</param>
    /// <param name="arraySetter">Method to invoke with the new Array order.</param>
    /// <param name="stringGetter">Method for turning an Array element into a string (e.g. for <see cref="Operation.Category"/>).</param>
    /// <param name="design">Wrapper for a <see cref="View"/> of Type <typeparamref name="T1"/> which owns the collection (e.g. <see cref="MenuBar"/>).</param>
    /// <param name="toMove">The Array element to move.</param>
    /// <param name="adjustment">Negative to move left, positive to move right.</param>
    protected MoveOperation(Func<T1, T2[]> arrayGetter, Action<T1, T2[]> arraySetter, Func<T2, string> stringGetter, Design design, T2 toMove, int adjustment)
    {
        if (design.View is not T1 t1)
        {
            throw new ArgumentException($"Design must wrap a {typeof(T1).Name} to be used with this operation.");
        }

        this.view = t1;
        this.toMove = toMove;
        this.stringGetter = stringGetter;
        this.arrayGetter = arrayGetter;
        this.arraySetter = arraySetter;

        var array = arrayGetter(this.view);

        this.originalIdx = Array.IndexOf(array, toMove);

        if (this.originalIdx == -1)
        {
            throw new ArgumentException(nameof(toMove), $"{nameof(toMove)} {typeof(T2).Name} did not belong to the passed {nameof(design)}");
        }

        // calculate new index without falling off array
        this.newIndex = Math.Max(0, Math.Min(array.Length - 1, this.originalIdx + adjustment));

        // they are moving it nowhere?!
        if (this.newIndex == this.originalIdx)
        {
            this.IsImpossible = true;
        }

        this.adjustment = adjustment;
        this.Category = stringGetter(toMove);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this.adjustment == 0)
        {
            return $"Bad Command '{this.GetType().Name}'";
        }

        if (this.adjustment < 0)
        {
            return $"Move '{this.stringGetter(this.toMove)}' Left";
        }

        if (this.adjustment > 0)
        {
            return $"Move '{this.stringGetter(this.toMove)}' Right";
        }

        return base.ToString();
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        var list = this.arrayGetter(this.view).ToList();

        list.Remove(this.toMove);
        list.Insert(this.originalIdx, this.toMove);

        this.arraySetter(this.view, list.Cast<T2>().ToArray());
        this.view.SetNeedsDisplay();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        var list = this.arrayGetter(this.view).ToList();

        list.Remove(this.toMove);
        list.Insert(this.newIndex, this.toMove);

        this.arraySetter(this.view, list.Cast<T2>().ToArray());
        this.view.SetNeedsDisplay();
        return true;
    }
}
