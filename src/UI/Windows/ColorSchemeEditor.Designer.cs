
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
    
    
    public partial class ColorSchemeEditor : Terminal.Gui.Dialog {
        
        private Terminal.Gui.ColorScheme dialogBackground;
        
        private Terminal.Gui.ColorScheme buttons;
        
        private Terminal.Gui.Label label2;
        
        private Terminal.Gui.Label lblForegroundNormal;
        
        private Terminal.Gui.Label label1;
        
        private Terminal.Gui.Label lblBackgroundNormal;
        
        private Terminal.Gui.Button btnEditNormal;
        
        private Terminal.Gui.Label label22;
        
        private Terminal.Gui.Label lblForegroundHotNormal;
        
        private Terminal.Gui.Label lblHotNormalSlash;
        
        private Terminal.Gui.Label lblBackgroundHotNormal;
        
        private Terminal.Gui.Button btnEditHotNormal;
        
        private Terminal.Gui.Label lblFocus;
        
        private Terminal.Gui.Label lblForegroundFocus;
        
        private Terminal.Gui.Label lblHotNormalSlash2;
        
        private Terminal.Gui.Label lblBackgroundFocus;
        
        private Terminal.Gui.Button btnEditFocus;
        
        private Terminal.Gui.Label label223;
        
        private Terminal.Gui.Label lblForegroundHotFocus;
        
        private Terminal.Gui.Label lblHotNormalSlash3;
        
        private Terminal.Gui.Label lblBackgroundHotFocus;
        
        private Terminal.Gui.Button btnEditHotFocus;
        
        private Terminal.Gui.Label label2232;
        
        private Terminal.Gui.Label lblForegroundDisabled;
        
        private Terminal.Gui.Label lblHotNormalSlash32;
        
        private Terminal.Gui.Label lblBackgroundDisabled;
        
        private Terminal.Gui.Button btnEditDisabled;
        
        private Terminal.Gui.Button btnOk;
        
        private Terminal.Gui.Button btnCancel;
        
        private void InitializeComponent() {
            this.btnCancel = new Terminal.Gui.Button();
            this.btnOk = new Terminal.Gui.Button();
            this.btnEditDisabled = new Terminal.Gui.Button();
            this.lblBackgroundDisabled = new Terminal.Gui.Label();
            this.lblHotNormalSlash32 = new Terminal.Gui.Label();
            this.lblForegroundDisabled = new Terminal.Gui.Label();
            this.label2232 = new Terminal.Gui.Label();
            this.btnEditHotFocus = new Terminal.Gui.Button();
            this.lblBackgroundHotFocus = new Terminal.Gui.Label();
            this.lblHotNormalSlash3 = new Terminal.Gui.Label();
            this.lblForegroundHotFocus = new Terminal.Gui.Label();
            this.label223 = new Terminal.Gui.Label();
            this.btnEditFocus = new Terminal.Gui.Button();
            this.lblBackgroundFocus = new Terminal.Gui.Label();
            this.lblHotNormalSlash2 = new Terminal.Gui.Label();
            this.lblForegroundFocus = new Terminal.Gui.Label();
            this.lblFocus = new Terminal.Gui.Label();
            this.btnEditHotNormal = new Terminal.Gui.Button();
            this.lblBackgroundHotNormal = new Terminal.Gui.Label();
            this.lblHotNormalSlash = new Terminal.Gui.Label();
            this.lblForegroundHotNormal = new Terminal.Gui.Label();
            this.label22 = new Terminal.Gui.Label();
            this.btnEditNormal = new Terminal.Gui.Button();
            this.lblBackgroundNormal = new Terminal.Gui.Label();
            this.label1 = new Terminal.Gui.Label();
            this.lblForegroundNormal = new Terminal.Gui.Label();
            this.label2 = new Terminal.Gui.Label();
            this.dialogBackground = new Terminal.Gui.ColorScheme(new Terminal.Gui.Attribute(4294967295u, 4285953654u), new Terminal.Gui.Attribute(4294967295u, 4285953654u), new Terminal.Gui.Attribute(4294967295u, 4285953654u), new Terminal.Gui.Attribute(4278190080u, 4278190080u), new Terminal.Gui.Attribute(4294967295u, 4285953654u));
            this.buttons = new Terminal.Gui.ColorScheme(new Terminal.Gui.Attribute(4285953654u, 4294967295u), new Terminal.Gui.Attribute(4294901760u, 4294967040u), new Terminal.Gui.Attribute(4278190080u, 4294967295u), new Terminal.Gui.Attribute(4278190080u, 4278190080u), new Terminal.Gui.Attribute(4278190080u, 4294967040u));
            this.Width = 31;
            this.Height = 11;
            this.X = Pos.Center();
            this.Y = Pos.Center();
            this.Visible = true;
            this.Arrangement = (Terminal.Gui.ViewArrangement.Movable | Terminal.Gui.ViewArrangement.Overlapped);
            this.ColorScheme = this.dialogBackground;
            this.CanFocus = true;
            this.ShadowStyle = Terminal.Gui.ShadowStyle.Transparent;
            this.Modal = true;
            this.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Title = "Color Scheme Editor";
            this.label2.Width = 10;
            this.label2.Height = 1;
            this.label2.X = 1;
            this.label2.Y = 0;
            this.label2.Visible = true;
            this.label2.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.label2.CanFocus = false;
            this.label2.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.label2.Data = "label2";
            this.label2.Text = "Normal   :";
            this.label2.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.label2);
            this.lblForegroundNormal.Width = 1;
            this.lblForegroundNormal.Height = 1;
            this.lblForegroundNormal.X = Pos.Right(label2) + 1;
            this.lblForegroundNormal.Y = 0;
            this.lblForegroundNormal.Visible = true;
            this.lblForegroundNormal.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblForegroundNormal.CanFocus = false;
            this.lblForegroundNormal.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblForegroundNormal.Data = "lblForegroundNormal";
            this.lblForegroundNormal.Text = " ";
            this.lblForegroundNormal.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblForegroundNormal);
            this.label1.Width = 1;
            this.label1.Height = 1;
            this.label1.X = 13;
            this.label1.Y = 0;
            this.label1.Visible = true;
            this.label1.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.label1.CanFocus = false;
            this.label1.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.label1.Data = "label1";
            this.label1.Text = "\\";
            this.label1.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.label1);
            this.lblBackgroundNormal.Width = 1;
            this.lblBackgroundNormal.Height = 1;
            this.lblBackgroundNormal.X = Pos.Right(label2) + 3;
            this.lblBackgroundNormal.Y = 0;
            this.lblBackgroundNormal.Visible = true;
            this.lblBackgroundNormal.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblBackgroundNormal.CanFocus = false;
            this.lblBackgroundNormal.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblBackgroundNormal.Data = "lblBackgroundNormal";
            this.lblBackgroundNormal.Text = " ";
            this.lblBackgroundNormal.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblBackgroundNormal);
            this.btnEditNormal.Width = 13;
            this.btnEditNormal.Height = Dim.Auto();
            this.btnEditNormal.X = 15;
            this.btnEditNormal.Y = 0;
            this.btnEditNormal.Visible = true;
            this.btnEditNormal.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.btnEditNormal.ColorScheme = this.buttons;
            this.btnEditNormal.CanFocus = true;
            this.btnEditNormal.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.btnEditNormal.Data = "btnEditNormal";
            this.btnEditNormal.Text = "Choose...";
            this.btnEditNormal.TextAlignment = Terminal.Gui.Alignment.Center;
            this.btnEditNormal.IsDefault = false;
            this.Add(this.btnEditNormal);
            this.label22.Width = 10;
            this.label22.Height = 1;
            this.label22.X = 1;
            this.label22.Y = 1;
            this.label22.Visible = true;
            this.label22.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.label22.CanFocus = false;
            this.label22.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.label22.Data = "label22";
            this.label22.Text = "HotNormal:";
            this.label22.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.label22);
            this.lblForegroundHotNormal.Width = 1;
            this.lblForegroundHotNormal.Height = 1;
            this.lblForegroundHotNormal.X = Pos.Right(label22) + 1;
            this.lblForegroundHotNormal.Y = 1;
            this.lblForegroundHotNormal.Visible = true;
            this.lblForegroundHotNormal.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblForegroundHotNormal.CanFocus = false;
            this.lblForegroundHotNormal.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblForegroundHotNormal.Data = "lblForegroundHotNormal";
            this.lblForegroundHotNormal.Text = " ";
            this.lblForegroundHotNormal.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblForegroundHotNormal);
            this.lblHotNormalSlash.Width = 1;
            this.lblHotNormalSlash.Height = 1;
            this.lblHotNormalSlash.X = 13;
            this.lblHotNormalSlash.Y = 1;
            this.lblHotNormalSlash.Visible = true;
            this.lblHotNormalSlash.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblHotNormalSlash.CanFocus = false;
            this.lblHotNormalSlash.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblHotNormalSlash.Data = "lblHotNormalSlash";
            this.lblHotNormalSlash.Text = "\\";
            this.lblHotNormalSlash.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblHotNormalSlash);
            this.lblBackgroundHotNormal.Width = 1;
            this.lblBackgroundHotNormal.Height = 1;
            this.lblBackgroundHotNormal.X = Pos.Right(label22) + 3;
            this.lblBackgroundHotNormal.Y = 1;
            this.lblBackgroundHotNormal.Visible = true;
            this.lblBackgroundHotNormal.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblBackgroundHotNormal.CanFocus = false;
            this.lblBackgroundHotNormal.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblBackgroundHotNormal.Data = "lblBackgroundHotNormal";
            this.lblBackgroundHotNormal.Text = " ";
            this.lblBackgroundHotNormal.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblBackgroundHotNormal);
            this.btnEditHotNormal.Width = 13;
            this.btnEditHotNormal.Height = Dim.Auto();
            this.btnEditHotNormal.X = 15;
            this.btnEditHotNormal.Y = 1;
            this.btnEditHotNormal.Visible = true;
            this.btnEditHotNormal.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.btnEditHotNormal.ColorScheme = this.buttons;
            this.btnEditHotNormal.CanFocus = true;
            this.btnEditHotNormal.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.btnEditHotNormal.Data = "btnEditHotNormal";
            this.btnEditHotNormal.Text = "Choose...";
            this.btnEditHotNormal.TextAlignment = Terminal.Gui.Alignment.Center;
            this.btnEditHotNormal.IsDefault = false;
            this.Add(this.btnEditHotNormal);
            this.lblFocus.Width = 10;
            this.lblFocus.Height = 1;
            this.lblFocus.X = 1;
            this.lblFocus.Y = 2;
            this.lblFocus.Visible = true;
            this.lblFocus.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblFocus.CanFocus = false;
            this.lblFocus.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblFocus.Data = "lblFocus";
            this.lblFocus.Text = "Focus    :";
            this.lblFocus.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblFocus);
            this.lblForegroundFocus.Width = 1;
            this.lblForegroundFocus.Height = 1;
            this.lblForegroundFocus.X = Pos.Right(lblFocus) + 1;
            this.lblForegroundFocus.Y = 2;
            this.lblForegroundFocus.Visible = true;
            this.lblForegroundFocus.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblForegroundFocus.CanFocus = false;
            this.lblForegroundFocus.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblForegroundFocus.Data = "lblForegroundFocus";
            this.lblForegroundFocus.Text = " ";
            this.lblForegroundFocus.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblForegroundFocus);
            this.lblHotNormalSlash2.Width = 1;
            this.lblHotNormalSlash2.Height = 1;
            this.lblHotNormalSlash2.X = 13;
            this.lblHotNormalSlash2.Y = 2;
            this.lblHotNormalSlash2.Visible = true;
            this.lblHotNormalSlash2.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblHotNormalSlash2.CanFocus = false;
            this.lblHotNormalSlash2.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblHotNormalSlash2.Data = "lblHotNormalSlash2";
            this.lblHotNormalSlash2.Text = "\\";
            this.lblHotNormalSlash2.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblHotNormalSlash2);
            this.lblBackgroundFocus.Width = 1;
            this.lblBackgroundFocus.Height = 1;
            this.lblBackgroundFocus.X = Pos.Right(lblFocus) + 3;
            this.lblBackgroundFocus.Y = 2;
            this.lblBackgroundFocus.Visible = true;
            this.lblBackgroundFocus.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblBackgroundFocus.CanFocus = false;
            this.lblBackgroundFocus.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblBackgroundFocus.Data = "lblBackgroundFocus";
            this.lblBackgroundFocus.Text = " ";
            this.lblBackgroundFocus.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblBackgroundFocus);
            this.btnEditFocus.Width = 13;
            this.btnEditFocus.Height = Dim.Auto();
            this.btnEditFocus.X = 15;
            this.btnEditFocus.Y = 2;
            this.btnEditFocus.Visible = true;
            this.btnEditFocus.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.btnEditFocus.ColorScheme = this.buttons;
            this.btnEditFocus.CanFocus = true;
            this.btnEditFocus.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.btnEditFocus.Data = "btnEditFocus";
            this.btnEditFocus.Text = "Choose...";
            this.btnEditFocus.TextAlignment = Terminal.Gui.Alignment.Center;
            this.btnEditFocus.IsDefault = false;
            this.Add(this.btnEditFocus);
            this.label223.Width = 10;
            this.label223.Height = 1;
            this.label223.X = 1;
            this.label223.Y = 3;
            this.label223.Visible = true;
            this.label223.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.label223.CanFocus = false;
            this.label223.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.label223.Data = "label223";
            this.label223.Text = "HotFocus :";
            this.label223.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.label223);
            this.lblForegroundHotFocus.Width = 1;
            this.lblForegroundHotFocus.Height = 1;
            this.lblForegroundHotFocus.X = Pos.Right(label223) + 1;
            this.lblForegroundHotFocus.Y = 3;
            this.lblForegroundHotFocus.Visible = true;
            this.lblForegroundHotFocus.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblForegroundHotFocus.CanFocus = false;
            this.lblForegroundHotFocus.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblForegroundHotFocus.Data = "lblForegroundHotFocus";
            this.lblForegroundHotFocus.Text = " ";
            this.lblForegroundHotFocus.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblForegroundHotFocus);
            this.lblHotNormalSlash3.Width = 1;
            this.lblHotNormalSlash3.Height = 1;
            this.lblHotNormalSlash3.X = 13;
            this.lblHotNormalSlash3.Y = 3;
            this.lblHotNormalSlash3.Visible = true;
            this.lblHotNormalSlash3.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblHotNormalSlash3.CanFocus = false;
            this.lblHotNormalSlash3.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblHotNormalSlash3.Data = "lblHotNormalSlash3";
            this.lblHotNormalSlash3.Text = "\\";
            this.lblHotNormalSlash3.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblHotNormalSlash3);
            this.lblBackgroundHotFocus.Width = 1;
            this.lblBackgroundHotFocus.Height = 1;
            this.lblBackgroundHotFocus.X = Pos.Right(label223) + 3;
            this.lblBackgroundHotFocus.Y = 3;
            this.lblBackgroundHotFocus.Visible = true;
            this.lblBackgroundHotFocus.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblBackgroundHotFocus.CanFocus = false;
            this.lblBackgroundHotFocus.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblBackgroundHotFocus.Data = "lblBackgroundHotFocus";
            this.lblBackgroundHotFocus.Text = " ";
            this.lblBackgroundHotFocus.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblBackgroundHotFocus);
            this.btnEditHotFocus.Width = 13;
            this.btnEditHotFocus.Height = Dim.Auto();
            this.btnEditHotFocus.X = 15;
            this.btnEditHotFocus.Y = 3;
            this.btnEditHotFocus.Visible = true;
            this.btnEditHotFocus.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.btnEditHotFocus.ColorScheme = this.buttons;
            this.btnEditHotFocus.CanFocus = true;
            this.btnEditHotFocus.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.btnEditHotFocus.Data = "btnEditHotFocus";
            this.btnEditHotFocus.Text = "Choose...";
            this.btnEditHotFocus.TextAlignment = Terminal.Gui.Alignment.Center;
            this.btnEditHotFocus.IsDefault = false;
            this.Add(this.btnEditHotFocus);
            this.label2232.Width = 10;
            this.label2232.Height = 1;
            this.label2232.X = 1;
            this.label2232.Y = 4;
            this.label2232.Visible = true;
            this.label2232.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.label2232.CanFocus = false;
            this.label2232.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.label2232.Data = "label2232";
            this.label2232.Text = "Disabled :";
            this.label2232.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.label2232);
            this.lblForegroundDisabled.Width = 1;
            this.lblForegroundDisabled.Height = 1;
            this.lblForegroundDisabled.X = Pos.Right(label2232) + 1;
            this.lblForegroundDisabled.Y = 4;
            this.lblForegroundDisabled.Visible = true;
            this.lblForegroundDisabled.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblForegroundDisabled.CanFocus = false;
            this.lblForegroundDisabled.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblForegroundDisabled.Data = "lblForegroundDisabled";
            this.lblForegroundDisabled.Text = " ";
            this.lblForegroundDisabled.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblForegroundDisabled);
            this.lblHotNormalSlash32.Width = 1;
            this.lblHotNormalSlash32.Height = 1;
            this.lblHotNormalSlash32.X = 13;
            this.lblHotNormalSlash32.Y = 4;
            this.lblHotNormalSlash32.Visible = true;
            this.lblHotNormalSlash32.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblHotNormalSlash32.CanFocus = false;
            this.lblHotNormalSlash32.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblHotNormalSlash32.Data = "lblHotNormalSlash32";
            this.lblHotNormalSlash32.Text = "\\";
            this.lblHotNormalSlash32.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblHotNormalSlash32);
            this.lblBackgroundDisabled.Width = 1;
            this.lblBackgroundDisabled.Height = 1;
            this.lblBackgroundDisabled.X = Pos.Right(label2232) + 3;
            this.lblBackgroundDisabled.Y = 4;
            this.lblBackgroundDisabled.Visible = true;
            this.lblBackgroundDisabled.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.lblBackgroundDisabled.CanFocus = false;
            this.lblBackgroundDisabled.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.lblBackgroundDisabled.Data = "lblBackgroundDisabled";
            this.lblBackgroundDisabled.Text = " ";
            this.lblBackgroundDisabled.TextAlignment = Terminal.Gui.Alignment.Start;
            this.Add(this.lblBackgroundDisabled);
            this.btnEditDisabled.Width = 13;
            this.btnEditDisabled.Height = Dim.Auto();
            this.btnEditDisabled.X = 15;
            this.btnEditDisabled.Y = 4;
            this.btnEditDisabled.Visible = true;
            this.btnEditDisabled.Arrangement = Terminal.Gui.ViewArrangement.Fixed;
            this.btnEditDisabled.ColorScheme = this.buttons;
            this.btnEditDisabled.CanFocus = true;
            this.btnEditDisabled.ShadowStyle = Terminal.Gui.ShadowStyle.None;
            this.btnEditDisabled.Data = "btnEditDisabled";
            this.btnEditDisabled.Text = "Choose...";
            this.btnEditDisabled.TextAlignment = Terminal.Gui.Alignment.Center;
            this.btnEditDisabled.IsDefault = false;
            this.Add(this.btnEditDisabled);
            this.btnOk.Width = Dim.Auto();
            this.btnOk.Height = Dim.Auto();
            this.btnOk.X = 3;
            this.btnOk.Y = 6;
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
            this.btnCancel.X = 13;
            this.btnCancel.Y = 6;
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
