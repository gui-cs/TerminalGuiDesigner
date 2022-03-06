using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner.Windows;

internal class DimDesigner
{
    internal bool GetDimDesign(PropertyInfo property, out PropertyDesign result)
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

    private bool DesignDimAbsolute(PropertyInfo property, out PropertyDesign result)
    {

        if (Modals.GetInt(property.Name, "Absolute Size", 0, out int newDim))
        {
            result = new PropertyDesign(newDim.ToString(), (Dim)newDim);
            return true;
        }

        result = null;
        return false;
    }

    private bool DesignDimPercent(PropertyInfo property, out PropertyDesign result)
    {
        if (Modals.GetFloat(property.Name, "Percent (0-100)", 50f, out float newPercent))
        {
            if (Modals.GetInt("Offset", "Offset", 0, out int offset))
            {
                result = BuildOffsetDim($"Dim.Percent({newPercent})", Dim.Percent(newPercent), offset);
                return true;
            }
        }

        result = null;
        return false;
    }

    private bool DesignDimFill(PropertyInfo property, out PropertyDesign result)
    {
        if (Modals.GetInt(property.Name, "Margin", 0, out int margin))
        {
            result = new PropertyDesign($"Dim.Fill({margin})", Dim.Fill(margin));
            return true;
        }

        result = null;
        return false;
    }
    private PropertyDesign BuildOffsetDim(string code, Dim dim, int offset, params Func<string>[] codeParameters)
    {
        if (offset == 0)
        {
            return new PropertyDesign(code, dim, codeParameters);
        }
        else
        if (offset > 0)
        {
            return new PropertyDesign($"{code} + {offset}", dim + offset, codeParameters);
        }
        else
        {
            return new PropertyDesign($"{code} - {-offset}", dim - offset, codeParameters);
        }
    }
}
