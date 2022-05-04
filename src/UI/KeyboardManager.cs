using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI
{
    public class KeyboardManager
    {

        SetPropertyOperation? _currentOperation;

        public bool HandleKey(View focusedView,KeyEvent keystroke)
        {
            var menuItem = MenuTracker.Instance.CurrentlyOpenMenuItem;

            // if we are in a menu
            if (menuItem != null)
            {
                // TODO: This probably lets us edit the Editors own context menus lol

                // TODO once https://github.com/migueldeicaza/gui.cs/pull/1689 is merged and published
                // we can integrate this into the Design undo/redo systems


                // if taking a new line add an extra menu item
                if (keystroke.Key == Key.Enter)
                {
                    // TODO: this seems to always be null. need to investigate why
                    var parent = menuItem.Parent as MenuBarItem;
                    if(parent != null)
                    {
                        var children = parent.Children.ToList<MenuItem>();
                        children.Add(new MenuItem());
                        parent.Children = children.ToArray();
                        focusedView.SetNeedsDisplay();
                        return true;
                    }
                }
                else
                if (ApplyKeystrokeToString(menuItem.Title.ToString() ?? "", keystroke, out var newValue))
                {
                    menuItem.Title = newValue;
                    focusedView.SetNeedsDisplay();
                    return true;
                }
            }

            var d = focusedView.GetNearestDesign();

            // if we are no longer focused 
            if(d == null)
            {
                // if there is another operation underway
                if(_currentOperation != null)
                {
                    FinishOperation();
                }

                // do not swallow this keystroke
                return false;
            }

            // if we have changed focus
            if(_currentOperation != null && d != _currentOperation.Design)
            {
                FinishOperation();
            }


            if(!IsActionableKey(keystroke))
            {
                // we can't do anything with this keystroke
                return false;
            }

            // if we are not currently doing anything
            if(_currentOperation == null)
            {
                // start a new operation
                StartOperation(d);
            }

            ApplyKeystrokeToTextProperty(keystroke);

            return false;
        }

        private void StartOperation(Design d)
        {
            // these can already handle editing themselves
            if(d.View is DateField || d.View is TextField || d.View is TextView)
                return;

            _currentOperation = new SetPropertyOperation(d,d.GetDesignableProperty("Text"),d.View.Text,d.View.Text);
        }

        private void FinishOperation()
        {
            if(_currentOperation == null)
            {
                return;
            }

            // finish it and clear it
            OperationManager.Instance.Do(_currentOperation);
            _currentOperation = null;
        }

        private bool ApplyKeystrokeToTextProperty(KeyEvent keystroke)
        {
            if(_currentOperation == null){
                return false;
            }

            var str = _currentOperation.Design.View.GetActualText();
            
            if (!ApplyKeystrokeToString(str, keystroke, out var newStr)) // not a keystroke we can act upon
                return false;

            _currentOperation.Design.View.SetActualText(newStr);
            _currentOperation.Design.View.SetNeedsDisplay();
            _currentOperation.NewValue = str;
            
            return true;

        }

        private bool ApplyKeystrokeToString(string str, KeyEvent keystroke, out string newString)
        {
            newString = str;

            if (keystroke.Key == Key.Backspace)
            {
                // no change
                if (str == null || str.Length == 0)
                    return false;

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
            if(keystroke.Key == Key.Backspace)
            {
                return true;
            }

            var punctuation = "\"'a:;%^&*~`bc!@#.,? ";

            var ch = (char)keystroke.KeyValue;

            return punctuation.Contains(ch) || char.IsLetterOrDigit(ch);
        }
    }
}