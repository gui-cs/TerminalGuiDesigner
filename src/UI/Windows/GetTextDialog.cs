using Terminal.Gui;

namespace TerminalGuiDesigner.UI.Windows;

class GetTextDialog
{
    private readonly DialogArgs _args;
    private readonly string? _initialValue;
    private readonly Window win;
    public string? ResultText;
    private readonly TextView textField;
    private bool okClicked = false;

    public GetTextDialog(DialogArgs args, string? initialValue)
    {
        _args = args;
        _initialValue = initialValue;

        win = new Window(_args.WindowTitle)
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
            IsDefault = !_args.MultiLine
        };
        btnOk.Clicked += () =>
        {
            Accept();
        };

        var btnCancel = new Button("Cancel")
        {
            X = Pos.Right(btnOk),
            Y = Pos.Bottom(textField),
            IsDefault = false
        };
        btnCancel.Clicked += () =>
        {
            okClicked = false;
            Application.RequestStop();
        };

        var btnClear = new Button("Clear")
        {
            X = Pos.Right(btnCancel),
            Y = Pos.Bottom(textField)
        };
        btnClear.Clicked += () =>
        {
            textField.Text = "";
        };

        win.Add(btnOk);
        win.Add(btnCancel);
        win.Add(btnClear);
    }

    public bool ShowDialog()
    {
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
        if (obj.KeyEvent.Key == Key.Enter && !_args.MultiLine)
        {
            Accept();
            obj.Handled = true;
        }
    }
}
