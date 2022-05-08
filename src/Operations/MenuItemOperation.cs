using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    public abstract class MenuItemOperation : Operation
    {
        protected readonly MenuBar? Bar;
        protected readonly MenuBarItem? Parent;
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