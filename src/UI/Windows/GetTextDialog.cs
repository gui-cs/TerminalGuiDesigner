using Terminal.Gui;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Popup prompting user to enter some text.
/// </summary>
internal class GetTextDialog
{
    private readonly DialogArgs args;
    private readonly string? initialValue;
    private readonly Window win;
    private readonly TextView textField;
    private bool okClicked = false;

    public GetTextDialog(DialogArgs args, string? initialValue)
    {
        this.args = args;
        this.initialValue = initialValue;

        this.win = new Window(this.args.WindowTitle)
        {
            X = 0,
            Y = 0,
            Modal = true,
        };

        var description = new Label
        {
            Text = this.args.TaskDescription ?? string.Empty,
            Y = 0,
        };

        this.win.Add(description);

        var entryLabel = new Label
        {
            Text = this.args.EntryLabel ?? string.Empty,
            Y = Pos.Bottom(description),
        };

        this.win.Add(entryLabel);

        this.textField = new TextView()
        {
            X = 1,
            Y = Pos.Bottom(entryLabel),
            Height = Dim.Fill(2),
            Width = Dim.Fill(2),
            Text = this.initialValue ?? string.Empty,
            AllowsTab = false,
        };
        this.textField.KeyPress += this.TextField_KeyPress;

        // make it easier for user to replace this text with something else
        // by directly selecting it all so next keypress replaces text
        this.textField.SelectAll();

        this.win.Add(this.textField);

        var btnOk = new Button("Ok", true)
        {
            X = 0,
            Y = Pos.Bottom(this.textField),
            IsDefault = !this.args.MultiLine,
        };
        btnOk.Clicked += (s, e) =>
        {
            this.Accept();
        };

        var btnCancel = new Button("Cancel")
        {
            X = Pos.Right(btnOk),
            Y = Pos.Bottom(this.textField),
            IsDefault = false,
        };
        btnCancel.Clicked += (s, e) =>
        {
            this.okClicked = false;
            Application.RequestStop();
        };

        var btnClear = new Button("Clear")
        {
            X = Pos.Right(btnCancel),
            Y = Pos.Bottom(this.textField),
        };
        btnClear.Clicked += (s, e) =>
        {
            this.textField.Text = string.Empty;
        };

        this.win.Add(btnOk);
        this.win.Add(btnCancel);
        this.win.Add(btnClear);
    }

    public string? ResultText { get; set; }

    public bool ShowDialog()
    {
        Application.Run(this.win);

        return this.okClicked;
    }

    private void Accept()
    {
        this.okClicked = true;
        this.ResultText = this.textField.Text.ToString();
        Application.RequestStop();
    }

    private void TextField_KeyPress(object sender, KeyEventEventArgs obj)
    {
        if (obj.KeyEvent.Key == Key.Enter && !this.args.MultiLine)
        {
            this.Accept();
            obj.Handled = true;
        }
    }
}
