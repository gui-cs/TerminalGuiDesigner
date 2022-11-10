using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    public class RemoveMenuItemOperation : MenuItemOperation
    {
        private int removedAtIdx;

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
        public bool PrunedTopLevelMenu => this.prunedEmptyTopLevelMenus != null && this.prunedEmptyTopLevelMenus.Any();

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

        public RemoveMenuItemOperation(MenuItem toRemove)
            : base(toRemove)
        {
        }

        protected override bool DoImpl()
        {
            if (this.Parent == null || this.OperateOn == null)
            {
                return false;
            }

            var children = this.Parent.Children.ToList<MenuItem>();

            this.removedAtIdx = Math.Max(0, children.IndexOf(this.OperateOn));

            children.Remove(this.OperateOn);
            this.Parent.Children = children.ToArray();
            this.Bar?.SetNeedsDisplay();

            if (this.Bar != null)
            {
                this._convertedMenuBars = MenuTracker.Instance.ConvertEmptyMenus();
            }

            // if a top level menu now has no children
            if (this.Bar != null)
            {
                var empty = this.Bar.Menus.Where(bi => bi.Children.Length == 0).ToArray();
                if (empty.Any())
                {
                    // remember where they were
                    this.prunedEmptyTopLevelMenus = empty.ToDictionary(e => Array.IndexOf(this.Bar.Menus, e), v => v);
                    // and remove them
                    this.Bar.Menus = this.Bar.Menus.Except(this.prunedEmptyTopLevelMenus.Values).ToArray();
                }

                // if we just removed the last menu header
                // leaving a completely blank menu bar
                if (this.Bar.Menus.Length == 0 && this.Bar.SuperView != null)
                {
                    // remove the bar completely
                    this.Bar.CloseMenu();
                    this._barRemovedFrom = this.Bar.SuperView;
                    this._barRemovedFrom.Remove(this.Bar);
                }
            }

            return true;
        }

        public override void Redo()
        {
            this.Do();
        }

        public override void Undo()
        {
            if (this.Parent == null || this.OperateOn == null)
            {
                return;
            }

            var children = this.Parent.Children.ToList<MenuItem>();

            children.Insert(this.removedAtIdx, this.OperateOn);
            this.Parent.Children = children.ToArray();
            this.Bar?.SetNeedsDisplay();

            // if any MenuBarItem were converted to vanilla MenuItem
            // because we were removed from a submenu then convert
            // it back
            if (this._convertedMenuBars != null)
            {
                foreach (var converted in this._convertedMenuBars)
                {
                    var grandparent = MenuTracker.Instance.GetParent(converted.Value, out _);
                    if (grandparent != null)
                    {
                        var popIdx = Array.IndexOf(grandparent.Children, converted.Value);
                        var newParents = grandparent.Children.ToList<MenuItem>();
                        newParents.RemoveAt(popIdx);
                        newParents.Insert(popIdx, converted.Key);

                        grandparent.Children = newParents.ToArray();
                    }
                }
            }

            // if we removed any top level empty menus as a
            // side effect of the removal then put them back
            if (this.prunedEmptyTopLevelMenus != null && this.Bar != null)
            {
                var l = this.Bar.Menus.ToList<MenuBarItem>();

                // for each index they used to be at
                foreach (var kvp in this.prunedEmptyTopLevelMenus.OrderBy(k => k))
                {
                    // put them back
                    l.Insert(kvp.Key, kvp.Value);
                }

                this.Bar.Menus = l.ToArray();
            }

            if (this.Bar != null && this._barRemovedFrom != null)
            {
                this._barRemovedFrom.Add(this.Bar);

                // lets clear this incase the user some
                // manages to undo this command multiple
                // times
                this._barRemovedFrom = null;
            }
        }
    }
}