using System.Collections;
using System.Collections.ObjectModel;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extensions methods for the <see cref="Array"/> class.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Converts an <see cref="Array"/> to list of objects.
    /// </summary>
    /// <param name="a">An array to convert.</param>
    /// <returns>The array as a list.</returns>
    public static List<object?> ToList(this Array a)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        var toReturn = new List<object?>();

        for (int i = 0; i < a.Length; i++)
        {
            toReturn.Add(a.GetValue(i));
        }

        return toReturn;
    }
    
    public static IListDataSource ToListDataSource(this IEnumerable enumerable)
    {
        // Get the type of the elements
        var elementType = enumerable.GetType().GetElementType() ?? enumerable.GetType().GetGenericArguments().FirstOrDefault();
        if (elementType == null)
        {
            throw new Exception("Unable to get element type for collection");
        }

        // Convert the enumerable to an ObservableCollection<T>
        var observableCollectionType = typeof(ObservableCollection<>).MakeGenericType(elementType);
        var list = Activator.CreateInstance(observableCollectionType, enumerable);

        // Create an instance of ListWrapper<T>
        var listWrapperType = typeof(ListWrapper<>).MakeGenericType(elementType);
        return (IListDataSource)Activator.CreateInstance(listWrapperType, list);
    }

}
