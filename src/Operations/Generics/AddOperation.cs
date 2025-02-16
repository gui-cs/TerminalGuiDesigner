using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.Generics;

/// <summary>
/// Generic abstract class for an <see cref="Operation"/> which adds a new element
/// to a collection hosted by a <see cref="View"/> e.g. a new <see cref="Tab"/>
/// to a <see cref="TabView"/>.
/// </summary>
/// <typeparam name="T1">The <see cref="View"/> that hosts the collection you want to modify.</typeparam>
/// <typeparam name="T2">The element <see cref="Type"/> that makes up the collection, that is being added to.</typeparam>
public abstract class AddOperation<T1, T2> : GenericArrayOperation<T1, T2>
    where T1 : View
{
    private readonly ArrayElementFactory<T1, T2> elementFactory;

    private T2? newItem;
    private string? name;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddOperation{T1, T2}"/> class.
    /// </summary>
    /// <param name="arrayGetter">Method to get the current collection.</param>
    /// <param name="arraySetter">Method to save the new collection to the <typeparamref name="T1"/>.</param>
    /// <param name="stringGetter">Method to turn an element into a string (e.g. for ToString()).</param>
    /// <param name="elementFactory">Method for creating a new element with the given name.</param>
    /// <param name="design">Wrapper for a <see cref="View"/> of type <typeparamref name="T1"/>.</param>
    /// <param name="name">The name to use for the new object or null to prompt user at runtime.</param>
    public AddOperation(
        ArrayGetterDelegate<T1, T2> arrayGetter,
        ArraySetterDelegate<T1, T2> arraySetter,
        StringGetterDelegate<T2> stringGetter,
        ArrayElementFactory<T1, T2> elementFactory,
        Design design,
        string? name)
        : base(arrayGetter, arraySetter, stringGetter, design)
    {
        this.name = name;
        this.elementFactory = elementFactory;
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

        this.newItem = this.elementFactory(this.View, uniqueName);
        this.Add(this.newItem);
        this.SetNeedsDraw();
        return true;
    }
}