using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.Generics;
public abstract class GenericArrayElementOperation<T1, T2> : GenericArrayOperation<T1, T2>
    where T1 : View
{
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
    }

    /// <summary>
    /// Gets the array element owned by <typeparamref name="T1"/> <see cref="Design"/>
    /// which will be operated on by this operation.
    /// </summary>
    protected T2 OperateOn { get; }
}
