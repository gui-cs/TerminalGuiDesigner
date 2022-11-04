using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.UI
{
    public class KeyboardManager
    {
        private SetPropertyOperation? _currentOperation;
        private KeyMap _keyMap;

        public KeyboardManager(KeyMap keyMap)
        {
            _keyMap = keyMap;
        }

        public bool HandleKey(View focusedView, KeyEvent keystroke)
        {
            var menuItem = MenuTracker.Instance.CurrentlyOpenMenuItem;

            // if we are in a menu
            if (menuItem != null)
            {
                return HandleKeyPressInMenu(focusedView, menuItem, keystroke);
            }

            var d = focusedView.GetNearestDesign();

            // if we are no longer focused 
            if (d == null)
            {
                // if there is another operation underway
                if (_currentOperation != null)
                {
                    FinishOperation();
                }

                // do not swallow this keystroke
                return false;
            }

            // if we have changed focus
            if (_currentOperation != null && d != _currentOperation.Design)
            {
                FinishOperation();
            }

            if (keystroke.Key == _keyMap.Rename)
            {
                var nameProp = d.GetDesignableProperties().OfType<NameProperty>().FirstOrDefault();
                if (nameProp != null)
                {
                    EditDialog.SetPropertyToNewValue(d, nameProp, nameProp.GetValue());
                    return true;
                }
            }

            if (!IsActionableKey(keystroke))
            {
                // we can't do anything with this keystroke
                return false;
            }

            // if we are not currently doing anything
            if (_currentOperation == null)
            {
                // start a new operation
                StartOperation(d);
            }

            ApplyKeystrokeToTextProperty(keystroke);

            return false;
        }

        private bool HandleKeyPressInMenu(View focusedView, MenuItem menuItem, KeyEvent keystroke)
        {
            if (keystroke.Key == _keyMap.Rename)
            {
                OperationManager.Instance.Do(
                        new RenameMenuItemOperation(menuItem)
                    );
                return true;
            }

            if (keystroke.Key == Key.Enter)
            {
                OperationManager.Instance.Do(
                        new AddMenuItemOperation(menuItem)
                    );

                keystroke.Key = Key.CursorDown;
                return false;
            }

            if (keystroke.Key == _keyMap.SetShortcut)
            {
                Key key = 0;

                var dlg = new LoadingDialog("Press Shortcut or Del");
                dlg.KeyPress += (s) =>
                {
                    key = s.KeyEvent.Key;
                    Application.RequestStop();
                };
                Application.Run(dlg);

                menuItem.Shortcut = key == Key.DeleteChar ? 0 : key;

                focusedView.SetNeedsDisplay();
                return false;
            }

            if (keystroke.Key == _keyMap.MoveRight)
            {
                OperationManager.Instance.Do(
                    new MoveMenuItemRightOperation(menuItem)
                );

                keystroke.Key = Key.CursorUp;
                return true;
            }

            if (keystroke.Key == _keyMap.MoveLeft)
            {
                OperationManager.Instance.Do(
                    new MoveMenuItemLeftOperation(menuItem)
                );

                keystroke.Key = Key.CursorDown;
                return false;
            }

            if (keystroke.Key == _keyMap.MoveUp)
            {
                OperationManager.Instance.Do(
                    new MoveMenuItemOperation(menuItem, true)
                );
                keystroke.Key = Key.CursorUp;
                return false;
            }

            if (keystroke.Key == _keyMap.MoveDown)
            {
                OperationManager.Instance.Do(
                    new MoveMenuItemOperation(menuItem, false)
                );
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

                        // convert keystroke to left
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
            if (!IsActionableKey(keystroke))
            {
                return false;
            }

            // TODO: This probably lets us edit the Editors own context menus lol

            // TODO once https://github.com/migueldeicaza/gui.cs/pull/1689 is merged and published
            // we can integrate this into the Design undo/redo systems
            if (ApplyKeystrokeToString(menuItem.Title.ToString() ?? "", keystroke, out var newValue))
            {
                // convert to a separator by typing three hyphens
                if (newValue.Equals("---"))
                {
                    if (OperationManager.Instance.Do(
                            new ConvertMenuItemToSeperatorOperation(menuItem)
                        ))
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
                _currentOperation = new SetPropertyOperation(d, textProp, d.View.Text, d.View.Text);
            }
        }

        private void FinishOperation()
        {
            if (_currentOperation == null)
            {
                return;
            }

            // finish it and clear it
            OperationManager.Instance.Do(_currentOperation);
            _currentOperation = null;
        }

        private bool ApplyKeystrokeToTextProperty(KeyEvent keystroke)
        {
            if (_currentOperation == null)
            {
                return false;
            }

            var str = _currentOperation.Design.View.GetActualText();

            if (!ApplyKeystrokeToString(str, keystroke, out var newStr)) // not a keystroke we can act upon
            {
                return false;
            }

            _currentOperation.Design.View.SetActualText(newStr);
            _currentOperation.Design.View.SetNeedsDisplay();
            _currentOperation.NewValue = newStr;

            return true;
        }

        private bool ApplyKeystrokeToString(string str, KeyEvent keystroke, out string newString)
        {
            newString = str;

            if (keystroke.Key == Key.Backspace)
            {
                // no change
                if (str == null || str.Length == 0)
                {
                    return false;
                }

                // chop off a letter
                newString = str.Length == 1 ? "" : str.Substring(0, str.Length - 1);
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
}