
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.18.0
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace TerminalGuiDesigner.UI.Windows {
    using System;
    using Terminal.Gui;
    
    
    public partial class SizeEditor : Terminal.Gui.Dialog {
        
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
            this.Width = 20;
            this.Height = 7;
            this.X = Pos.Center();
            this.Y = Pos.Center();
            this.Modal = true;
            this.Text = "";
            this.Border.BorderStyle = Terminal.Gui.BorderStyle.Single;
            this.Border.Effect3D = true;
            this.Border.DrawMarginFrame = true;
            this.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Title = "Size";
            this.label1.Width = 4;
            this.label1.Height = 1;
            this.label1.X = 1;
            this.label1.Y = 0;
            this.label1.Data = "label1";
            this.label1.Text = "Width:";
            this.label1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.label1);
            this.tfWidth.Width = Dim.Fill(1);
            this.tfWidth.Height = 1;
            this.tfWidth.X = 8;
            this.tfWidth.Y = 0;
            this.tfWidth.Secret = false;
            this.tfWidth.Data = "tfWidth";
            this.tfWidth.Text = "";
            this.tfWidth.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.tfWidth);
            this.label12.Width = 4;
            this.label12.Height = 1;
            this.label12.X = 0;
            this.label12.Y = 2;
            this.label12.Data = "label12";
            this.label12.Text = "Height:";
            this.label12.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.label12);
            this.tfHeight.Width = Dim.Fill(1);
            this.tfHeight.Height = 1;
            this.tfHeight.X = 8;
            this.tfHeight.Y = 2;
            this.tfHeight.Secret = false;
            this.tfHeight.Data = "tfHeight";
            this.tfHeight.Text = "";
            this.tfHeight.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.tfHeight);
            this.btnOk.Width = 6;
            this.btnOk.X = 0;
            this.btnOk.Y = 4;
            this.btnOk.Data = "btnOk";
            this.btnOk.Text = "Ok";
            this.btnOk.TextAlignment = Terminal.Gui.TextAlignment.Centered;
            this.btnOk.IsDefault = false;
            this.Add(this.btnOk);
            this.btnCancel.Width = 10;
            this.btnCancel.X = 8;
            this.btnCancel.Y = 4;
            this.btnCancel.Data = "btnCancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlignment = Terminal.Gui.TextAlignment.Centered;
            this.btnCancel.IsDefault = false;
            this.Add(this.btnCancel);
        }
    }
}
