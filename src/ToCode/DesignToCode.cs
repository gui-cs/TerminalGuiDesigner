using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

internal class DesignToCode : ToCodeBase
{
    public DesignToCode(Design design)
    {
        this.Design = design;
    }

    public Design Design { get; }

    internal void ToCode(CodeDomArgs args, CodeExpression parentView)
    {
        this.AddFieldToClass(args, this.Design);
        var constructorCall = this.GetConstructorCall($"this.{this.Design.FieldName}", this.Design.View.GetType());
        args.InitMethod.Statements.Insert(0, constructorCall);

        foreach (var prop in this.Design.GetDesignableProperties())
        {
            prop.ToCode(args);
        }

        // if the current component is a TableView we should persist the table too
        if (this.Design.View is TableView tv && tv.Table != null)
        {
            var designTable = new DataTableToCode(this.Design);
            designTable.ToCode(args);
        }

        if (this.Design.View is MenuBar)
        {
            var designItems = new MenuBarItemsToCode(this.Design);
            designItems.ToCode(args);
        }

        if (this.Design.View is SpinnerView)
        {
            var spin = new SpinnerViewToCode(this.Design);
            spin.ToCode(args);
        }

        if (this.Design.View is TabView tabView)
        {
            foreach (var tab in tabView.Tabs)
            {
                var designTab = new TabToCode(this.Design, tab);
                designTab.ToCode(args);
            }

            // add call to ApplyStyleChanges();
            this.AddMethodCall(
                args,
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.Design.FieldName),
                nameof(TabView.ApplyStyleChanges));
        }

        if (this.Design.View is StatusBar)
        {
            var designItems = new StatusBarItemsToCode(this.Design);
            designItems.ToCode(args);
        }

        // call this.Add(someView)
        this.AddAddToViewStatement(args, this.Design, parentView);
    }
}
