using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.Operations
{
    public class RemoveMenuItemOperation : MenuItemOperation
    {
        private int _removedAtIdx;

        /// <summary>
        /// If as a result of removing this menu item any
        /// top level menu items were also removed (for being
        /// empty). Track indexes here so we can undo if necessary
        /// </summary>
        private Dictionary<int, MenuBarItem>? prunedEmptyTopLevelMenus;

        /// <summary>
        /// True if as a result of removing a this menu item any
        /// top level menu items were also removed (for being empty)
        /// </summary>
        public bool PrunedTopLevelMenu => prunedEmptyTopLevelMenus != null && prunedEmptyTopLevelMenus.Any();

        public RemoveMenuItemOperation(MenuItem toRemove): base(toRemove)
        {
        }
        

        public override bool Do()
        {
            if(Parent == null || OperateOn == null)
                return false;

            var children = Parent.Children.ToList<MenuItem>();
                
            _removedAtIdx = Math.Max(0,children.IndexOf(OperateOn));
            
            children.Remove(OperateOn);
            Parent.Children = children.ToArray();
            Bar?.SetNeedsDisplay();

            MenuTracker.Instance.ConvertEmptyMenus();


            // if a top level menu now has no children 
            if(Bar != null)
            {
                var empty = Bar.Menus.Where(bi => bi.Children.Length == 0).ToArray();
                if(empty.Any())
                {
                    // remember where they were
                    prunedEmptyTopLevelMenus = empty.ToDictionary(e=>Array.IndexOf(Bar.Menus,e),v=>v);
                    // and remove them
                    Bar.Menus = Bar.Menus.Except(prunedEmptyTopLevelMenus.Values).ToArray();
                }               
            }            
                
            return true;
        }

        public override void Redo()
        {
            Do();
        }

        public override void Undo()
        {
            if(Parent == null || OperateOn == null)
                return;

            var children = Parent.Children.ToList<MenuItem>();
                
            children.Insert(_removedAtIdx, OperateOn);
            Parent.Children = children.ToArray();
            Bar?.SetNeedsDisplay();

            // if we removed any top level empty menus as a 
            // side effect of the removal then put them back
            if(prunedEmptyTopLevelMenus != null && Bar != null)
            {
                var l = Bar.Menus.ToList<MenuBarItem>();

                // for each index they used to be at
                foreach(var kvp in prunedEmptyTopLevelMenus.OrderBy(k=>k))
                {
                    // put them back
                    l.Insert(kvp.Key,kvp.Value);
                }

                Bar.Menus = l.ToArray();
            }
        }
    }
}