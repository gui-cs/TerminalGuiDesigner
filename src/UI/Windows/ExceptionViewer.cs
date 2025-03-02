using System.Text.RegularExpressions;
using Terminal.Gui;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Popup dialog that shows an <see cref="Exception"/> including toggle for <see cref="Exception.StackTrace"/>.
/// </summary>
public class ExceptionViewer
{
    /// <summary>
    /// Launches a new modal instance of <see cref="ExceptionViewer"/> showing <paramref name="exception"/>.
    /// </summary>
    /// <param name="errorText">Message that describes what was going on when <paramref name="exception"/>
    /// occurred (e.g. "Could not open file x").</param>
    /// <param name="exception"><see cref="Exception"/> to show.</param>
    public static void ShowException(string errorText, Exception exception)
    {
        var msg = GetExceptionText(errorText, exception, false);

        var textView = new TextView()
        {
            Text = msg,
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 2,
            ReadOnly = true,
            AllowsTab = false,
        };

        bool toggleStack = true;

        var btnOk = new Button()
        {
            Text = "Ok",
            IsDefault = true
        };

        btnOk.Accepting += (s, e) =>
        {
            e.Cancel = true;
            Application.RequestStop();
        };
        var btnStack = new Button()
        {
            Text = "Stack"
        };
        btnStack.Accepting += (s, e) =>
        {
            e.Cancel = true;
            // flip between stack / no stack
            textView.Text = GetExceptionText(errorText, exception, toggleStack);
            textView.SetNeedsDraw();
            toggleStack = !toggleStack;
        };

        var dlg = new Dialog()
        {
            Title = "Error",
            X = Pos.Percent(10),
            Y = Pos.Percent(10),
            Width = Dim.Percent(80),
            Height = Dim.Percent(80),
            Buttons = new[] { btnOk, btnStack }
        };
        dlg.Add(textView);

        Application.Run(dlg);
    }

    private static string GetExceptionText(string errorText, Exception exception, bool includeStackTrace)
    {
        return Wrap(errorText + "\n" + ExceptionHelper.ExceptionToListOfInnerMessages(exception, includeStackTrace), 76);
    }

    private static string Wrap(string longString, int width)
    {
        return string.Join("\n", Regex.Matches(longString, ".{1," + width + "}").Select(m => m.Value).ToArray());
    }
}
