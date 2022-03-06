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

            instance.SetActualText("Heya");

            instance.Width = Math.Max(instance.Bounds.Width, 4);
            
            return instance;
        }
    }
}