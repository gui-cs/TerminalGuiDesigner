using System.Collections.ObjectModel;
using System.Runtime.InteropServices.ComTypes;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Showcase
{
    internal class Program
    {
        private static Type[] views = new[]
        {
            typeof(NumericUpDown) 

        };
        static void Main(string[] args)
        {
            Application.Init();

            var w = new Window()
            {
                Title = "Showcase"
            };

            var lv = new ListView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            w.Add(lv);
            lv.SetSource(new ObservableCollection<Type>(views));
            

            lv.KeyDown += (_, e) =>
            {
                if (e.KeyCode == KeyCode.Enter)
                {
                    var v = (Toplevel)Activator.CreateInstance(views[lv.SelectedItem]);
                    e.Handled = true;
                    Application.Run(v);
                }
            };

            Application.Run(w);
            Application.Shutdown();
        }
    }
}
