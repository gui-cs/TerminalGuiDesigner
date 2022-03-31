using System.Reflection;
using NStack;

namespace TerminalGuiDesigner;

public static class ObjectExtensions
{
    public static T CastTo<T>(this object o) => (T)o;

    public static dynamic CastToReflected(this object o, Type type)
    {
        if(o is string s && type == typeof(ustring))
            return ustring.Make(s);

        var methodInfo = typeof(ObjectExtensions).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o }) ?? throw new Exception("Expected genericMethodInfo CastTo<T> to have a non null return value");
    }
}
