using Terminal.Gui;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

public class RenameMenuItemOperation : MenuItemOperation
{
    private string? originalName;
    private string? newName;

    public RenameMenuItemOperation(MenuItem toRename)
        : base(toRename)
    {
        this.originalName = toRename.Data as string;
    }

    public override bool Do()
    {
        if (this.OperateOn == null)
        {
            return false;
        }

        if (Modals.GetString("Menu Item Name", "Name", this.originalName, out string? newName))
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return false;
            }

            this.newName = CodeDomArgs.MakeValidFieldName(newName);
            this.OperateOn.Data = this.newName;

            return true;
        }

        return false;
    }

    public override void Redo()
    {
        if (this.OperateOn != null)
        {
            this.OperateOn.Data = this.newName;
        }
    }

    public override void Undo()
    {
        if (this.OperateOn != null)
        {
            this.OperateOn.Data = this.originalName;
        }
    }
}
