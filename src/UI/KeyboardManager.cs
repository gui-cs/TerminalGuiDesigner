using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.UI
{
    public class KeyboardManager
    {

        SetPropertyOperation? _currentOperation;

        public bool HandleKey(View focusedView,KeyEvent keystroke)
        {
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
                _currentOperation = new SetPropertyOperation(d,d.GetDesignableProperty("Text"),d.View.Text,d.View.Text);
            }

            ApplyKeystrokeToTextProperty(keystroke);

            return false;
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

            if(keystroke.Key == Key.Backspace)
            {
                var str = _currentOperation.Design.View.GetActualText(); 

                // no change
                if(str == null || str.Length == 0)
                    return false;

                // chop off a letter
                str = str.Length == 1 ? "" : str.Substring(0,str.Length -1);

                _currentOperation.Design.View.SetActualText(str);
                _currentOperation.Design.View.SetNeedsDisplay();
                _currentOperation.NewValue = str;
                
                // we acted upon the backspace so consume it
                return true;
            }
            else
            {
                var ch = (char)keystroke.KeyValue;

                var str = _currentOperation.Design.View.GetActualText(); 
                str += ch;

                _currentOperation.Design.View.SetActualText(str);
                _currentOperation.Design.View.SetNeedsDisplay();
                _currentOperation.NewValue = str;

                return true;
            }
        }

        private bool IsActionableKey(KeyEvent keystroke)
        {
            if(keystroke.Key == Key.Backspace)
            {
                return true;
            }

            var ch = (char)keystroke.KeyValue;

            return char.IsLetterOrDigit(ch) || ch == ' ';
        }
    }
}