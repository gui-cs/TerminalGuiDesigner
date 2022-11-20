namespace TerminalGuiDesigner.Operations.Generics
{
    public delegate T2[] ArrayGetterDelegate<T1, T2>(T1 view);

    public delegate void ArraySetterDelegate<T1, T2>(T1 view, T2[] newArray);

    public delegate string StringGetterDelegate<T>(T arrayElement);
    public delegate void StringSetterDelegate<T>(T arrayElement, string newValue);

    public delegate T2 ArrayElementFactory<T1, T2>(T1 view, string name);
}
