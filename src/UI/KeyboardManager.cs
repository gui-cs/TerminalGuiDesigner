using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.MenuOperations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.UI;

/// <summary>
/// Manager for acting on global key presses before they are passed to other
/// controls while <see cref="Editor"/> has an open <see cref="View"/>.
/// </summary>
public class KeyboardManager
{
    private readonly IApplication app;
    private readonly KeyMap keyMap;

    private SetPropertyOperation? CurrentOperation => OperationManager.Instance.PendingOperation as SetPropertyOperation;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardManager"/> class.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="keyMap">User configurable keybindings for class functionality.</param>
    public KeyboardManager(IApplication app, KeyMap keyMap)
    {
        this.app = app;
        this.keyMap = keyMap;
    }

    /// <summary>
    /// Evaluates <paramref name="keystroke"/> when <paramref name="focusedView"/> has
    /// focus and orders any <see cref="Operation"/> based on it or lets it pass through
    /// to the rest of the regular Terminal.Gui API layer.
    /// </summary>
    /// <param name="focusedView">The <see cref="View"/> that currently holds focus in <see cref="Editor"/>.</param>
    /// <param name="keystroke">The key that has been reported by <see cref="Application.KeyDown"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keystroke"/> should be suppressed.</returns>
    public bool HandleKey(View focusedView, Key keystroke)
    {
        var menuItem = MenuTracker.GetFocusedMenuItemIfAny(app);

        // if we are in a menu
        if (menuItem != null)
        {
            return this.HandleKeyPressInMenu(focusedView, menuItem, keystroke);
        }

        var d = focusedView.GetNearestDesign();

        // if we are no longer focused
        if (d == null)
        {
            OperationManager.Instance.FlushPending();
            return false;
        }

        // if we have changed focus, flush any pending operation
        if (OperationManager.Instance.PendingOperation != null)
        {
            var viewTextOp = this.CurrentOperation;
            if (viewTextOp == null || !viewTextOp.Designs.Contains(d))
            {
                OperationManager.Instance.FlushPending();
            }
        }

        if (keystroke.ToString( ) == this.keyMap.Rename)
        {
            var nameProp = d.GetDesignableProperties().OfType<NameProperty>().FirstOrDefault();
            if (nameProp != null)
            {
                EditDialog.SetPropertyToNewValue(app, d, nameProp, nameProp.GetValue());
                return true;
            }
        }

        if (!this.IsActionableKey(keystroke))
        {
            // we can't do anything with this keystroke
            return false;
        }

        // if we are not currently doing anything
        if (this.CurrentOperation == null)
        {
            // start a new operation
            this.StartOperation(d);
        }

        return this.ApplyKeystrokeToTextProperty(keystroke);
    }

    private bool HandleKeyPressInMenu(View focusedView, MenuItem menuItem, Key keystroke)
    {
        if (keystroke.ToString( ) == this.keyMap.Rename)
        {
            OperationManager.Instance.Do(
                    new RenameMenuItemOperation(this.app, menuItem));
            return true;
        }

        if (keystroke == Key.Enter)
        {
            OperationManager.Instance.Do(
                    new AddMenuItemOperation(this.app, menuItem));

            return true;
        }

        if (keystroke.ToString( ) == this.keyMap.SetShortcut)
        {
            var shortcutProp = typeof(MenuItem).GetProperty(nameof(MenuItem.Key))
                ?? throw new Exception("MenuItem.Key property not found");
            var op = new SetChildPropertyOperation(this.app, menuItem, shortcutProp);
            op.NewValue = Modals.GetShortcut(app);
            OperationManager.Instance.Do(op);
            return false;
        }

        if (keystroke.ToString( ) == this.keyMap.MoveRight)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemRightOperation(this.app, menuItem));

