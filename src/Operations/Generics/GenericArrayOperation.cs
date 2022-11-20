using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.Generics;

/// <summary>
/// Generic abstract base class for operations which modify an array of elements
/// of Type <typeparamref name="T2"/> hosted by a <see cref="View"/> of Type
/// <typeparamref name="T1"/>.
/// </summary>
/// <typeparam name="T1">The <see cref="View"/> Type that hosts the collection.</typeparam>
/// <typeparam name="T2">The array element type.</typeparam>
public abstract class GenericArrayOperation<T1, T2> : GenericOperation<T1>
    where T1 : View
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericArrayOperation{T1, T2}"/> class.
    /// </summary>
    /// <param name="arrayGetter">Method for getting the current collection.</param>
    /// <param name="arraySetter">Method for setting the new collection.</param>
    /// <param name="stringGetter">Method for turning an element into a user readable string (Name, Title etc).</param>
    /// <param name="design">Wrapper for a <see cref="View"/> of Type <typeparamref name="T1"/>.</param>
    public GenericArrayOperation(
        ArrayGetterDelegate<T1, T2> arrayGetter,
        ArraySetterDelegate<T1, T2> arraySetter,
        StringGetterDelegate<T2> stringGetter,
        Design design)
        : base(design)
    {
        this.StringGetter = stringGetter;
        this.ArrayGetter = arrayGetter;
        this.ArraySetter = arraySetter;
    }

    /// <summary>
    /// Gets the method for retrieving the array of <typeparamref name="T2"/> elements from
    /// <typeparamref name="T1"/>.
    /// </summary>
    protected ArrayGetterDelegate<T1, T2> ArrayGetter { get; }

    /// <summary>
    /// Gets the method for writing a new array of <typeparamref name="T2"/> elements to <typeparamref name="T1"/>.
    /// </summary>
    protected ArraySetterDelegate<T1, T2> ArraySetter { get; }

    /// <summary>
    /// Gets the method for converting an array element to a human readable string.
    /// </summary>
    protected StringGetterDelegate<T2> StringGetter { get; }

    /// <summary>
    /// Adds <paramref name="newItem"/> to <see cref="View"/>.
    /// </summary>
    /// <param name="newItem">The array element to add.</param>
    protected virtual void Add(T2? newItem)
    {
        var current = this.ArrayGetter(this.View).ToList();

        // its already there
        if (newItem == null || current.Contains(newItem))
        {
            return;
        }

        current.Add(newItem);
        this.ArraySetter(this.View, current.Cast<T2>().ToArray());
        this.SetNeedsDisplay();
    }

    /// <summary>
    /// Calls any update/refresh status code that is needed after making changes to collection.
    /// Default implementation just calls <see cref="View.SetNeedsDisplay()"/> on <see cref="View"/>.
    /// </summary>
    protected virtual void SetNeedsDisplay()
    {
        this.View.SetNeedsDisplay();
    }

    /// <summary>
    /// Removes <paramref name="toRemove"/> from <see cref="View"/>.
    /// </summary>
    /// <param name="toRemove">Array element to remove.</param>
    /// <returns>True if element was removed.  False if element was null or not in collection.</returns>
    protected bool Remove(T2? toRemove)
    {
        var current = this.ArrayGetter(this.View).ToList();

        // its not there anyways
        if (toRemove == null || !current.Contains(toRemove))
        {
            return false;
        }

        current.Remove(toRemove);
        this.ArraySetter(this.View, current.Cast<T2>().ToArray());
        this.SetNeedsDisplay();

        return true;
    }
}
