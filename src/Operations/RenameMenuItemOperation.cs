using Terminal.Gui;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

public class RenameMenuItemOperation : MenuItemOperation
{
    private string? _originalName;
    private string? _newName;

    public RenameMenuItemOperation(MenuItem toRename): base(toRename)
    {
        _originalName = toRename.Data as string;
    }

    public override bool Do()
    {
        if(OperateOn == null)
            return false;

        if (Modals.GetString("Menu Item Name","Name",_originalName,out string? newName))
        {
            if(string.IsNullOrWhiteSpace(newName))
            {
                return false;
            }

            _newName = CodeDomArgs.MakeValidFieldName(newName);
            OperateOn.Data = _newName;
            
            return true;
        }

        return false;
    }

    public override void Redo()
    {
        if(OperateOn != null)
        {
            OperateOn.Data = _newName;
        }
    }

    public override void Undo()
    {
        if(OperateOn != null)
        {
            OperateOn.Data = _originalName;
        }
    }
}
