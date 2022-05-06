using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    public abstract class MenuItemOperation : Operation
    {
        protected readonly View FocusedView;
        protected readonly MenuBar? Bar;
        protected readonly MenuBarItem Parent;
        protected readonly MenuItem OperateOn;

        public MenuItemOperation(View focusedView, MenuBar? bar, MenuBarItem parent, MenuItem operateOn)
        {
            this.FocusedView = focusedView;
            this.Bar = bar;
            this.Parent = parent;
            this.OperateOn = operateOn;
        }

    }
}