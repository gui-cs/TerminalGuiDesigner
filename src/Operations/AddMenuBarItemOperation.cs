using System.Data;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Lets the user type alphanumeric and space keys to
/// create and rename new menu items just like in the
/// winforms designer
/// </summary>
public class AddMenuBarItemsByTypingManager
{
    public bool HandleKey(View focusedView,KeyEvent keystroke)
    {
        var bar = focusedView as MenuBar;

        if(bar == null)
            return false;

        GetSelected(bar,out var selected, out var selectedSub);

        // If typing into a menu bar
        var ch = (char)keystroke.KeyValue;
        
        if(char.IsLetterOrDigit(ch))
        {
            if(bar.Menus.Length == 0)
            {
                bar.Menus = new []{new MenuBarItem{
                    Title = ""+ch}
                    };
                
                bar.SetNeedsDisplay();
            }
        }

        return false;
    }

    private void GetSelected(MenuBar mb, out int selected, out int selectedSub)
    {
        var fselected = typeof(MenuBar).GetField("selected",BindingFlags.Instance|BindingFlags.NonPublic)?? throw new MissingFieldException("Expected MenuBar private field was missing");
        var fselectedSub = typeof(MenuBar).GetField("selectedSub",BindingFlags.Instance|BindingFlags.NonPublic)?? throw new MissingFieldException("Expected MenuBar private field was missing");

        selected = (int?)fselected.GetValue(mb) ?? throw new Exception("Expected int but was null");
        selectedSub = (int?)fselectedSub.GetValue(mb) ?? throw new Exception("Expected int but was null");
    }
}

public class AddMenuBarItemOperation : Operation
{
    public AddMenuBarItemOperation(MenuBar bar,string name)
    {
        
    }


    public override void Do()
    {
        throw new NotImplementedException();
    }

    public override void Redo()
    {
        throw new NotImplementedException();
    }

    public override void Undo()
    {
        throw new NotImplementedException();
    }
}