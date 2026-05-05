using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Showcase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Type[] types = [
                typeof(Menus),
                typeof(Buttons),
                typeof(Checkboxes),
                typeof(DateTimes),
                typeof(ColorPickers),
                typeof(Ranges),
                typeof(Lists),
                typeof(Tables)
                ];

            using (var app = Application.Create())
            {
                var tv = new TableView()
                {
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                tv.Table = new EnumerableTableSource<Type>(types,
                    new Dictionary<string, Func<Type, object>> { { "Scenario (Enter to open, Esc to close/exit)", (t) => t.Name + $" ({t.BaseType?.Name})"} }
                    );

                tv.KeyBindings.ReplaceCommands(Key.Enter,Command.Accept);

                tv.Accepted += (s, e) =>
                {
                    var row = tv.Value.Cursor.Y;
                    if (row >= 0 && row < types.Length)
                    {
                        var toCreate = types[row];
                        View view = (View)Activator.CreateInstance(toCreate);
                        
                        if(view is Runnable r)
                        {
                            app.Run(r);
                        }
                        else
                        {
                            var newRunnable = new Runnable();
                            newRunnable.Add(view);
                            app.Run(newRunnable);
                        }

                        e.Handled = true;
                    }
                };

                var r = new Runnable();
                r.Add(tv);

                app.Init();
                app.Run(r);
                app.Dispose();
            }
        }
    }
}
