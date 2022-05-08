using System.CodeDom;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

internal class MenuBarItemsToCode : ToCodeBase
{
    public Design Design {get;}
    public MenuBar MenuBar {get;}

    HashSet<string> fieldNamesUsed = new ();


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
        fieldName = GetUniqueFieldName(child);
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
            {
                string subFieldName = GetUniqueFieldName(sub);
                AddFieldToClass(args,sub.GetType(),subFieldName);
                AddConstructorCall(args, $"this.{subFieldName}",sub.GetType());
                AddPropertyAssignment(args,$"this.{subFieldName}.{nameof(MenuItem.Title)}",sub.Title);
                children.Add(subFieldName);
            }
        }

        AddPropertyAssignment(args,
            $"this.{fieldName}.{nameof(MenuBarItem.Children)}",
            new CodeArrayCreateExpression(typeof(MenuItem),
            children.Select(c=> 
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),c))
                    .ToArray()));
        
    }

    public string GetUniqueFieldName(MenuItem item)
    {
        var name = item.Title.ToString();
        
        name = string.IsNullOrWhiteSpace(name) ? "emptyMenu" : name;
        name = Regex.Replace(name,"\\W","");

        if(!fieldNamesUsed.Contains(name))
        {
            fieldNamesUsed.Add(name);
            return name;
        }

        // name is already used, add a number
        int number = 2;
        while (fieldNamesUsed.Contains(name + number))
        {
            // menu2 is taken, try menu3 etc
            number++;
        }

        // found a unique one
        fieldNamesUsed.Add(name + number);
        return name + number;
    }

}