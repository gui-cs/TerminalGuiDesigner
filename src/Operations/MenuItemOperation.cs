using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    public abstract class MenuItemOperation : Operation
    {
        /// <summary>
        /// The big long bar view that goes at the top
        /// of windows.
        /// </summary>
        public MenuBar? Bar { get; private set; }

        /// <summary>
        /// The collection that contains the menu item 
        /// being operated on.
        /// </summary>
        public MenuBarItem? Parent { get; private set; }

        protected readonly MenuItem? OperateOn;

        public MenuItemOperation(MenuItem operateOn)
        {
            // if taking a new line add an extra menu item
            // menuItem.Parent doesn't work for root menu items
            var parent = MenuTracker.Instance.GetParent(operateOn, out var bar);

            if (parent == null)
            {
                IsImpossible = true;
                return;
            }

            this.Bar = bar;
            this.Parent = parent;
            this.OperateOn = operateOn;
        }
    }
}