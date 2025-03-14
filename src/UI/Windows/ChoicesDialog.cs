﻿
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.18.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace TerminalGuiDesigner.UI.Windows;

using System.Text;
using Terminal.Gui;


/// <summary>
/// Popup dialog with a message and 1 or more buttons the user can press.
/// </summary>
public partial class ChoicesDialog
{
    /// <summary>
    /// The index of the button user clicked (starting at 0).
    /// </summary>
    public int Result { get; private set; }

    private string _title;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="options"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ChoicesDialog(string title, string message, params string[] options) {
        
        const int defaultWidth = 50;

        InitializeComponent();

        if (options.Length == 0 || options.Length > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Too many or too few buttons");
        }

        var buttons = new Button[]
        {
            btn1,btn2,btn3,btn4
        };

        for (int i = 0; i < options.Length;i++)
        {
            // add space for right hand side shadow
            buttons[i].Text = options[i] + " ";

            // TODO think it depends if it is default if we have to do this hack
            buttons[i].Width = Dim.Auto();

            
            var i2 = i;

            buttons[i].Accept += (s,e) => {
                Result = i2;
                Application.RequestStop();
            };

            buttons[i].DrawContentComplete += (s,r) =>
                ChoicesDialog.PaintShadow(buttons[i2], ColorScheme);
        }

        // hide other buttons
        for(int i=options.Length;i<buttons.Length;i++)
        {
            buttonPanel.Remove(buttons[i]);
        }

        _title = title;
        label1.Text = message;

        int buttonWidth;

        // align buttons bottom of dialog 
        buttonPanel.Width = buttonWidth = buttons.Sum(b=>buttonPanel.Subviews.Contains(b) ? b.Frame.Width : 0) + 1;

        int maxWidthLine = TextFormatter.GetSumMaxCharWidth(message);
        if (maxWidthLine > Application.Driver.Cols)
        {
            maxWidthLine = Application.Driver.Cols;
        }
        
        maxWidthLine = Math.Max(maxWidthLine, defaultWidth);
           

        int textWidth = Math.Min(TextFormatter.GetSumMaxCharWidth(message, maxWidthLine), Application.Driver.Cols);
        int textHeight =  message.Count (c=>c=='\n') + 4;
        int msgboxHeight = Math.Min(Math.Max(1, textHeight) + 4, Application.Driver.Rows); // textHeight + (top + top padding + buttons + bottom)

        Width = Math.Min(Math.Max(maxWidthLine, Math.Max(Title.GetColumns(), Math.Max(textWidth + 2, buttonWidth))), Application.Driver.Cols);
        Height = msgboxHeight;
    }

    /// <inheritdoc/>
    public override void OnDrawContentComplete(Rectangle bounds)
    {
        base.OnDrawContentComplete(bounds);

        var screenTopLeft = FrameToScreen();
        Driver.Move(screenTopLeft.X+2, screenTopLeft.Y);
        
        var padding = ((bounds.Width - _title.EnumerateRunes().Sum(v=>v.GetColumns())) / 2) - 1;

        Driver.SetAttribute(
            new Attribute(ColorScheme.Normal.Foreground, ColorScheme.Normal.Background));
        
        Driver.AddStr(string.Join("",Enumerable.Repeat(ConfigurationManager.Glyphs.HLineHv, padding)));

        Driver.SetAttribute(
            new Attribute(ColorScheme.Normal.Background, ColorScheme.Normal.Foreground));
        Driver.AddStr(_title);

        Driver.SetAttribute(
            new Attribute(ColorScheme.Normal.Foreground, ColorScheme.Normal.Background));

        StringBuilder sb = new StringBuilder();
        sb.Append(ConfigurationManager.Glyphs.HLineHv);

        Driver.AddStr(string.Join("", Enumerable.Repeat(ConfigurationManager.Glyphs.HLineHv.ToString(), padding)));
    }

    internal static int Query(string title, string message, params string[] options)
    {
        var dlg = new ChoicesDialog(title, message, options);
        Application.Run(dlg);
        return dlg.Result;
    }

    internal static void PaintShadow(Button btn, ColorScheme backgroundScheme)
    {
        var bounds = btn.GetContentSize();

        Attribute buttonColor = btn.HasFocus ? 
            new Terminal.Gui.Attribute(btn.ColorScheme.Focus.Foreground, btn.ColorScheme.Focus.Background):
            new Terminal.Gui.Attribute(btn.ColorScheme.Normal.Foreground, btn.ColorScheme.Normal.Background);

        Driver.SetAttribute(buttonColor);

        if (btn.IsDefault)
        {
            var rightDefault = Driver != null ? ConfigurationManager.Glyphs.RightDefaultIndicator : new Rune('>');

            // draw the 'end' button symbol one in
            btn.AddRune(bounds.Width - 3, 0, rightDefault);
        }

        btn.AddRune(bounds.Width - 2, 0, new System.Text.Rune(']'));
        btn.AddRune(0, 0, new System.Text.Rune('['));

        var backgroundColor = backgroundScheme.Normal.Background;

        // shadow color
        Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Black, backgroundColor));

        // end shadow (right)
        btn.AddRune(bounds.Width - 1, 0, new System.Text.Rune('▄'));

        // leave whitespace in lower left in parent/default background color
        Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Black, backgroundColor));
        btn.AddRune(0, 1, new System.Text.Rune(' '));

        // The color for rendering shadow is 'black' + parent/default background color
        Driver.SetAttribute(new Terminal.Gui.Attribute(backgroundColor, Color.Black));

        // underline shadow                
        for (int x = 1; x < bounds.Width; x++)
        {
            btn.AddRune(x, 1, new System.Text.Rune('▄'));
        }
    }

    internal static bool Confirm(string title, string message, string okText = "Yes", string cancelText = "No")
    {
        var dlg = new ChoicesDialog(title, message, okText, cancelText);
        Application.Run(dlg);
        return dlg.Result == 0;
    }
}
