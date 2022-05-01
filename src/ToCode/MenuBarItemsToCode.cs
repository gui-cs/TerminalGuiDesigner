using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode
{
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
            int i = 0;
            foreach(var child in MenuBar.Menus)
            {
                i++;
                string fieldName = $"m{i}";
                AddFieldToClass(args,child.GetType(),fieldName);
                AddConstructorCall(args, $"this.{fieldName}",child.GetType());
                AddPropertyAssignment(args,$"this.{fieldName}.{nameof(MenuItem.Title)}",child.Title);
                
                menus.Add(fieldName);

                List<string> children = new();

                // TODO: Make recursive for more children
                // plus again let user name these
                int j=0;
                foreach(var sub in child.Children)
                {
                    string subFieldName = $"m{i}_{j}";
                    AddFieldToClass(args,sub.GetType(),subFieldName);
                    AddConstructorCall(args, $"this.{subFieldName}",sub.GetType());
                    AddPropertyAssignment(args,$"this.{subFieldName}.{nameof(MenuItem.Title)}",sub.Title);

                    children.Add(subFieldName);
                }

                AddPropertyAssignment(args,
                    $"this.{fieldName}.{nameof(MenuBarItem.Children)}",
                    new CodeArrayCreateExpression(typeof(MenuItem),
                    children.Select(c=> 
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),c))
                            .ToArray()));
            }

            AddPropertyAssignment(args,
                $"this.{Design.FieldName}.{nameof(MenuBar.Menus)}",
                new CodeArrayCreateExpression(typeof(MenuBarItem),
                menus.Select(c=> 
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),c))
                        .ToArray()));
        }
    }
}