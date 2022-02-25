using System.Text;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Registers events and sets up keybindings such that views do not
/// function as live but instead support editing (resizing etc)
/// </summary>
public class DesignTimeEventsManager
{
    public void RegisterEvents(View view)
    {
        // all views can be focused so that they can be edited
        // or deleted etc
        view.CanFocus = true;

        view.KeyPress += (k)=>
        {
            if(k.KeyEvent.Key == Key.F4)
            {
                StringBuilder sb = new StringBuilder();
                
                foreach(var prop in view.GetType().GetProperties())
                {
                    sb.AppendLine($"{prop.Name}:{prop.GetValue(view)}");
                }

                MessageBox.Query(10,10,"Properties",sb.ToString(),"Ok");
                k.Handled = true;
            }
        };
    }
}

