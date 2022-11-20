using Microsoft.CodeAnalysis.CSharp.Syntax;
using NStack;
using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.TabOperations;

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
    /// <param name="design">Wrapper for a <see cref="View"/> of Type <typeparamref name="T1"/> which owns the collection (e.g. <see cref="MenuBar"/>).</param>
    /// <param name="toMove">The Array element to move.</param>
    /// <param name="adjustment">Negative to move left, positive to move right.</param>
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
        this.originalName = StringGetter(toRename);
        this.newName = newName;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Rename Tab '{this.StringGetter(this.OperateOn)}'";
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.stringSetter(OperateOn, newName);
        this.SetNeedsDisplay();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        this.stringSetter(this.OperateOn, this.originalName);
        this.SetNeedsDisplay();
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
        return true;
    }
}
