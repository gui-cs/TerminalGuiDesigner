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
            
            var i2 = i;

            buttons[i].Accepting += (s,e) => {
                Result = i2;
                e.Cancel = true;
                Application.RequestStop();

            };
        }

        buttonPanel.LayoutSubviews();

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

        btn1.FocusDeepest(NavigationDirection.Forward, TabBehavior.TabGroup);
    }

    internal static int Query(string title, string message, params string[] options)
    {
        var dlg = new ChoicesDialog(title, message, options);
        Application.Run(dlg);
        return dlg.Result;
    }
    internal static bool Confirm(string title, string message, string okText = "Yes", string cancelText = "No")
    {
        var dlg = new ChoicesDialog(title, message, okText, cancelText);
        Application.Run(dlg);
        return dlg.Result == 0;
    }
}
