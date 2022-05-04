using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace TerminalGuiDesigner
{
    public static class MenuBarExtensions
    {
        /// <summary>
        /// Gets the top level selected <see cref="MenuBarItem"/> in the <paramref name="menuBar"/>
        /// or null if it is not open/no selection is set.  Note that a submenu may still be 
        /// present but only the root will be returned
        /// </summary>
        /// <param name="menuBar"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static MenuBarItem? GetSelectedMenuItem(this MenuBar menuBar)
        {
            var field = "selected";
            var selectedField = typeof(MenuBar).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception($"Expected private field {field} was not present on {nameof(MenuBar)}");

            var selected = (int)(selectedField.GetValue(menuBar)
                ?? throw new Exception($"Private field {field} was unexpectedly null on {nameof(MenuBar)}"));

            if (selected < 0 || selected >= menuBar.Menus.Length)
                return null;

            return menuBar.Menus[selected];
        }

    }
}
