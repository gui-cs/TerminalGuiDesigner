using Terminal.Gui;

namespace TerminalGuiDesigner;

class GetTextDialog
{
    private readonly DialogArgs _args;
    private readonly string _initialValue;

    public string ResultText;

    public GetTextDialog(DialogArgs args, string initialValue)
    {
        _args = args;
        _initialValue = initialValue;
    }
    public bool ShowDialog()
    {
        bool okClicked = false;

        var win = new Window(_args.WindowTitle)
        {
            X = 0,
            Y = 0,

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(1),
            Height = Dim.Fill(1),
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

        var textField = new TextView()
        {
            X = 1,
            Y = Pos.Bottom(entryLabel),
            Height = Dim.Fill(2),
            Width = Dim.Fill(2),
            Text = _initialValue ?? "",
            AllowsTab = false
        };

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
            okClicked = true;
            ResultText = textField.Text.ToString();
            Application.RequestStop();
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
}
