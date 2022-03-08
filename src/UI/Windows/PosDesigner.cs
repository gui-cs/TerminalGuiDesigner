using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner.UI.Windows;

public class PosDesigner
{
    public bool GetPosDesign(Design owner, PropertyInfo property, out PropertyDesign result)
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

    private bool DesignPosRelative(Design owner, PropertyInfo property, out PropertyDesign result)
    {
        if (Modals.Get(property.Name, "Relative To", owner.GetSiblings().ToArray(), out Design relativeTo))
        {
            if (Modals.Get("Side", "Pick", Enum.GetValues<Side>(), out Side side))
            {
                if (Modals.GetInt("Offset", "Offset", 0, out int offset))
                {
                    switch (side)
                    {
                        case Side.Above:
                            result = BuildOffsetPos("Pos.Top({0})", Pos.Top(relativeTo.View), offset, () => relativeTo.FieldName);
                            break;
                        case Side.Below:
                            result = BuildOffsetPos("Pos.Bottom({0})", Pos.Bottom(relativeTo.View), offset, () => relativeTo.FieldName);
                            break;
                        case Side.Left:
                            result = BuildOffsetPos("Pos.Left({0})", Pos.Left(relativeTo.View), offset, () => relativeTo.FieldName);
                            break;
                        case Side.Right:
                            result = BuildOffsetPos("Pos.Right({0})", Pos.Right(relativeTo.View), offset, () => relativeTo.FieldName);
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

    private bool DesignPosAbsolute(PropertyInfo property, out PropertyDesign result)
    {

        if (Modals.GetInt(property.Name, "Absolute Position", 0, out int newPos))
        {
            result = new PropertyDesign(newPos.ToString(), (Pos)newPos);
            return true;
        }

        result = null;
        return false;
    }

    private bool DesignPosPercent(PropertyInfo property, out PropertyDesign result)
    {
        if (Modals.GetFloat(property.Name, "Percent(0 - 100)", 0.5f, out float newPercent))
        {
            if (Modals.GetInt("Offset", "Offset", 0, out int offset))
            {
                result = BuildOffsetPos($"Pos.Percent({newPercent})", Pos.Percent(newPercent), offset);
                return true;
            }
        }

        result = null;
        return false;
    }

    private PropertyDesign BuildOffsetPos(string code, Pos pos, int offset, params Func<string>[] codeParameters)
    {
        if (offset == 0)
        {
            return new PropertyDesign(code, pos, codeParameters);
        }
        else
        if (offset > 0)
        {
            return new PropertyDesign($"{code} + {offset}", pos + offset, codeParameters);
        }
        else
        {
            return new PropertyDesign($"{code} - {-offset}", pos - offset, codeParameters);
        }
    }

}
