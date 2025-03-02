using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;
using YamlDotNet.Core.Tokens;
using Key = Terminal.Gui.Key;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Static methods for launching popups that illicit choices from user (e.g. <see cref="GetInt(string, string, int?, out int?)"/>).
/// </summary>
public class Modals
{
    /// <summary>
    /// Prompts user to enter a number.
    /// </summary>
    /// <param name="windowTitle">Title for the pop-up.</param>
    /// <param name="entryLabel">Text to show next to the entry field.</param>
    /// <param name="initialValue">Initial value to put into the entry field.</param>
    /// <param name="result">Output value user typed.</param>
    /// <returns>True if user confirmed a choice.</returns>
    public static bool GetInt(string windowTitle, string entryLabel, int? initialValue, out int? result)
    {
        if (GetString(windowTitle, entryLabel, initialValue.ToString(), out var newValue))
        {
            if (string.IsNullOrWhiteSpace(newValue))
            {
                result = null;
                return true;
            }

            if (int.TryParse(newValue, out var r))
            {
                result = r;
                return true;
            }
        }

        result = 0;
        return false;
    }

    internal static bool GetArray(string windowTitle, string entryLabel, Type arrayElement, Array? initialValue, out Array? result)
    {
        var dlg = new GetTextDialog(
            new DialogArgs()
        {
            WindowTitle = windowTitle,
            EntryLabel = entryLabel,
            MultiLine = true,
        },
            initialValue == null ? string.Empty : string.Join('\n', initialValue.ToList().Select(v => v?.ToString() ?? string.Empty)));

        if (dlg.ShowDialog())
        {
            var resultText = dlg.ResultText;

            if (string.IsNullOrWhiteSpace(resultText))
            {
                result = result = Array.CreateInstance(arrayElement, 0);
                return true;
            }

            resultText = resultText.Replace("\r\n", "\n");
            var newValues = resultText.Split('\n');

            result = Array.CreateInstance(arrayElement, newValues.Length);

            for (int i = 0; i < newValues.Length; i++)
            {
                result.SetValue(newValues[i].CastToReflected(arrayElement), i);
            }

            return true;
        }

        result = null;
        return false;
    }

    internal static bool TryGetArray<T>(string windowTitle, string entryLabel, Array? initialValue, out Array? result)
    {
        var dlg = new GetTextDialog(
            new ()
            {
                WindowTitle = windowTitle,
                EntryLabel = entryLabel,
                MultiLine = true,
            },
            initialValue == null ? string.Empty : string.Join('\n', initialValue.ToList().Select(v => v?.ToString() ?? string.Empty)));

        if (dlg.ShowDialog())
        {
            var resultText = dlg.ResultText;

            if (string.IsNullOrWhiteSpace(resultText))
            {
                result = Array.Empty<T>( );
                return true;
            }

            resultText = resultText.Replace("\r\n", "\n");
            var newValues = resultText.Split('\n');

            result = new T[newValues.Length];

            for (int i = 0; i < newValues.Length; i++)
            {
                result.SetValue(newValues[i].CastToReflected(typeof(T)), i);
            }

            return true;
        }

        result = null;
        return false;
    }

    internal static bool GetString(string windowTitle, string entryLabel, string? initialValue, out string? result, bool multiLine = false)
    {
        var dlg = new GetTextDialog(
            new DialogArgs()
        {
            WindowTitle = windowTitle,
            EntryLabel = entryLabel,
            MultiLine = multiLine,
        },
            initialValue);

        if (dlg.ShowDialog())
        {
            result = dlg.ResultText;
            return true;
        }

        result = null;
        return false;
    }

    internal static bool Get<T>(string prompt, string okText, T[] collection, T? currentSelection, out T? selected, bool sort = true)
    {
        return Get(prompt, okText, true, collection, o => o is Type t ? t.Name : o?.ToString() ?? "Null", false, currentSelection, out selected,sort);
    }

    internal static bool Get<T>( string prompt, string okText, in bool addSearch, T[] collection, Func<T?, string> displayMember, bool addNull, [NotNullWhen( true )]T? currentSelection, [NotNullWhen( true )] out T? selected, bool sort=true )
    {
        var pick = new BigListBox<T>( prompt, okText, in addSearch, collection, displayMember, addNull, currentSelection,sort );
        bool toReturn = pick.ShowDialog( );
        selected = pick.Selected;
        return toReturn;
    }

    internal static bool GetEnum(string prompt, string okText, Type enumType, Enum? currentValue, out Enum? result)
    {
        return Get(prompt, okText, true, Enum.GetValues(enumType).Cast<Enum>().ToArray(), o => o?.ToString() ?? "Null", false, currentValue, out result);
    }

    internal static bool GetChar(string windowTitle, string entryLabel, char? oldValue, out char? resultChar)
    {
        if (GetString(windowTitle, entryLabel, oldValue?.ToString() ?? string.Empty, out var result))
        {
            if (result == null || result.Length == 0)
            {
                resultChar = null;
                return true;
            }

            // TODO: pulling first character from what they type is all very
            // well but we should try to actually put a max length on that field
            // so they can't input more than 1 character in the first place
            resultChar = result.First();
            return true;
        }

        resultChar = null;
        return false;
    }

    internal static Terminal.Gui.Key GetShortcut()
    {
        Key key = KeyCode.Null;
        var dlg = new LoadingDialog("Press Shortcut or Del");
        dlg.KeyDown += (s, e) =>
        {
            if (IsValidShortcut(e))
            {
                key = e;
                key.Handled = true;
                Application.RequestStop();
            }
        };
        Application.Run(dlg);

        return key == Key.DeleteChar ? KeyCode.Null : key;
    }

    private static bool IsValidShortcut(Key key)
    {
        if (key.KeyCode == KeyCode.CtrlMask || key.KeyCode == KeyCode.ShiftMask || key.KeyCode == KeyCode.AltMask)
        {
            return false;
        }

        return true;
    }
}
