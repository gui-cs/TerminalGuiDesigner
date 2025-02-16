using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.Generics;

/// <summary>
/// Operation that removes a single element of Type <typeparamref name="T2"/>
/// from the collection hosted by a <see cref="View"/> of Type <typeparamref name="T1"/>.
/// </summary>
/// <typeparam name="T1">The Type of <see cref="View"/> that hosts the collection.</typeparam>
/// <typeparam name="T2">Array element Type.</typeparam>
public abstract class RemoveOperation<T1, T2> : GenericArrayElementOperation<T1, T2>
    where T1 : View
{
    private readonly int idx;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveOperation{T1, T2}"/> class.
    /// </summary>
    /// <param name="arrayGetter">Method for getting the current collection.</param>
    /// <param name="arraySetter">Method for setting a new collection on a <see cref="View"/> of Type <typeparamref name="T1"/>.</param>
    /// <param name="stringGetter">Method for getting the 'name' from an element (Name, Title etc).</param>
    /// <param name="design">Wrapper for a <see cref="View"/> of Type <typeparamref name="T1"/> which owns the collection (e.g. <see cref="MenuBar"/>).</param>
    /// <param name="toRemove">Element to remove.</param>
    public RemoveOperation(
        ArrayGetterDelegate<T1, T2> arrayGetter,
        ArraySetterDelegate<T1, T2> arraySetter,
        StringGetterDelegate<T2> stringGetter,
        Design design,
        T2 toRemove)
        : base(arrayGetter, arraySetter, stringGetter, design, toRemove)
    {
        this.idx = Array.IndexOf(arrayGetter(this.View), toRemove);
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        // its not there anyways
        if (this.OperateOn == null)
        {
            return;
        }

        var current = this.ArrayGetter(this.View).Cast<T2>().ToList();
        current.Insert(this.idx, this.OperateOn);
        this.ArraySetter(this.View, current.ToArray());
        this.SetNeedsDraw();
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Do();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Remove {typeof(T2).Name} '{this.StringGetter(this.OperateOn)}'";
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        return this.Remove(this.OperateOn);
    }
}
