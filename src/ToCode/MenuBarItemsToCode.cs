using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Handles generating code for building all the <see cref="MenuItem"/> in
/// a <see cref="Terminal.Gui.MenuBar"/> into .Designer.cs (See <see cref="CodeDomArgs"/>).
/// This will then be assigned to the <see cref="MenuBar.Menus"/> property of the
/// <see cref="Terminal.Gui.MenuBar"/>.
/// </summary>
public class MenuBarItemsToCode : ToCodeBase
{
    private readonly Design design;
    private readonly MenuBar menuBar;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuBarItemsToCode"/> class.
    /// </summary>
    /// <param name="design">Wrapper for a <see cref="MenuBar"/> for which you want
    /// to generate CodeDOM code to build all <see cref="MenuItem"/>.</param>
    public MenuBarItemsToCode(Design design)
    {
        this.design = design;

        if (design.View is not MenuBar mb)
        {
            throw new ArgumentException(nameof(design), $"{nameof(MenuBarItemsToCode)} can only be used with {nameof(TerminalGuiDesigner.Design)} that wrap {nameof(MenuBar)}");
        }

        this.menuBar = mb;
    }

    /// <summary>
    /// Adds code to .Designer.cs to construct and initialize all <see cref="MenuItem"/>
    /// in the <see cref="MenuBar"/> (including sub-menus recursively).
    /// </summary>
    /// <param name="args">State object for the .Designer.cs file being generated.</param>
    public void ToCode(CodeDomArgs args)
    {
        /* Make something like this

        MenuItem m1_1 = new MenuItem();

        MenuBarItem m1 = new MenuBarItem();
        m1.Children = new []{m1_1};

        mb.Menus = new []{m1};
        */

        // TODO: Let user name these
        List<string> menus = new();
        foreach (var child in this.menuBar.Menus)
        {
            this.ToCode(args, child, out string fieldName);
            menus.Add(fieldName);
        }

        this.AddPropertyAssignment(
            args,
            $"this.{this.design.FieldName}.{nameof(this.menuBar.Menus)}",
            new CodeArrayCreateExpression(
                typeof(MenuBarItem),
                menus.Select(c =>
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), c))
                    .ToArray()));
    }

    private void ToCode(CodeDomArgs args, MenuBarItem child, out string fieldName)
    {
        fieldName = this.GetUniqueFieldName(args, child);
        this.AddFieldToClass(args, child.GetType(), fieldName);
        this.AddConstructorCall(args, $"this.{fieldName}", child.GetType());
        this.AddPropertyAssignment(args, $"this.{fieldName}.{nameof(MenuItem.Title)}", child.Title);

        List<string?> children = new();

        // TODO: Make recursive for more children
        // plus again let user name these
        foreach (var sub in child.Children)
        {
            if (sub is MenuBarItem bar)
            {
                this.ToCode(args, bar, out string f);
                children.Add(f);
            }
            else
            if (sub == null)
            {
                // its a menu separator (in Terminal.Gui separators are indicated by having a null element).
                children.Add(null);
            }
            else
            {
                string subFieldName = this.GetUniqueFieldName(args, sub);
                this.AddFieldToClass(args, sub.GetType(), subFieldName);
                this.AddConstructorCall(args, $"this.{subFieldName}", sub.GetType());
                this.AddPropertyAssignment(args, $"this.{subFieldName}.{nameof(MenuItem.Title)}", sub.Title);
                this.AddPropertyAssignment(args, $"this.{subFieldName}.{nameof(MenuItem.Data)}", subFieldName);

                if (sub.Shortcut != Key.Null)
                {
                    this.AddPropertyAssignment(
                    args,
                    $"this.{subFieldName}.{nameof(MenuItem.Shortcut)}",
                    new CodeCastExpression(
                        new CodeTypeReference(typeof(Key)),
                        new CodePrimitiveExpression((uint)sub.Shortcut)));
                }

                children.Add(subFieldName);
            }
        }

        // we have created fields and constructor calls for our menu
        // now set the menu to an array of all those fields
        this.AddPropertyAssignment(
            args,
            $"this.{fieldName}.{nameof(MenuBarItem.Children)}",
            new CodeArrayCreateExpression(
                typeof(MenuItem),
                children.Select(c =>

                    // the array elements have null for separator
                    c is null ? new CodePrimitiveExpression() :

                    // or the name of the field for each menu item
                    (CodeExpression)new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), c))
                    .ToArray()));
    }

    private string GetUniqueFieldName(CodeDomArgs args, MenuItem item)
    {
        // if user has an explicit name they have set
        if (item.Data is string s)
        {
            return args.GetUniqueFieldName(s);
        }

        var suffix = item is MenuBarItem ? "Menu" : "MenuItem";

        // Remove underscores from title when generating field name because those indicate shortcut keys
        // and are not rendered (user might not even be aware they are there).
        var title = item.Title.ToString()?.Replace('_', ' ');

        // Make sure name + suffix is unique and not null
        var fname = args.GetUniqueFieldName(title + suffix);

        return fname;
    }
}