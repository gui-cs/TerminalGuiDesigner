using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// <see cref="Operation"/> for adding a new <see cref="View"/> to a <see cref="Design"/>.
/// Supports adding to the root or any container view (e.g. <see cref="TabView"/>).
/// </summary>
public class AddViewOperation : Operation
{
    private readonly Design to;
    private View? add;
    private string? fieldName;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddViewOperation"/> class.
    /// When/If run this operation will add <paramref name="add"/> to the <see cref="View"/>
    /// wrapped by <paramref name="to"/> with the provided <paramref name="fieldName"/>.
    /// </summary>
    /// <param name="add">A <see cref="View"/> instance to add.  If you want to pick at runtime then
    /// use <see cref="AddViewOperation(Design)"/> overload instead.</param>
    /// <param name="to">A <see cref="Design"/> (which should be <see cref="Design.IsContainerView"/>)
    /// to add the <paramref name="add"/> to.</param>
    /// <param name="fieldName">Field name to assign to <paramref name="add"/> when wrapping it as a
    /// <see cref="Design"/>.  This determines the private field name that it will have in the .Designer.cs
    /// file.</param>
    public AddViewOperation(View add, Design to, string? fieldName)
    {
        this.add = add;
        this.fieldName = fieldName == null
            ? to.GetUniqueFieldName(add.GetType())
            : to.GetUniqueFieldName(fieldName);
        this.to = to;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddViewOperation"/> class.
    /// This overload asks users what view type they want at runtime (See <see cref="Operation.Do"/>).
    /// </summary>
    /// <param name="design">A <see cref="Design"/> (which should be <see cref="Design.IsContainerView"/>)
    /// to add any newly created <see cref="View"/> to.</param>
    public AddViewOperation(Design design)
    {
        this.to = design;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (this.add == null)
        {
            var selectable = ViewFactory.SupportedViewTypes.ToArray();

            if (Modals.Get("Type of Control", "Add", true, selectable, this.TypeNameDelegate, false, null, out var selected) && selected != null)
            {
                if (selected.IsGenericType)
                {
                    // TODO: Move to some kind of helper class and allow more options later
                    var allowedTTypes = new[] { typeof(int), typeof(string), typeof(float), typeof(double) };

                    if(Modals.Get("Enter a Type for <T>", "Choose", true, allowedTTypes, this.TypeNameDelegate, false, null, out var selectedTType) && selectedTType != null)
                    {
                        selected = selected.MakeGenericType(new[] { selectedTType });
                    }
                    else
                    {
                        return false;
                    }
                }

                this.add = ViewFactory.Create(selected);
                this.fieldName = this.to.GetUniqueFieldName(selected);
            }
        }

        // user canceled picking a type
        if (this.add == null || string.IsNullOrWhiteSpace(this.fieldName))
        {
            return false;
        }

        Design design;
        this.add.Data = design = this.to.CreateSubControlDesign(this.fieldName, this.add);

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

    private string TypeNameDelegate(Type? t)
    {
        if (t == null)
        {
            return "Null";
        }

        return t.Name.Replace("`1", "<T>");
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