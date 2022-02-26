using Terminal.Gui;

namespace TerminalGuiDesigner
{
    internal class ViewFactory
    {
        public ViewFactory()
        {
        }

        public View Create(Type t)
        {
            var instance = (View)Activator.CreateInstance(t);
            instance.Text = "Heya";
            return instance;
        }
    }
}