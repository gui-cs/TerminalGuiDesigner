//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YourNamespace {
    using System;
    using Terminal.Gui;
    
    
    public partial class MyWindow : Terminal.Gui.Window {
        
        private Terminal.Gui.TabView tabview1;
        
        private Terminal.Gui.Label lblHelloWorld;
        
        private Terminal.Gui.Button button1;
        
        private Terminal.Gui.CheckBox checkbox1;
        
        private Terminal.Gui.Label label2;
        
        private Terminal.Gui.Label label3;
        
        private void InitializeComponent() {
            this.Text = "";
            this.Width = Dim.Fill(0);
            this.Height = Dim.Fill(0);
            this.X = 0;
            this.Y = 0;
            this.TextAlignment = TextAlignment.Centered;
            this.Title = "Welcome to Demo";
            this.tabview1 = new Terminal.Gui.TabView();
            this.tabview1.Data = "tabview1";
            this.tabview1.Text = "";
            this.tabview1.Width = 50;
            this.tabview1.Height = Dim.Fill(0);
            this.tabview1.X = 0;
            this.tabview1.Y = 0;
            this.tabview1.TextAlignment = TextAlignment.Left;
            this.tabview1.MaxTabTextWidth = 30u;
            this.tabview1.Style.ShowBorder = true;
            this.tabview1.Style.ShowTopLine = true;
            this.tabview1.Style.TabsOnBottom = false;
            Terminal.Gui.TabView.Tab tabview1Tab1;
            tabview1Tab1 = new Terminal.Gui.TabView.Tab("Tab1", new View());
            tabview1Tab1.View.Width = Dim.Fill();
            tabview1Tab1.View.Height = Dim.Fill();
            this.lblHelloWorld = new Terminal.Gui.Label();
            this.lblHelloWorld.Data = "lblHelloWorld";
            this.lblHelloWorld.Text = "Heya";
            this.lblHelloWorld.Width = Dim.Fill(4);
            this.lblHelloWorld.Height = 1;
            this.lblHelloWorld.X = 0;
            this.lblHelloWorld.Y = Pos.Percent(50);
            this.lblHelloWorld.TextAlignment = TextAlignment.Centered;
            tabview1Tab1.View.Add(this.lblHelloWorld);
            this.button1 = new Terminal.Gui.Button();
            this.button1.Data = "button1";
            this.button1.Text = "Press Me!";
            this.button1.Width = 15;
            this.button1.Height = 1;
            this.button1.X = 5;
            this.button1.Y = 3;
            this.button1.TextAlignment = TextAlignment.Centered;
            this.button1.IsDefault = true;
            tabview1Tab1.View.Add(this.button1);
            this.checkbox1 = new Terminal.Gui.CheckBox();
            this.checkbox1.Data = "checkbox1";
            this.checkbox1.Text = "Check me";
            this.checkbox1.Width = 4;
            this.checkbox1.Height = 1;
            this.checkbox1.X = 0;
            this.checkbox1.Y = 0;
            this.checkbox1.TextAlignment = TextAlignment.Left;
            this.checkbox1.Checked = true;
            tabview1Tab1.View.Add(this.checkbox1);
            tabview1.AddTab(tabview1Tab1, false);
            Terminal.Gui.TabView.Tab tabview1Tab2;
            tabview1Tab2 = new Terminal.Gui.TabView.Tab("Tab2", new View());
            tabview1Tab2.View.Width = Dim.Fill();
            tabview1Tab2.View.Height = Dim.Fill();
            this.label2 = new Terminal.Gui.Label();
            this.label2.Data = "label2";
            this.label2.Text = "Label 1";
            this.label2.Width = 7;
            this.label2.Height = 1;
            this.label2.X = 10;
            this.label2.Y = 3;
            this.label2.TextAlignment = TextAlignment.Left;
            tabview1Tab2.View.Add(this.label2);
            this.label3 = new Terminal.Gui.Label();
            this.label3.Data = "label3";
            this.label3.Text = "Label 2";
            this.label3.Width = 7;
            this.label3.Height = 1;
            this.label3.X = 0;
            this.label3.Y = 0;
            this.label3.TextAlignment = TextAlignment.Left;
            tabview1Tab2.View.Add(this.label3);
            tabview1.AddTab(tabview1Tab2, false);
            this.Add(this.tabview1);
        }
    }
}
