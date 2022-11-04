using System.Reflection;
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
            var selected = (int)GetNonNullPrivateFieldValue("selected", menuBar, typeof(MenuBar));

            if (selected < 0 || selected >= menuBar.Menus.Length)
            {
                return null;
            }

            return menuBar.Menus[selected];
        }

        private static object GetNonNullPrivateFieldValue(string fieldName, object item, Type type)
        {
            var selectedField = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception($"Expected private field {fieldName} was not present on {type.Name}");
            return selectedField.GetValue(item)
                ?? throw new Exception($"Private field {fieldName} was unexpectedly null on {type.Name}");
        }

        /// <summary>
        /// Returns the currently selected <paramref name="menu"/> in a dropdown menu that
        /// is currently expanded and being explored.
        /// </summary>
        /// <param name="menu">Must be the internal Terminal.Gui class "Menu"</param>
        /// <returns></returns>
        internal static MenuItem? GetSelectedSubMenuItemFromMenu(View menu)
        {
            var menuClass = menu.GetType();
            if (menuClass.Name != "Menu")
            {
                return null;
            }

            var barItems = (MenuBarItem)GetNonNullPrivateFieldValue("barItems", menu, menuClass);
            var current = (int)GetNonNullPrivateFieldValue("current", menu, menuClass);

            if (current < 0 || current >= barItems.Children.Length)
            {
                return null;
            }

            return barItems.Children[current];
        }
    }
}
