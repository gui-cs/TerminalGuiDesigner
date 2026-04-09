using System.CodeDom;
using Terminal.Gui;
using Terminal.Gui.Drivers;
using Terminal.Gui.Views;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Handles generating code for building all the <see cref="MenuItem"/> in
/// a <see cref="MenuBar"/> into .Designer.cs (See <see cref="CodeDomArgs"/>).
/// This will then be assigned to the <see cref="MenuBar.Menus"/> property of the
/// <see cref="MenuBar"/>.
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

        mb.Menus = new []{m1};*/
        
        // TODO: Let user name these
        List<string> menus = new();

        foreach (var child in this.menuBar.SubViews.OfType<MenuBarItem>())
        {
            this.ToCode(args, child, out string fieldName);
            menus.Add(fieldName);


            this.AddMethodCall(args,
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(), this.design.FieldName),
                "Add",
                        // or the name of the field for each menu item
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)
            );
        }
    }
    
    private void ToCode(CodeDomArgs args, MenuItem child, out string fieldName)
    {
        // ------------ Class Fields -------------

        // private Terminal.Gui.Views.MenuBarItem fileMenu;
        fieldName = this.GetUniqueFieldName(args, child);
        this.AddFieldToClass(args, child.GetType(), fieldName);

        // --------- InitializeComponent() ---------

        // this.fileMenu = new Terminal.Gui.Views.MenuBarItem();
        this.AddConstructorCall(args, $"this.{fieldName}", child.GetType());

        // this.fileMenu.Title = "_File";
        this.AddPropertyAssignment(args, $"this.{fieldName}.{nameof(MenuItem.Title)}", child.Title);
        
        // TODO: Verify that all ToString exactly match the static property
        // this.fileMenu.Key = Key.F9;
        this.AddPropertyAssignment(args, $"this.{fieldName}.{nameof(MenuItem.Key)}", 
            new CodeSnippetExpression($"Key.{child.Key}"));


        List<string?> children = new();

        // TODO: Make recursive for more children
        // plus again let user name these
        foreach (var mi in child.GetMenuItems())
        {
            string subFieldName = this.GetUniqueFieldName(args, mi);
            this.AddFieldToClass(args, mi.GetType(), subFieldName);
            this.AddConstructorCall(args, $"this.{subFieldName}", mi.GetType());
            this.AddPropertyAssignment(args, $"this.{subFieldName}.{nameof(MenuItem.Title)}", mi.Title);
            this.AddPropertyAssignment(args, $"this.{subFieldName}.{nameof(MenuItem.Data)}", subFieldName);

            if (mi.Key != KeyCode.Null)
            {
                this.AddPropertyAssignment(
                args,
                $"this.{subFieldName}.{nameof(MenuItem.Key)}",
                new CodeCastExpression(
                    new CodeTypeReference(typeof(KeyCode)),
                    new CodePrimitiveExpression((uint)mi.Key)));
            }
                
            // If it has its own children e.g.
            // File->New->Project
            if (mi.SubMenu != null)
            {
                //ToCode(args, mi, out string subFieldName2);
            }

            children.Add(subFieldName);
            
        }

        // this.fileMenu.PopoverMenu = new PopoverMenu([editMeMenuItem]);
        this.AddPropertyAssignment(args, $"this.{fieldName}.{nameof(MenuBarItem.PopoverMenu)}",
             new CodeSnippetExpression($"new PopoverMenu([{string.Join(",",children)}])"));
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