            ChangeKeyTo(keystroke, Key.CursorUp);
            return true;
        }

        if (keystroke.ToString( ) == this.keyMap.MoveLeft)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemLeftOperation(this.app, menuItem));

            ChangeKeyTo(keystroke, Key.CursorDown);
            return false;
        }

        if (keystroke.ToString( ) == this.keyMap.MoveUp)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemOperation(this.app, menuItem, true));
            ChangeKeyTo(keystroke, Key.CursorUp);
            return false;
        }

        if (keystroke.ToString( ) == this.keyMap.MoveDown)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemOperation(this.app, menuItem, false));
            ChangeKeyTo(keystroke, Key.CursorDown);
            return false;
        }

        if ((keystroke == Key.DeleteChar)
            ||
            (keystroke == Key.Backspace && string.IsNullOrWhiteSpace(menuItem.Title.ToString())))
        {
            // deleting the menu item using backspace to
            // remove all characters in the title or the Del key
            var remove = new RemoveMenuItemOperation(this.app, menuItem);
            if (OperationManager.Instance.Do(remove))
            {
                // if we are removing the last item
                if (remove.PrunedTopLevelMenu)
                {
                    // if we deleted the last menu item
                    /*
                    if (remove.Bar?.Menus.Length == 0)
                    {
                        remove.Bar.CloseMenu(false);
                        return true;
                    }*/

                    // convert keystroke to left,
                    // so we move to the next menu
                    ChangeKeyTo(keystroke, Key.CursorLeft);
                    return false;
                }

                // otherwise convert keystroke to up
                // so that focus now sits nicely on the
                // menu item above the deleted one
                ChangeKeyTo(keystroke, Key.CursorUp);
                return false;
            }
        }

        // When menu is being edited let user paste in text e.g. command names
        if (keystroke == keyMap.Paste)
        {
            if (Clipboard.TryGetClipboardData(out string text) && IsValidSimpleStringToPaste(text))
            {
                var titleProp = typeof(MenuItem).GetProperty(nameof(MenuItem.Title))
                    ?? throw new Exception("MenuItem.Title property not found");
                var pasteOp = new SetChildPropertyOperation(this.app, menuItem, titleProp);
                pasteOp.NewValue = (menuItem.Title?.ToString() ?? string.Empty) + text;
                OperationManager.Instance.Do(pasteOp);
                return true;
            }
        }

        // Allow typing but also Enter to create a new sub-item
        if ( !this.IsActionableKey( keystroke ) )
        {
            return false;
        }

        // TODO: This probably lets us edit the Editors own context menus lol

        // Get or create a pending title operation for this menu item
        var pendingTitleOp = OperationManager.Instance.PendingOperation as SetChildPropertyOperation;
        if (pendingTitleOp == null || !ReferenceEquals(pendingTitleOp.Target, menuItem))
        {
            OperationManager.Instance.FlushPending();
            var titleProp = typeof(MenuItem).GetProperty(nameof(MenuItem.Title))
                ?? throw new Exception("MenuItem.Title property not found");
            pendingTitleOp = new SetChildPropertyOperation(this.app, menuItem, titleProp);
            OperationManager.Instance.PendingOperation = pendingTitleOp;
        }

        if (this.ApplyKeystrokeToString(menuItem.Title?.ToString() ?? string.Empty, keystroke, out var newValue))
        {
            menuItem.Title = newValue;
            pendingTitleOp.NewValue = newValue;
            focusedView.SetNeedsDraw();
            return true;
        }

        return false;
    }

    private bool IsValidSimpleStringToPaste(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            return false;
        }

        if (result.Contains('\n') || result.Contains('\r'))
        {
            return false;
        }

        if (result.Length > 100)
        {
            return false;
        }

        return true;
    }

    private void ChangeKeyTo(Key keystroke, Key newKey)
    {
        Type t = typeof(Key);
        var p = t.GetProperty("KeyCode") ?? throw new Exception("Property somehow doesn't exist");
        p.SetValue(keystroke, newKey.KeyCode);
    }

    private void StartOperation(Design d)
    {
        // these can already handle editing themselves
        if (d.View is TextField || d.View is TextView)
        {
            return;
        }

        var textProp = d.GetDesignableProperty("Text");

        if (textProp != null)
        {
            OperationManager.Instance.PendingOperation = new SetPropertyOperation(this.app, d, textProp, d.View.Text, d.View.Text);
        }
    }

    private void FinishOperation()
    {
        OperationManager.Instance.FlushPending();
    }

    private bool ApplyKeystrokeToTextProperty(Key keystroke)
    {
        if (this.CurrentOperation == null || this.CurrentOperation.Designs.Count != 1)
        {
            return false;
        }

        var design = this.CurrentOperation.Designs.Single();

        var str = design.View.GetActualText();

        if (!this.ApplyKeystrokeToString(str, keystroke, out var newStr))
        {
            // not a keystroke we can act upon
            return false;
        }

        design.View.SetActualText(newStr);
        design.View.SetNeedsDraw();
        this.CurrentOperation!.NewValue = newStr;

        return true;
    }

    private bool ApplyKeystrokeToString(string? str, Key keystroke, out string newString)
    {
        newString = str;

        if (keystroke == Key.Backspace)
        {
            // no change
            if ( string.IsNullOrEmpty( str ) )
            {
                return false;
            }

            // chop off a letter
            newString = str.Length == 1 ? string.Empty : str.Substring(0, str.Length - 1);
            return true;
        }

        var ch = KeyToLetter(keystroke);



        newString += ch;

        return true;
    }

    private char KeyToLetter(Key keystroke)
    {
        if(keystroke == Key.Space)
        {
            return ' ';
        }

        var ch = (char)Key.ToRune(keystroke.KeyCode).Value;

        if(ch >= 'A' && ch <= 'Z' && !keystroke.IsShift)
        {
            return char.ToLower(ch);
        }       

        return ch;
    }

    private bool IsActionableKey(Key keystroke)
    {
        
        if (keystroke == Key.Backspace)
        {
            return true;
        }
        if(keystroke == Key.Delete ||  keystroke.KeyCode == KeyCode.ShiftMask)
        {
            return false;
        }
        // Don't let Ctrl+Q add a Q!
        if (keystroke.IsCtrl)
        {
            return false;
        }

        if (keystroke >= Key.A && keystroke <= Key.Z) {
            return true;
        }

        var punctuation = "\"\\/':;%^&*~`!@#.,? ()-+{}<>=_][|";

        var ch = KeyToLetter(keystroke);

        

        return punctuation.Contains(ch) || char.IsLetterOrDigit(ch);
    }

}