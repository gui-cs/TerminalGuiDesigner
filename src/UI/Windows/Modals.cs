using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI.Windows;

public class Modals
{

    public static bool GetInt(string windowTitle, string entryLabel, int initialValue, out int result)
    {
        if (GetString(windowTitle, entryLabel, initialValue.ToString(), out string newValue))
        {
            if (int.TryParse(newValue, out result))
            {
                return true;
            }
        }

        result = 0;
        return false;
    }

    // TODO : Can this be generic
    public static bool GetFloat(string windowTitle, string entryLabel, float initialValue, out float result)
    {
        if (GetString(windowTitle, entryLabel, initialValue.ToString(), out string newValue))
        {
            if (float.TryParse(newValue, out result))
            {
                return true;
            }
        }

        result = 0;
        return false;
    }
    public static bool GetString(string windowTitle, string entryLabel, string initialValue, out string? result)
    {
        var dlg = new GetTextDialog(new DialogArgs()
        {
            WindowTitle = windowTitle,
            EntryLabel = entryLabel,
        }, initialValue);

        if (dlg.ShowDialog())
        {
            result = dlg.ResultText;
            return true;
        }

        result = null;
        return false;
    }

    public static bool Get<T>(string prompt, string okText, T[] collection, out T selected)
    {
        return Get(prompt, okText, true, collection, o => o?.ToString() ?? "Null", false, out selected);
    }


    public static bool Get<T>(string prompt, string okText, bool addSearch, T[] collection, Func<T, string> displayMember, bool addNull, out T selected)
    {
        var pick = new BigListBox<T>(prompt, okText, addSearch, collection, displayMember, addNull);
        bool toReturn = pick.ShowDialog();
        selected = pick.Selected;
        return toReturn;
    }
}
