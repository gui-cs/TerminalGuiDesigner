using System.CodeDom;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

internal class MenuBarItemsToCode : ToCodeBase
{
    public Design Design {get;}
    public MenuBar MenuBar {get;}

    public MenuBarItemsToCode(Design design, MenuBar mb)
    {
        this.Design = design;
        this.MenuBar = mb;
    }

    internal void ToCode(CodeDomArgs args)
    {
        /* Make something like this

        MenuItem m1_1 = new MenuItem();

        MenuBarItem m1 = new MenuBarItem();
        m1.Children = new []{m1_1};

        mb.Menus = new []{m1};
        */
        
        // TODO: Let user name these
        List<string> menus = new();
        foreach(var child in MenuBar.Menus)
        {
            ToCode(args,child, out string fieldName);
            menus.Add(fieldName);
        }

        AddPropertyAssignment(args,
            $"this.{Design.FieldName}.{nameof(MenuBar.Menus)}",
            new CodeArrayCreateExpression(typeof(MenuBarItem),
            menus.Select(c=> 
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),c))
                    .ToArray()));
    }

    private void ToCode(CodeDomArgs args, MenuBarItem child, out string fieldName)
    {
        fieldName = GetUniqueFieldName(args, child);
        AddFieldToClass(args,child.GetType(),fieldName);
        AddConstructorCall(args, $"this.{fieldName}",child.GetType());
        AddPropertyAssignment(args,$"this.{fieldName}.{nameof(MenuItem.Title)}",child.Title);

        List<string> children = new();

        // TODO: Make recursive for more children
        // plus again let user name these
        foreach(var sub in child.Children)
        {
            if(sub is MenuBarItem bar)
            {
                ToCode(args,bar,out string f);
                children.Add(f);
            }
            else
            if(sub == null)
            {
                // its a menu seperator (in Terminal.Gui separators are indicated by having a null element).
                children.Add(null);
            }
            else
            {
                string subFieldName = GetUniqueFieldName(args,sub);
                AddFieldToClass(args,sub.GetType(),subFieldName);
                AddConstructorCall(args, $"this.{subFieldName}",sub.GetType());
                AddPropertyAssignment(args,$"this.{subFieldName}.{nameof(MenuItem.Title)}",sub.Title);
                children.Add(subFieldName);
            }
        }

        // we have created fields and constructor calls for our menu
        // now set the menu to an array of all those fields
        AddPropertyAssignment(args,
            $"this.{fieldName}.{nameof(MenuBarItem.Children)}",
            new CodeArrayCreateExpression(typeof(MenuItem),
            children.Select(c=> 
                    
                    // the array elements have null for separator
                    c is null ? new CodePrimitiveExpression(null):

                    // or the name of the field for each menu item
                    (CodeExpression) new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),c))
                    .ToArray()));
        
    }

    public string GetUniqueFieldName(CodeDomArgs args, MenuItem item)
    {
        return args.GetUniqueFieldName(item.Title.ToString());
    }

}