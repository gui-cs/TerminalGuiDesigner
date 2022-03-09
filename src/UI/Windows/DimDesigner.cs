using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.UI.Windows;

public class DimDesigner
{
    public bool GetDimDesign(Property property, out SnippetProperty result)
    {

        // pick what type of Pos they want
        if (Modals.Get("Dim Type", "Pick", Enum.GetValues<DimType>(), out DimType selected))
        {
            switch (selected)
            {
                case DimType.Absolute:
                    return DesignDimAbsolute(property, out result);
                case DimType.Percent:
                    return DesignDimPercent(property, out result);
                case DimType.Fill:
                    return DesignDimFill(property, out result);

                default: throw new ArgumentOutOfRangeException(nameof(selected));

            }
        }

        result = null;
        return false;
    }

    private bool DesignDimAbsolute(Property property, out SnippetProperty result)
    {

        if (Modals.GetInt(property.PropertyInfo.Name, "Absolute Size", 0, out int newDim))
        {
            result = new SnippetProperty(property, newDim.ToString(), (Dim)newDim);
            return true;
        }

        result = null;
        return false;
    }

    private bool DesignDimPercent(Property property, out SnippetProperty result)
    {
        if (Modals.GetFloat(property.PropertyInfo.Name, "Percent (0-100)", 50f, out float newPercent))
        {
            if (Modals.GetInt("Offset", "Offset", 0, out int offset))
            {
                result = BuildOffsetDim(property, $"Dim.Percent({newPercent})", Dim.Percent(newPercent), offset);
                return true;
            }
        }

        result = null;
        return false;
    }

    private bool DesignDimFill(Property property, out SnippetProperty result)
    {
        if (Modals.GetInt(property.PropertyInfo.Name, "Margin", 0, out int margin))
        {
            result = new SnippetProperty(property,$"Dim.Fill({margin})", Dim.Fill(margin));
            return true;
        }

        result = null;
        return false;
    }
    private SnippetProperty BuildOffsetDim(Property property, string code, Dim dim, int offset, params Func<string>[] codeParameters)
    {
        if (offset == 0)
        {
            return new SnippetProperty(property, code, dim, codeParameters);
        }
        else
        if (offset > 0)
        {
            return new SnippetProperty(property, $"{code} + {offset}", dim + offset, codeParameters);
        }
        else
        {
            return new SnippetProperty(property, $"{code} - {-offset}", dim - offset, codeParameters);
        }
    }
}
