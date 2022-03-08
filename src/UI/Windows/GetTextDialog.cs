using Terminal.Gui;

namespace TerminalGuiDesigner.UI.Windows;

class GetTextDialog
{
    private readonly DialogArgs _args;
    private readonly string _initialValue;

    public string ResultText;
    private TextView textField;
    private bool okClicked = false;

    public GetTextDialog(DialogArgs args, string initialValue)
    {
        _args = args;
        _initialValue = initialValue;
    }
    public bool ShowDialog()
    {

        var win = new Window(_args.WindowTitle)
        {
            X = 0,
            Y = 0,
            Modal = true,
        };

        var description = new Label
        {
            Text = _args.TaskDescription ?? "",
            Y = 0,
        };

        win.Add(description);

        var entryLabel = new Label
        {
            Text = _args.EntryLabel ?? "",
            Y = Pos.Bottom(description),
        };

        win.Add(entryLabel);

        textField = new TextView()
        {
            X = 1,
            Y = Pos.Bottom(entryLabel),
            Height = Dim.Fill(2),
            Width = Dim.Fill(2),
            Text = _initialValue ?? "",
            AllowsTab = false,
        };
        textField.KeyPress += TextField_KeyPress;

        // make it easier for user to replace this text with something else
        // by directly selecting it all so next keypress replaces text
        textField.SelectAll();

        win.Add(textField);

        var btnOk = new Button("Ok", true)
        {
            X = 0,
            Y = Pos.Bottom(textField),
            Width = 10,
            Height = 1,
            IsDefault = true
        };
        btnOk.Clicked += () =>
        {
            Accept();
        };

        var btnCancel = new Button("Cancel", true)
        {
            X = Pos.Right(btnOk),
            Y = Pos.Bottom(textField),
            Width = 13,
            Height = 1
        };
        btnCancel.Clicked += () =>
        {
            okClicked = false;
            Application.RequestStop();
        };

        var btnClear = new Button("Clear", true)
        {
            X = Pos.Right(btnCancel),
            Y = Pos.Bottom(textField),
            Width = 13,
            Height = 1,
        };
        btnClear.Clicked += () =>
        {
            textField.Text = "";
        };

        win.Add(btnOk);
        win.Add(btnCancel);
        win.Add(btnClear);

        Application.Run(win);

        return okClicked;
    }

    private void Accept()
    {
        okClicked = true;
        ResultText = textField.Text.ToString();
        Application.RequestStop();
    }

    private void TextField_KeyPress(View.KeyEventEventArgs obj)
    {
        if (obj.KeyEvent.Key == Key.Enter)
        {
            Accept();
            obj.Handled = true;
        }
    }
}
