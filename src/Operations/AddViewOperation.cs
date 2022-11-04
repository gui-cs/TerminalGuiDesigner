using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;
public class AddViewOperation : Operation
{
    private readonly SourceCodeFile sourceCode;
    private View? add;
    private string? fieldName;
    private readonly Design to;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddViewOperation"/> class.
    /// When/If run this operation will add <paramref name="add"/> to the <see cref="View"/>
    /// wrapped by <paramref name="to"/> with the provided <paramref name="fieldName"/>.
    /// </summary>
    /// <param name="sourceCode"></param>
    /// <param name="add"></param>
    /// <param name="to"></param>
    /// <param name="fieldName"></param>
    public AddViewOperation(SourceCodeFile sourceCode, View add, Design to, string? fieldName)
    {
        this.sourceCode = sourceCode;
        this.add = add;
        this.fieldName = fieldName ?? to.GetUniqueFieldName(add.GetType());
        this.to = to;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddViewOperation"/> class.
    /// This overload asks users what view type they want at runtime (See <see cref="Do"/>).
    /// </summary>
    public AddViewOperation(SourceCodeFile sourceCode, Design design)
    {
        this.sourceCode = sourceCode;
        this.to = design;
    }

    public override bool Do()
    {
        if (this.add == null)
        {
            var factory = new ViewFactory();
            var selectable = factory.GetSupportedViews().ToArray();

            if (Modals.Get("Type of Control", "Add", true, selectable, t => t?.Name ?? "Null", false, out var selected) && selected != null)
            {
                this.add = factory.Create(selected);
                this.fieldName = this.to.GetUniqueFieldName(selected);
            }
        }

        // user canceled picking a type
        if (this.add == null || string.IsNullOrWhiteSpace(this.fieldName))
        {
            return false;
        }

        Design design;
        this.add.Data = design = this.to.CreateSubControlDesign(this.sourceCode, this.fieldName, this.add);

        var v = this.GetViewToAddTo();
        v.Add(this.add);

        if (Application.Driver != null)
        {
            this.add.SetFocus();
        }

        SelectionManager.Instance.ForceSetSelection(design);

        v.SetNeedsDisplay();
        return true;
    }


    public override void Redo()
    {
        if (this.add == null)
        {
            return;
        }

        var v = this.GetViewToAddTo();
        v.Add(this.add);
        v.SetNeedsDisplay();
    }

    public override void Undo()
    {
        if (this.add == null)
        {
            return;
        }

        var v = this.GetViewToAddTo();
        v.Remove(this.add);
        v.SetNeedsDisplay();
    }

    private View GetViewToAddTo()
    {
        if (this.to.View is TabView tabView)
        {
            return tabView.SelectedTab.View;
        }

        return this.to.View;
    }
}