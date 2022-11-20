using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.Generics;

/// <summary>
/// Delegate for getting the current sub-element collection of element
/// Type <typeparamref name="T2"/> from a <typeparamref name="T1"/> <see cref="View"/>.
/// </summary>
public delegate T2[] ArrayGetterDelegate<T1, T2>(T1 view);

/// <summary>
/// Delegate for saving a new ordering (which may have new/less values)
/// of <typeparamref name="T2"/> to a <typeparamref name="T1"/> <see cref="View"/>.
/// </summary>
public delegate void ArraySetterDelegate<T1, T2>(T1 view, T2[] newArray);

/// <summary>
/// Delegate for getting the current name from an array element.
/// </summary>
public delegate string StringGetterDelegate<T>(T arrayElement);

/// <summary>
/// Delegate for renaming an array element.
/// </summary>
public delegate void StringSetterDelegate<T>(T arrayElement, string newValue);

/// <summary>
/// Delegate for creating new elements of type <typeparamref name="T2"/>.
/// </summary>
public delegate T2 ArrayElementFactory<T1, T2>(T1 view, string name);
