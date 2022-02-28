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

            if(instance is Button b)
            {
                // See https://github.com/migueldeicaza/gui.cs/issues/1619
                b.Text = "Heya";
            }
            else
            {
                instance.Text = "Heya";
            }
            
            return instance;
        }
    }
}