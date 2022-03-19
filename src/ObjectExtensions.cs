using System.Reflection;

namespace TerminalGuiDesigner;

public static class ObjectExtensions
{
    public static T CastTo<T>(this object o) => (T)o;

    public static dynamic CastToReflected(this object o, Type type)
    {
        var methodInfo = typeof(ObjectExtensions).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o });
    }
}
