using Terminal.Gui.App;

namespace Showcase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using(var app = Application.Create())
            {
                app.Run<Menus>();
            }
        }
    }
}
