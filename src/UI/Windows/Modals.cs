﻿using System.Linq;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI.Windows;

public class Modals
{

    public static bool GetInt(string windowTitle, string entryLabel, int? initialValue, out int? result)
    {
        if (GetString(windowTitle, entryLabel, initialValue.ToString(), out var newValue))
        {
            if(string.IsNullOrWhiteSpace(newValue))
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

    public static bool GetFloat(string windowTitle, string entryLabel, float? initialValue, out float? result)
    {
        if (GetString(windowTitle, entryLabel, initialValue.ToString(), out var newValue))
        {
            if(string.IsNullOrWhiteSpace(newValue))
            {
                result = null;
                return true;
            }

            if (float.TryParse(newValue, out var r))
            {
                result = r;
                return true;
            }
        }

        result = 0;
        return false;
    }

    public static bool GetArray(string windowTitle, string entryLabel, Type arrayElement, Array? initialValue, out Array? result)
    {
        var dlg = new GetTextDialog(new DialogArgs()
        {
            WindowTitle = windowTitle,
            EntryLabel = entryLabel,
            MultiLine = true,
        }, initialValue == null ? "" : string.Join('\n',initialValue.ToList().Select(v=>v?.ToString() ?? "")));

        if (dlg.ShowDialog())
        {
            var resultText = dlg.ResultText;

            if(string.IsNullOrWhiteSpace(resultText))
            {
                result = result = Array.CreateInstance(arrayElement, 0);
                return true;
            }

            resultText = resultText.Replace("\r\n", "\n");
            var newValues = resultText.Split('\n');

            result = Array.CreateInstance(arrayElement,newValues.Length);

            for(int i=0;i<newValues.Length;i++)
                result.SetValue(newValues[i].CastToReflected(arrayElement),i);

            return true;
        }

        result = null;
        return false;
    }
    public static bool GetString(string windowTitle, string entryLabel, string? initialValue, out string? result)
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

    public static bool Get<T>(string prompt, string okText, T[] collection, out T? selected)
    {
        return Get(prompt, okText, true, collection, o => o?.ToString() ?? "Null", false, out selected);
    }

    public static bool Get<T>(string prompt, string okText, bool addSearch, T[] collection, Func<T?, string> displayMember, bool addNull, out T? selected)
    {
        var pick = new BigListBox<T>(prompt, okText, addSearch, collection, displayMember, addNull);
        bool toReturn = pick.ShowDialog();
        selected = pick.Selected;
        return toReturn;
    }

    internal static bool GetEnum(string prompt, string okText, Type enumType, out Enum? result)
    {       
        return Get(prompt,okText,true,Enum.GetValues(enumType).Cast<Enum>().ToArray(),o=>o?.ToString() ?? "Null",false,out result);
    }
}
