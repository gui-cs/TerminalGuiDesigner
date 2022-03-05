using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner.Windows;

internal class PosDesigner
{
    internal bool GetPosDesign(Design owner, PropertyInfo property, out PropertyDesign result)
    {
        // pick what type of Pos they want
        if(Modals.Get("Position Type","Pick",Enum.GetValues<PosType>(), out PosType selected))
        {
            switch(selected)
            {
                case PosType.Absolute: 
                        if(Modals.GetInt(property.Name,"Absolute Position",0,out int newPos))
                        {
                            result = new PropertyDesign(newPos.ToString(),(Pos)newPos);
                            return true;
                        }
                        break;
                case PosType.Relative:
                    if (Modals.Get(property.Name, "Relative To",owner.GetSiblings().ToArray(), out Design relativeTo))
                    {
                        if (Modals.Get("Side", "Pick", Enum.GetValues<Side>(), out Side side))
                        {
                            switch(side)
                            {
                                case Side.Above:
                                    result = new PropertyDesign("Pos.Top({0})",Pos.Top(relativeTo.View),()=>relativeTo.FieldName);
                                    break;
                                case Side.Below:
                                    result = new PropertyDesign("Pos.Bottom({0})",Pos.Bottom(relativeTo.View),()=>relativeTo.FieldName);
                                    break;
                                case Side.Left:
                                    result = new PropertyDesign("Pos.Left({0})",Pos.Left(relativeTo.View),()=>relativeTo.FieldName);
                                    break;
                                case Side.Right:
                                    result = new PropertyDesign("Pos.Right({0})",Pos.Right(relativeTo.View),()=>relativeTo.FieldName);
                                    break;
                                default: throw new ArgumentOutOfRangeException(nameof(side));
                            }
                            return true;
                        }
                    }
                    break;
                case PosType.Percent: throw new NotImplementedException();
                case PosType.Anchor: throw new NotImplementedException();

                default : throw new ArgumentOutOfRangeException(nameof(selected));

            }
        }

        result = null;
        return false;
    }
}
