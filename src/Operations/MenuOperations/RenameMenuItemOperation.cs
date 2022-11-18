using Terminal.Gui;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.MenuOperations;

/// <summary>
/// Renames a <see cref="MenuItem"/>.  Normally names are automatically
/// generated based on <see cref="MenuItem.Title"/>.  If this operation is
/// used then a user provided name will be used instead.  This name will
/// become the private field of the class being designed when written out
/// to a .Designer.cs file.
/// </summary>
public class RenameMenuItemOperation : MenuItemOperation
{
    private string? originalName;
    private string? newName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameMenuItemOperation"/> class.
    /// Note that this operation renames the field name in .Designer.cs not the <see cref="MenuItem.Title"/>.
    /// </summary>
    /// <param name="toRename">The column to choose a new private field name for.</param>
    public RenameMenuItemOperation(MenuItem toRename)
        : base(toRename)
    {
        this.originalName = toRename.Data as string;
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        if (this.OperateOn != null)
        {
            this.OperateOn.Data = this.newName;
        }
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (this.OperateOn != null)
        {
            this.OperateOn.Data = this.originalName;
        }
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.OperateOn == null)
        {
            return false;
        }

        // TODO: make this an optional constructor field so it can be unit tested
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
}
