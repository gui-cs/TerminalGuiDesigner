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
    private readonly TextView textView;
    private bool okClicked = false;
    private static CheckState lastKnownEnableNewlines = CheckState.UnChecked;
    private bool? multiLineChecked;

    public GetTextDialog(DialogArgs args, string? initialValue)
    {
        this.args = args;
        this.initialValue = initialValue;

        this.win = new Window()
        {
            Title = this.args.WindowTitle,
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

        this.textView = new TextView()
        {
            X = 1,
            Y = Pos.Bottom(entryLabel),
            Height = Dim.Fill(2),
            Width = Dim.Fill(2),
            Text = this.initialValue ?? string.Empty,
            AllowsTab = false,
        };
        this.textView.KeyDown += this.TextViewKeyPress;

        // make it easier for user to replace this text with something else
        // by directly selecting it all so next keypress replaces text
        this.textView.SelectAll();

        this.win.Add(this.textView);

        if (args.MultiLine)
        {
            SetupMultiLineForced();
        }
        else
        if (args.ToggleableMultiLine)
        {
            SetupMultiLineOptional();
        }

        var btnOk = new Button()
        {
            Text = "Ok",
            X = 0,
            Y = Pos.Bottom(this.textView),
            IsDefault = !this.args.MultiLine,
        };
        btnOk.Accepting += (s, e) =>
        {
            e.Cancel = true;
            this.Accept();
        };

        var btnCancel = new Button()
        {
            Text = "Cancel",
            X = Pos.Right(btnOk),
            Y = Pos.Bottom(this.textView),
            IsDefault = false,
        };
        btnCancel.Accepting += (s, e) =>
        {
            e.Cancel = true;
            this.okClicked = false;
            Application.RequestStop();
        };

        var btnClear = new Button()
        {
            Text = "Clear",
            X = Pos.Right(btnCancel),
            Y = Pos.Bottom(this.textView),
        };
        btnClear.Accepting += (s, e) =>
        {
            e.Cancel = true;
            this.textField.Text = string.Empty;
        };

        this.win.Add(btnOk);
        this.win.Add(btnCancel);
        this.win.Add(btnClear);
    }

    private void SetupMultiLineForced()
    {
        var cbMultiLine = new CheckBox()
        {
            Text = "Enable Newlines",
            X = Pos.AnchorEnd(),
            CheckedState = CheckState.Checked,
            Enabled = false
        };
        win.Add(cbMultiLine);
    }

    private void SetupMultiLineOptional()
    {
        // Initial state
        SetEnableNewlines();

        var cbMultiLine = new CheckBox()
        {
            Text = "Enable Newlines",
            X = Pos.AnchorEnd(),
            CheckedState = lastKnownEnableNewlines
        };
        cbMultiLine.CheckedStateChanging += (s, e) =>
        {
            SetEnableNewlines(e.NewValue);
        };
        win.Add(cbMultiLine);
    }

    private void SetEnableNewlines()
    {
       SetEnableNewlines(lastKnownEnableNewlines);
    }

    private void SetEnableNewlines(CheckState newValue)
    {
        lastKnownEnableNewlines = newValue;
        multiLineChecked = textView.AllowsReturn = newValue == CheckState.Checked;
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
        this.ResultText = this.textView.Text.ToString();
        Application.RequestStop();
    }

    private void TextViewKeyPress(object? sender, Key key)
    {
        if (key == Key.Enter && !IsMultiLine())
        {
            this.Accept();
            key.Handled = true;
        }
    }

    private bool IsMultiLine()
    {
        return this.args.MultiLine || (multiLineChecked ?? false);
    }
}
