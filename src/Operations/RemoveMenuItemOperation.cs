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

        /// <summary>
        /// If as a result of removing this MenuItem the MenuBar ended
        /// up being completely blank and was therefore removed.  This
        /// field stores the View it was removed from (for undo purposes)
        /// </summary>
        private View? _barRemovedFrom;

        /// <summary>
        /// If the removed MenuItem was a submenu item and it was the last one then
        /// its parent will have been converted from a MenuBarItem (has children)
        /// to a regular MenuItem (does not have children).  This collection 
        /// stores all such replacements made during carrying out the command
        /// </summary>
        private Dictionary<MenuBarItem, MenuItem>? _convertedMenuBars;

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

            if (Bar != null)
            {
                _convertedMenuBars = MenuTracker.Instance.ConvertEmptyMenus();
            }

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

                // if we just removed the last menu header
                // leaving a completely blank menu bar
                if (Bar.Menus.Length == 0 && Bar.SuperView != null)
                {
                    // remove the bar completely
                    Bar.CloseMenu();
                    _barRemovedFrom = Bar.SuperView;
                    _barRemovedFrom.Remove(Bar);
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

            // if any MenuBarItem were converted to vanilla MenuItem
            // because we were removed from a submenu then convert
            // it back
            if(_convertedMenuBars != null)
            {
                foreach(var converted in _convertedMenuBars)
                {
                    var grandparent = MenuTracker.Instance.GetParent(converted.Value, out _);
                    if(grandparent != null)
                    {
                        var popIdx = Array.IndexOf(grandparent.Children,converted.Value);
                        var newParents = grandparent.Children.ToList<MenuItem>();
                        newParents.RemoveAt(popIdx);
                        newParents.Insert(popIdx,converted.Key);
                        
                        grandparent.Children = newParents.ToArray();
                    }
                }
            }

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

            if(Bar != null && _barRemovedFrom != null)
            {
                _barRemovedFrom.Add(Bar);
                
                // lets clear this incase the user some 
                // manages to undo this command multiple
                // times
                _barRemovedFrom = null;
            }
        }
    }
}