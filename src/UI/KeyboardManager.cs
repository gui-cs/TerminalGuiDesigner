using Terminal.Gui;
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
    private readonly KeyMap keyMap;
    private SetPropertyOperation? currentOperation;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardManager"/> class.
    /// </summary>
    /// <param name="keyMap">User configurable keybindings for class functionality.</param>
    public KeyboardManager(KeyMap keyMap)
    {
        this.keyMap = keyMap;
    }

    /// <summary>
    /// Evaluates <paramref name="keystroke"/> when <paramref name="focusedView"/> has
    /// focus and orders any <see cref="Operation"/> based on it or lets it pass through
    /// to the rest of the regular Terminal.Gui API layer.
    /// </summary>
    /// <param name="focusedView">The <see cref="View"/> that currently holds focus in <see cref="Editor"/>.</param>
    /// <param name="keystroke">The key that has been reported by <see cref="Application.RootKeyEvent"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="keystroke"/> should be suppressed.</returns>
    public bool HandleKey(View focusedView, KeyEvent keystroke)
    {
        var menuItem = MenuTracker.Instance.CurrentlyOpenMenuItem;

        // if we are in a menu
        if (menuItem != null)
        {
            return this.HandleKeyPressInMenu(focusedView, menuItem, keystroke);
        }

        var d = focusedView.GetNearestDesign();

        // if we are no longer focused
        if (d == null)
        {
            // if there is another operation underway
            if (this.currentOperation != null)
            {
                this.FinishOperation();
            }

            // do not swallow this keystroke
            return false;
        }

        // if we have changed focus
        if (this.currentOperation != null && !this.currentOperation.Designs.Contains(d))
        {
            this.FinishOperation();
        }

        if (keystroke.Key == this.keyMap.Rename)
        {
            var nameProp = d.GetDesignableProperties().OfType<NameProperty>().FirstOrDefault();
            if (nameProp != null)
            {
                EditDialog.SetPropertyToNewValue(d, nameProp, nameProp.GetValue());
                return true;
            }
        }

        if (!this.IsActionableKey(keystroke))
        {
            // we can't do anything with this keystroke
            return false;
        }

        // if we are not currently doing anything
        if (this.currentOperation == null)
        {
            // start a new operation
            this.StartOperation(d);
        }

        this.ApplyKeystrokeToTextProperty(keystroke);

        return false;
    }

    private bool HandleKeyPressInMenu(View focusedView, MenuItem menuItem, KeyEvent keystroke)
    {
        if (keystroke.Key == this.keyMap.Rename)
        {
            OperationManager.Instance.Do(
                    new RenameMenuItemOperation(menuItem));
            return true;
        }

        if (keystroke.Key == Key.Enter)
        {
            OperationManager.Instance.Do(
                    new AddMenuItemOperation(menuItem));

            keystroke.Key = Key.CursorDown;
            return false;
        }

        if (keystroke.Key == this.keyMap.SetShortcut)
        {
            menuItem.Shortcut = Modals.GetShortcut();

            focusedView.SetNeedsDisplay();
            return false;
        }

        if (keystroke.Key == this.keyMap.MoveRight)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemRightOperation(menuItem));

            keystroke.Key = Key.CursorUp;
            return true;
        }

        if (keystroke.Key == this.keyMap.MoveLeft)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemLeftOperation(menuItem));

            keystroke.Key = Key.CursorDown;
            return false;
        }

        if (keystroke.Key == this.keyMap.MoveUp)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemOperation(menuItem, true));
            keystroke.Key = Key.CursorUp;
            return false;
        }

        if (keystroke.Key == this.keyMap.MoveDown)
        {
            OperationManager.Instance.Do(
                new MoveMenuItemOperation(menuItem, false));
            keystroke.Key = Key.CursorDown;
            return false;
        }

        if ((keystroke.Key == Key.DeleteChar)
            ||
            (keystroke.Key == Key.Backspace && string.IsNullOrWhiteSpace(menuItem.Title.ToString())))
        {
            // deleting the menu item using backspace to
            // remove all characters in the title or the Del key
            var remove = new RemoveMenuItemOperation(menuItem);
            if (OperationManager.Instance.Do(remove))
            {
                // if we are removing the last item
                if (remove.PrunedTopLevelMenu)
                {
                    // if we deleted the last menu item
                    if (remove.Bar?.Menus.Length == 0)
                    {
                        remove.Bar.CloseMenu();
                        return true;
                    }

                    // convert keystroke to left,
                    // so we move to the next menu
                    keystroke.Key = Key.CursorLeft;
                    return false;
                }

                // otherwise convert keystroke to up
                // so that focus now sits nicely on the
                // menu item above the deleted one
                keystroke.Key = Key.CursorUp;
                return false;
            }
        }

        // Allow typing but also Enter to create a new subitem
        if (!this.IsActionableKey(keystroke))
        {
            return false;
        }

        // TODO: This probably lets us edit the Editors own context menus lol

        // TODO once https://github.com/migueldeicaza/gui.cs/pull/1689 is merged and published
        // we can integrate this into the Design undo/redo systems
        if (this.ApplyKeystrokeToString(menuItem.Title.ToString() ?? string.Empty, keystroke, out var newValue))
        {
            // convert to a separator by typing three hyphens
            if (newValue.Equals("---"))
            {
                if (OperationManager.Instance.Do(
                        new ConvertMenuItemToSeperatorOperation(menuItem)))
                {
                    return true;
                }
            }
            else
            {
                // changing the title
                menuItem.Title = newValue;
            }

            focusedView.SetNeedsDisplay();

            return true;
        }

        return false;
    }

    private void StartOperation(Design d)
    {
        // these can already handle editing themselves
        if (d.View is DateField || d.View is TextField || d.View is TextView)
        {
            return;
        }

        var textProp = d.GetDesignableProperty("Text");

        if (textProp != null)
        {
            this.currentOperation = new SetPropertyOperation(d, textProp, d.View.Text, d.View.Text);
        }
    }

    private void FinishOperation()
    {
        if (this.currentOperation == null)
        {
            return;
        }

        // finish it and clear it
        OperationManager.Instance.Do(this.currentOperation);
        this.currentOperation = null;
    }

    private bool ApplyKeystrokeToTextProperty(KeyEvent keystroke)
    {
        if (this.currentOperation == null || this.currentOperation.Designs.Count != 1)
        {
            return false;
        }

        var design = this.currentOperation.Designs.Single();

        var str = design.View.GetActualText();

        if (!this.ApplyKeystrokeToString(str, keystroke, out var newStr))
        {
            // not a keystroke we can act upon
            return false;
        }

        design.View.SetActualText(newStr);
        design.View.SetNeedsDisplay();
        this.currentOperation.NewValue = newStr;

        return true;
    }

    private bool ApplyKeystrokeToString(string? str, KeyEvent keystroke, out string newString)
    {
        newString = str;

        if (keystroke.Key == Key.Backspace)
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
        else
        {
            var ch = (char)keystroke.KeyValue;
            newString += ch;

            return true;
        }
    }

    private bool IsActionableKey(KeyEvent keystroke)
    {
        if (keystroke.Key == Key.Backspace)
        {
            return true;
        }

        // Don't let Ctrl+Q add a Q!
        if (keystroke.Key.HasFlag(Key.CtrlMask))
        {
            return false;
        }

        var punctuation = "\"\\/':;%^&*~`!@#.,? ()-+{}<>=_][|";

        var ch = (char)keystroke.KeyValue;

        return punctuation.Contains(ch) || char.IsLetterOrDigit(ch);
    }
}