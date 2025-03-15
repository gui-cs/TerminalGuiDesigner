
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v2.0.0.0
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace TerminalGuiDesigner.UI.Windows {
    using System;
    using Terminal.Gui;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    
    
    public partial class SizeEditor : Terminal.Gui.Dialog {
        
        private Terminal.Gui.ColorScheme dialogBackground;
        
        private Terminal.Gui.ColorScheme buttons;
        
        private Terminal.Gui.Label label1;
        
        private Terminal.Gui.TextField tfWidth;
        
        private Terminal.Gui.Label label12;
        
        private Terminal.Gui.TextField tfHeight;
        
        private Terminal.Gui.Button btnOk;
        
        private Terminal.Gui.Button btnCancel;
        
        private void InitializeComponent() {
            this.btnCancel = new Terminal.Gui.Button();
            this.btnOk = new Terminal.Gui.Button();
            this.tfHeight = new Terminal.Gui.TextField();
            this.label12 = new Terminal.Gui.Label();
            this.tfWidth = new Terminal.Gui.TextField();
            this.label1 = new Terminal.Gui.Label();
            this.dialogBackground = new Terminal.Gui.ColorScheme(new Terminal.Gui.Attribute(4294967295u, 4285953654u), new Terminal.Gui.Attribute(4294967295u, 4285953654u), new Terminal.Gui.Attribute(4294967295u, 4285953654u), new Terminal.Gui.Attribute(4278190080u, 4278190080u), new Terminal.Gui.Attribute(4294967295u, 4285953654u));
            this.buttons = new Terminal.Gui.ColorScheme(new Terminal.Gui.Attribute(4285953654u, 4294967295u), new Terminal.Gui.Attribute(4294901760u, 4294967040u), new Terminal.Gui.Attribute(4278190080u, 4294967295u), new Terminal.Gui.Attribute(4278190080u, 4278190080u), new Terminal.Gui.Attribute(4278190080u, 4294967040u));
            this.Width = 21;
            this.Height = 9;
            this.X = Pos.Center();
            this.Y = Pos.Center();
            this.Visible = true;
            this.Arrangement = (Terminal.Gui.ViewArrangement.Movable | Terminal.Gui.ViewArrangement.Overlapped);
            this.ColorScheme = this.dialogBackground;
            this.CanFocus = true;
            this.ShadowStyle = Terminal.Gui.ShadowStyle.Transparent;
            this.Modal = true;
            this.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Title = "Size";
            this.label1.Width = Dim.Auto();
            this.label1.Height = 1;
            this.label1.X = 1;
            this.label1.Y = 0;
            this.label1.Visible = true;
            this.label1.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.label1.CanFocus = false;
            this.label1.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.label1.Data = "label1";
            this.label1.Text = "Width:";
            this.label1.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.label1);
            this.tfWidth.Width = Dim.Fill(1);
            this.tfWidth.Height = 1;
            this.tfWidth.X = 8;
            this.tfWidth.Y = 0;
            this.tfWidth.Visible = true;
            this.tfWidth.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.tfWidth.CanFocus = true;
            this.tfWidth.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.tfWidth.Secret = false;
            this.tfWidth.Data = "tfWidth";
            this.tfWidth.Text = "";
            this.tfWidth.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.tfWidth);
            this.label12.Width = Dim.Auto();
            this.label12.Height = 1;
            this.label12.X = 0;
            this.label12.Y = 2;
            this.label12.Visible = true;
            this.label12.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.label12.CanFocus = false;
            this.label12.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.label12.Data = "label12";
            this.label12.Text = "Height:";
            this.label12.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.label12);
            this.tfHeight.Width = Dim.Fill(1);
            this.tfHeight.Height = 1;
            this.tfHeight.X = 8;
            this.tfHeight.Y = 2;
            this.tfHeight.Visible = true;
            this.tfHeight.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.tfHeight.CanFocus = true;
            this.tfHeight.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.tfHeight.Secret = false;
            this.tfHeight.Data = "tfHeight";
            this.tfHeight.Text = "";
            this.tfHeight.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.tfHeight);
            this.btnOk.Width = Dim.Auto();
            this.btnOk.Height = Dim.Auto();
            this.btnOk.X = 0;
            this.btnOk.Y = 4;
            this.btnOk.Visible = true;
            this.btnOk.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.btnOk.ColorScheme = this.buttons;
            this.btnOk.CanFocus = true;
            this.btnOk.ShadowStyle = Terminal.Gui.ShadowStyle.Opaque;
            this.btnOk.Data = "btnOk";
            this.btnOk.Text = "Ok";
            this.btnOk.TextAlignment = Terminal.Gui.Alignment.Center;
            this.btnOk.IsDefault = false;
            this.Add(this.btnOk);
            this.btnCancel.Width = Dim.Auto();
            this.btnCancel.Height = Dim.Auto();
            this.btnCancel.X = 8;
            this.btnCancel.Y = 4;
            this.btnCancel.Visible = true;
            this.btnCancel.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.btnCancel.ColorScheme = this.buttons;
            this.btnCancel.CanFocus = true;
            this.btnCancel.ShadowStyle = Terminal.Gui.ShadowStyle.Opaque;
            this.btnCancel.Data = "btnCancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlignment = Terminal.Gui.Alignment.Center;
            this.btnCancel.IsDefault = false;
            this.Add(this.btnCancel);
        }
    }
}
