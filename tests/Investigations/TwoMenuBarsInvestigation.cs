﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Investigations
{
    internal class TwoMenuBarsInvestigation : Investigation
    {
        #region Source
        protected override string Cs => @"
namespace YourNamespace {
    using Terminal.Gui;
    
    
    public partial class Class1 {
        
        public Class1() {
            InitializeComponent();
        }
    }
}";

        protected override string DesignerCs => @"
//------------------------------------------------------------------------------

        //  <auto-generated>
        //      This code was generated by:
        //        TerminalGuiDesigner v1.1.0.0
        //      Changes to this file may cause incorrect behavior and will be lost if
        //      the code is regenerated.
        //  </auto-generated>
        // -----------------------------------------------------------------------------
namespace YourNamespace {
  using System;
  using Terminal.Gui;
  using System.Collections;
  using System.Collections.Generic;
  
  
  public partial class Class1 : Terminal.Gui.Window {
      
      private Terminal.Gui.MenuBar menuBar;
      
      private Terminal.Gui.MenuBarItem fileF9Menu;
      
      private Terminal.Gui.MenuItem editMeMenuItem;
      
      private Terminal.Gui.MenuBar menuBar2;
      
      private Terminal.Gui.MenuBarItem fileF9Menu2;
      
      private Terminal.Gui.MenuItem editMeMenuItem2;
      
      private void InitializeComponent() {
          this.menuBar2 = new Terminal.Gui.MenuBar();
          this.menuBar = new Terminal.Gui.MenuBar();
          this.Width = Dim.Fill(0);
          this.Height = Dim.Fill(0);
          this.X = 0;
          this.Y = 0;
          this.Visible = true;
          this.Modal = false;
          this.TextAlignment = Terminal.Gui.TextAlignment.Left;
          this.Title = """";
          this.menuBar.Width = Dim.Fill(0);
          this.menuBar.Height = 1;
          this.menuBar.X = 0;
          this.menuBar.Y = 0;
          this.menuBar.Visible = true;
          this.menuBar.Data = ""menuBar"";
          this.menuBar.TextAlignment = Terminal.Gui.TextAlignment.Left;
          this.fileF9Menu = new Terminal.Gui.MenuBarItem();
          this.fileF9Menu.Title = ""_File (F9)"";
          this.editMeMenuItem = new Terminal.Gui.MenuItem();
          this.editMeMenuItem.Title = ""Edit Me"";
          this.editMeMenuItem.Data = ""editMeMenuItem"";
          this.fileF9Menu.Children = new Terminal.Gui.MenuItem[] {
                  this.editMeMenuItem};
          this.menuBar.Menus = new Terminal.Gui.MenuBarItem[] {
                  this.fileF9Menu};
          this.Add(this.menuBar);
          this.menuBar2.Width = Dim.Fill(0);
          this.menuBar2.Height = 1;
          this.menuBar2.X = 0;
          this.menuBar2.Y = 4;
          this.menuBar2.Visible = true;
          this.menuBar2.Data = ""menuBar2"";
          this.menuBar2.TextAlignment = Terminal.Gui.TextAlignment.Left;
          this.fileF9Menu2 = new Terminal.Gui.MenuBarItem();
          this.fileF9Menu2.Title = ""_File (F9)"";
          this.editMeMenuItem2 = new Terminal.Gui.MenuItem();
          this.editMeMenuItem2.Title = ""Edit Me"";
          this.editMeMenuItem2.Data = ""editMeMenuItem2"";
          this.fileF9Menu2.Children = new Terminal.Gui.MenuItem[] {
                  this.editMeMenuItem2};
          this.menuBar2.Menus = new Terminal.Gui.MenuBarItem[] {
                  this.fileF9Menu2};
          this.Add(this.menuBar2);
      }
  }
}
";
        #endregion
        protected override void MakeAssertions(Design rootDesign)
        {
            var menuBars = rootDesign.View.Subviews.OfType<MenuBar>().ToArray();
            Assert.That(menuBars, Has.Length.EqualTo(2));

            Assert.That(menuBars[0].Width, Is.EqualTo(Dim.Fill(0)));
            Assert.That(menuBars[1].Width, Is.EqualTo(Dim.Fill(0)));
        }

        [Test]
        public void SimplerTest()
        {
            MenuBar menuBar;
            MenuBar menuBar2;

            var w = new Window();
            menuBar2 = new Terminal.Gui.MenuBar();
            menuBar = new Terminal.Gui.MenuBar();
            w.Width = Dim.Fill(0);
            w.Height = Dim.Fill(0);
            w.X = 0;
            w.Y = 0;
            w.Visible = true;
            w.Modal = false;
            w.Title = "";
            menuBar.Width = Dim.Fill(0);
            menuBar.Height = 1;
            menuBar.X = 0;
            menuBar.Y = 0;
            menuBar.Visible = true;
            menuBar.Data = "menuBar";
            menuBar.TextAlignment = Terminal.Gui.TextAlignment.Left;
            var fileF9Menu = new Terminal.Gui.MenuBarItem();
            fileF9Menu.Title = "_File(F9)";
            var editMeMenuItem = new Terminal.Gui.MenuItem();
            editMeMenuItem.Title = "Edit Me";
            editMeMenuItem.Data = "editMeMenuItem";
            fileF9Menu.Children = new Terminal.Gui.MenuItem[] {
             editMeMenuItem};
            menuBar.Menus = new Terminal.Gui.MenuBarItem[] {
             fileF9Menu};

            w.Add(menuBar);
            menuBar2.Width = Dim.Fill(0);
            menuBar2.Height = 1;
            menuBar2.X = 0;
            menuBar2.Y = 4;
            menuBar2.Visible = true;
            menuBar2.Data = "menuBar2";
            menuBar2.TextAlignment = Terminal.Gui.TextAlignment.Left;
            var fileF9Menu2 = new Terminal.Gui.MenuBarItem();
            fileF9Menu2.Title = "_File(F9)";
            var editMeMenuItem2 = new Terminal.Gui.MenuItem();
            editMeMenuItem2.Title = "Edit Me";
            editMeMenuItem2.Data = "editMeMenuItem2";
            fileF9Menu2.Children = new Terminal.Gui.MenuItem[] {
                  editMeMenuItem2};
            menuBar2.Menus = new Terminal.Gui.MenuBarItem[] {
                  fileF9Menu2};
            w.Add(menuBar2);


            var menuBars = w.Subviews.OfType<MenuBar>().ToArray();
            Assert.That(menuBars, Has.Length.EqualTo(2));

            Assert.That(menuBars[0].Width, Is.EqualTo(Dim.Fill(0)));
            Assert.That(menuBars[1].Width, Is.EqualTo(Dim.Fill(0)));
        }

    }
}
