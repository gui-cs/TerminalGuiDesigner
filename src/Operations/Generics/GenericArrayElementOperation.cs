using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.Generics;

/// <summary>
/// Abstract generic base class for an operation that makes changes to a single element
/// of Type <typeparamref name="T2"/> of the collection hosted by a <see cref="View"/>
/// of Type <typeparamref name="T1"/> (renames, removes etc).
/// </summary>
/// <typeparam name="T1">The <see cref="View"/> type that hosts the collection.</typeparam>
/// <typeparam name="T2">The element type in the collection.</typeparam>
public abstract class GenericArrayElementOperation<T1, T2> : GenericArrayOperation<T1, T2>
    where T1 : View
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericArrayElementOperation{T1, T2}"/> class.
    /// </summary>
    /// <param name="arrayGetter">Method for getting current collection.</param>
    /// <param name="arraySetter">Method for storing new collection.</param>
    /// <param name="stringGetter">Method for turning array element to string.</param>
    /// <param name="design">Wrapper for <see cref="View"/> of type <typeparamref name="T1"/>.</param>
    /// <param name="element">Element to operate on.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="element"/> is not in collection
    /// or <paramref name="design"/> is not wrapping <typeparamref name="T1"/>.</exception>
    public GenericArrayElementOperation(
        ArrayGetterDelegate<T1, T2> arrayGetter,
        ArraySetterDelegate<T1, T2> arraySetter,
        StringGetterDelegate<T2> stringGetter,
        Design design,
        T2 element)
        : base(arrayGetter, arraySetter, stringGetter, design)
    {
        this.OperateOn = element;

        if (!this.ArrayGetter(this.View).Contains(this.OperateOn))
        {
            throw new ArgumentException(nameof(element), $"{nameof(element)} {typeof(T2).Name} did not belong to the passed {nameof(design)}");
        }

        this.Category = stringGetter(this.OperateOn);
    }

    /// <summary>
    /// Gets the array element owned by <typeparamref name="T1"/> <see cref="Design"/>
    /// which will be operated on by this operation.
    /// </summary>
    protected T2 OperateOn { get; }
}
