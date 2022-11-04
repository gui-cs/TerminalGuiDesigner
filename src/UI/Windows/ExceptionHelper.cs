using System.Reflection;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Helper for unwrapping Exception.InnerExceptions and ReflectionTypeLoadExceptions.LoaderExceptions into a single flat message string of all errors.
/// </summary>
public static class ExceptionHelper
{
    public static string ExceptionToListOfInnerMessages(Exception e, bool includeStackTrace = false)
    {
        string message = e.Message;
        if (includeStackTrace)
        {
            message += Environment.NewLine + e.StackTrace;
        }

        if (e is ReflectionTypeLoadException)
        {
            foreach (Exception? loaderException in ((ReflectionTypeLoadException)e).LoaderExceptions)
            {
                if (loaderException != null)
                {
                    message += Environment.NewLine + ExceptionToListOfInnerMessages(loaderException, includeStackTrace);
                }
            }
        }

        if (e.InnerException != null)
        {
            message += Environment.NewLine + ExceptionToListOfInnerMessages(e.InnerException, includeStackTrace);
        }

        return message;
    }

    /// <summary>
    /// Returns the first base Exception in the AggregateException.InnerExceptions list which is of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <returns></returns>
    public static T? GetExceptionIfExists<T>(this AggregateException e) where T : Exception
    {
        return e.Flatten().InnerExceptions.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Returns the first InnerException of type T in the Exception or null.
    /// 
    /// <para>If e is T then e is returned directly</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <returns></returns>
    public static T? GetExceptionIfExists<T>(this Exception e) where T : Exception
    {
        if (e is T)
        {
            return (T)e;
        }

        if (e.InnerException != null)
        {
            return GetExceptionIfExists<T>(e.InnerException);
        }

        return null;
    }
}
