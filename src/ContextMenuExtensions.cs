using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace TerminalGuiDesigner;

internal static class ContextMenuExtensions
{
    public static void SetMenuItems(this ContextMenu menu, MenuBarItem? value)
    {
        var prop = typeof(ContextMenu).GetProperty(nameof(ContextMenu.MenuItems))
            ?? throw new Exception($"Expected property {nameof(ContextMenu.MenuItems)} did not exist on {nameof(ContextMenu)}");
        prop.SetValue(menu,value);
    }
}

