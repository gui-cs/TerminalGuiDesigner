using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.UI.Windows;

public class PosDesigner
{
    public bool GetPosDesign(Design owner, Property property, out SnippetProperty result)
    {
        // pick what type of Pos they want
        if (Modals.Get("Position Type", "Pick", Enum.GetValues<PosType>(), out PosType selected))
        {
            switch (selected)
            {
                case PosType.Absolute:
                    return DesignPosAbsolute(property, out result);
                case PosType.Relative:
                    return DesignPosRelative(owner, property, out result);

                case PosType.Percent:
                    return DesignPosPercent(property, out result);
                case PosType.Anchor: throw new NotImplementedException();

                default: throw new ArgumentOutOfRangeException(nameof(selected));

            }
        }

        result = null;
        return false;
    }

    private bool DesignPosRelative(Design owner, Property property, out SnippetProperty result)
    {
        if (Modals.Get(property.PropertyInfo.Name, "Relative To", owner.GetSiblings().ToArray(), out Design relativeTo))
        {
            if (Modals.Get("Side", "Pick", Enum.GetValues<Side>(), out Side side))
            {
                if (Modals.GetInt("Offset", "Offset", 0, out int offset))
                {
                    switch (side)
                    {
                        case Side.Above:
                            result = BuildOffsetPos(property,"Pos.Top({0})", Pos.Top(relativeTo.View), offset, () => relativeTo.FieldName);
                            break;
                        case Side.Below:
                            result = BuildOffsetPos(property, "Pos.Bottom({0})", Pos.Bottom(relativeTo.View), offset, () => relativeTo.FieldName);
                            break;
                        case Side.Left:
                            result = BuildOffsetPos(property, "Pos.Left({0})", Pos.Left(relativeTo.View), offset, () => relativeTo.FieldName);
                            break;
                        case Side.Right:
                            result = BuildOffsetPos(property, "Pos.Right({0})", Pos.Right(relativeTo.View), offset, () => relativeTo.FieldName);
                            break;
                        default: throw new ArgumentOutOfRangeException(nameof(side));
                    }

                    return true;
                }
            }
        }

        result = null;
        return false;
    }

    private bool DesignPosAbsolute(Property property, out SnippetProperty result)
    {

        if (Modals.GetInt(property.PropertyInfo.Name, "Absolute Position", 0, out int newPos))
        {
            result = new SnippetProperty(property, newPos.ToString(), (Pos)newPos);
            return true;
        }

        result = null;
        return false;
    }

    private bool DesignPosPercent(Property property, out SnippetProperty result)
    {
        if (Modals.GetFloat(property.PropertyInfo.Name, "Percent(0 - 100)", 0.5f, out float newPercent))
        {
            if (Modals.GetInt("Offset", "Offset", 0, out int offset))
            {
                result = BuildOffsetPos(property,$"Pos.Percent({newPercent})", Pos.Percent(newPercent), offset);
                return true;
            }
        }

        result = null;
        return false;
    }

    private SnippetProperty BuildOffsetPos(Property property, string code, Pos pos, int offset, params Func<string>[] codeParameters)
    {
        if (offset == 0)
        {
            return new SnippetProperty(property, code, pos, codeParameters);
        }
        else
        if (offset > 0)
        {
            return new SnippetProperty(property, $"{code} + {offset}", pos + offset, codeParameters);
        }
        else
        {
            return new SnippetProperty(property, $"{code} - {-offset}", pos - offset, codeParameters);
        }
    }

}
