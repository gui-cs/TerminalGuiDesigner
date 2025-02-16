using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.Generics;

/// <summary>
/// Generic abstract base class for an <see cref="Operation"/> that will rename a single
/// array element of Type <typeparamref name="T2"/> in the collection hosted by a <see cref="View"/>
/// of Type <typeparamref name="T1"/>.
/// </summary>
/// <typeparam name="T1">Type of <see cref="View"/> that hosts the collection.</typeparam>
/// <typeparam name="T2">Type of array element that is to be renamed (e.g. <see cref="Tab"/>).</typeparam>
public abstract class RenameOperation<T1, T2> : GenericArrayElementOperation<T1, T2>
    where T1 : View
{
    private readonly StringSetterDelegate<T2> stringSetter;
    private readonly string originalName;
    private string? newName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameOperation{T1, T2}"/> class.
    /// </summary>
    /// <param name="arrayGetter">Method for retrieving the Array that should be modified.</param>
    /// <param name="arraySetter">Method to invoke with the new Array order.</param>
    /// <param name="stringGetter">Method for turning an Array element into a string (e.g. for <see cref="Operation.Category"/>).</param>
    /// <param name="stringSetter">Method for updating the 'name' of <paramref name="toRename"/>.</param>
    /// <param name="design">Wrapper for a <see cref="View"/> of Type <typeparamref name="T1"/> which owns the collection (e.g. <see cref="MenuBar"/>).</param>
    /// <param name="toRename">Element to rename.</param>
    /// <param name="newName">New name to use or null to prompt user for name.</param>
    protected RenameOperation(
        ArrayGetterDelegate<T1, T2> arrayGetter,
        ArraySetterDelegate<T1, T2> arraySetter,
        StringGetterDelegate<T2> stringGetter,
        StringSetterDelegate<T2> stringSetter,
        Design design,
        T2 toRename,
        string? newName)
        : base(arrayGetter, arraySetter, stringGetter, design, toRename)
    {
        this.stringSetter = stringSetter;
        this.originalName = this.StringGetter(toRename);
        this.newName = newName;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Rename {typeof(T2).Name} '{this.StringGetter(this.OperateOn)}'";
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        if (this.newName == null)
        {
            return;
        }

        this.stringSetter(this.OperateOn, this.newName);
        this.SetNeedsDraw();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        this.stringSetter(this.OperateOn, this.originalName);
        this.SetNeedsDraw();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (string.IsNullOrWhiteSpace(this.newName))
        {
            if (Modals.GetString($"Rename {typeof(T2).Name}", "Name", this.originalName?.ToString(), out string? n) && n != null)
            {
                this.newName = n;
            }
            else
            {
                // user canceled
                return false;
            }
        }

        this.stringSetter(this.OperateOn, this.newName);
        this.SetNeedsDraw();

        return true;
    }
}
