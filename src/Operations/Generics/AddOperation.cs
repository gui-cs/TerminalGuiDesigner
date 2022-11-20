using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.Generics;

public abstract class AddOperation<T1, T2> : GenericArrayOperation<T1, T2>
    where T1 : View
{
    private T2? newItem;
    private string? name;

    public ArrayElementFactory<T1, T2> ElementFactory { get; }

    public AddOperation(
        ArrayGetterDelegate<T1,T2> arrayGetter,
        ArraySetterDelegate<T1, T2> arraySetter,
        StringGetterDelegate<T2> stringGetter,
        ArrayElementFactory<T1, T2> elementFactory,
        Design design,
        string? name)
        : base(arrayGetter, arraySetter, stringGetter, design)
    {
        this.name = name;
        this.ElementFactory = elementFactory;
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        this.Remove(this.newItem);
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        this.Add(this.newItem);
    }

    /// <summary>
    /// Performs the operation.  Adding a new top level menu.
    /// </summary>
    /// <remarks>Calling this method multiple times will not result in more new menus.</remarks>
    /// <returns>True if the menu was added.  False if making repeated calls or user cancels naming the new menu etc.</returns>
    protected override bool DoImpl()
    {
        // if we have already run this operation
        if (this.newItem != null)
        {
            return false;
        }

        string? uniqueName = this.name;

        if (uniqueName == null)
        {
            if (!Modals.GetString("Name", "Name", $"My{typeof(T2).Name}", out uniqueName))
            {
                // user canceled adding
                return false;
            }
        }

        uniqueName = uniqueName.MakeUnique(
            this.ArrayGetter(this.View).Select(v => this.StringGetter(v))
                .Where(t => t != null)
                .Cast<string>());

        this.newItem = this.ElementFactory(this.View, uniqueName);
        this.Add(this.newItem);
        return true;
    }

}
